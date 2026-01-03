using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;

public partial class EmbeddedSubtitlesEditViewModel : ObservableObject
{
    [ObservableProperty] private string _videoFileName;
    [ObservableProperty] private ObservableCollection<EmbeddedTrack> _tracks;
    [ObservableProperty] private EmbeddedTrack? _selectedTrck;
    [ObservableProperty] private ObservableCollection<string> _videoExtensions;
    [ObservableProperty] private string _selectedVideoExtension;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private ObservableCollection<BurnInJobItem> _jobItems;
    [ObservableProperty] private BurnInJobItem? _selectedJobItem;
    [ObservableProperty] private bool _isGenerating;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public DataGrid TracksGrid { get; internal set; }

    private Subtitle _subtitle = new();
    private readonly StringBuilder _log;
    private long _startTicks;
    private long _processedFrames;
    private Process? _ffmpegProcess;
    private Process? _ffmpegListKeyFramesProcess;
    private readonly Timer _timerGenerate;
    private bool _doAbort;
    private int _jobItemIndex = -1;
    private SubtitleFormat? _subtitleFormat;
    private string _inputVideoFileName;
    private DispatcherTimer _positionTimer = new DispatcherTimer();
    private string _importFileName;
    private Subtitle _currentSubtitle;
    private FfmpegMediaInfo2? _mediaInfo;

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;
    private readonly IFileHelper _fileHelper;
    private readonly IInsertService _insertService;

    public EmbeddedSubtitlesEditViewModel(IFolderHelper folderHelper, IFileHelper fileHelper, IWindowService windowService, IInsertService insertService)
    {
        _folderHelper = folderHelper;
        _fileHelper = fileHelper;
        _windowService = windowService;
        _insertService = insertService;

        VideoExtensions = new ObservableCollection<string>
        {
            ".mkv",
            ".mp4",
            ".webm",
        };
        SelectedVideoExtension = VideoExtensions[0];

        Tracks = new ObservableCollection<EmbeddedTrack>();
        VideoFileName = string.Empty;
        ProgressText = string.Empty;
        JobItems = new ObservableCollection<BurnInJobItem>();
        TracksGrid = new DataGrid();

        _log = new StringBuilder();
        _timerGenerate = new();
        _timerGenerate.Elapsed += TimerGenerateElapsed;
        _timerGenerate.Interval = 100;
        _importFileName = string.Empty;
        _inputVideoFileName = string.Empty;
        _currentSubtitle = new Subtitle();
        UpdateSelection();
        LoadSettings();
    }

    public void Initialize(string videoFileName, Subtitle subtitle, SubtitleFormat subtitleFormat, FfmpegMediaInfo2? mediaInfo)
    {
        VideoFileName = videoFileName;
        _inputVideoFileName = videoFileName;
        _currentSubtitle = subtitle;
        _subtitleFormat = subtitleFormat;
        _mediaInfo = mediaInfo;
    }

    private void StartTitleTimer()
    {
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _positionTimer.Tick += (s, e) =>
        {
        };

        _positionTimer.Start();
    }

    private void OutputHandlerKeyFrames(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data))
        {
            return;
        }

        const string marker = "pts_time:";
        var idx = outLine.Data.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            var afterMarker = outLine.Data.Substring(idx + marker.Length);
            var endIdx = afterMarker.IndexOf(' ');
            var ptsValue = endIdx > 0 ? afterMarker.Substring(0, endIdx) : afterMarker;
        }
    }

    private void TimerGenerateElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_ffmpegProcess == null)
        {
            return;
        }

        if (_doAbort)
        {
            _timerGenerate.Stop();
#pragma warning disable CA1416
            _ffmpegProcess.Kill(true);
#pragma warning restore CA1416

            IsGenerating = false;
            return;
        }

        if (!_ffmpegProcess.HasExited)
        {
            var percentage = (int)Math.Round((double)_processedFrames / JobItems[_jobItemIndex].TotalFrames * 100.0,
                MidpointRounding.AwayFromZero);
            percentage = Math.Clamp(percentage, 0, 100);

            var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
            var msPerFrame = (float)durationMs / _processedFrames;
            var estimatedTotalMs = msPerFrame * JobItems[_jobItemIndex].TotalFrames;
            var estimatedLeft = ProgressHelper.ToProgressTime(estimatedTotalMs - durationMs);

            if (JobItems.Count == 1)
            {
                ProgressText = $"Generating video... {percentage}%     {estimatedLeft}";
            }
            else
            {
                ProgressText = $"Generating video {_jobItemIndex + 1}/{JobItems.Count}... {percentage}%     {estimatedLeft}";
            }

            return;
        }

        _timerGenerate.Stop();
        ProgressValue = 100;
        ProgressText = string.Empty;

        var jobItem = JobItems[_jobItemIndex];

        if (!File.Exists(jobItem.OutputVideoFileName))
        {
            SeLogger.Error("Output video file not found: " + jobItem.OutputVideoFileName + Environment.NewLine +
                                 "ffmpeg: " + _ffmpegProcess.StartInfo.FileName + Environment.NewLine +
                                 "Parameters: " + _ffmpegProcess.StartInfo.Arguments + Environment.NewLine +
                                 "OS: " + Environment.OSVersion + Environment.NewLine +
                                 "64-bit: " + Environment.Is64BitOperatingSystem + Environment.NewLine +
                                 "ffmpeg exit code: " + _ffmpegProcess.ExitCode + Environment.NewLine +
                                 "ffmpeg log: " + _log);

            Dispatcher.UIThread.Invoke(async () =>
            {
                await MessageBox.Show(Window!,
                    "Unable to generate video",
                    "Output video file not generated: " + jobItem.OutputVideoFileName + Environment.NewLine +
                    "Parameters: " + _ffmpegProcess.StartInfo.Arguments,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                IsGenerating = true;
                ProgressValue = 0;
            });

            return;
        }

        JobItems[_jobItemIndex].Status = Se.Language.General.Done;

        Dispatcher.UIThread.Invoke(async () =>
        {
            ProgressValue = 0;

            if (_jobItemIndex < JobItems.Count - 1)
            {
                InitAndStartJobItem(_jobItemIndex + 1);
                return;
            }

            IsGenerating = false;

            if (JobItems.Count == 1)
            {
                await _folderHelper.OpenFolderWithFileSelected(Window!, jobItem.OutputVideoFileName);
            }
            else
            {
                var sb = new StringBuilder($"Generated files ({JobItems.Count}):" + Environment.NewLine +
                                           Environment.NewLine);
                foreach (var item in JobItems)
                {
                    sb.AppendLine($"{item.OutputVideoFileName} ==> {item.Status}");
                }

                await MessageBox.Show(Window!,
                    "Generating done",
                    sb.ToString(),
                    MessageBoxButtons.OK);
            }
        });
    }

    private void InitAndStartJobItem(int index)
    {
        _startTicks = DateTime.UtcNow.Ticks;
        _jobItemIndex = index;
        var jobItem = JobItems[index];
        var mediaInfo = FfmpegMediaInfo.Parse(jobItem.InputVideoFileName);
        jobItem.TotalFrames = mediaInfo.GetTotalFrames();
        jobItem.TotalSeconds = mediaInfo.Duration.TotalSeconds;
        jobItem.Width = mediaInfo.Dimension.Width;
        jobItem.Height = mediaInfo.Dimension.Height;
        jobItem.UseTargetFileSize = false;
        jobItem.Status = Se.Language.General.Generating;

        var result = RunEncoding(jobItem);
        if (result)
        {
            _timerGenerate.Start();
        }
    }

    private bool RunEncoding(BurnInJobItem jobItem)
    {
        string arguments = "";

        //if (SelectedCutType.CutType == CutType.MergeSegments)
        //{
        //    arguments = FfmpegGenerator.GetMergeSegmentsParameters(jobItem.InputVideoFileName, jobItem.OutputVideoFileName, Segments.ToList());
        //}
        //else
        //{
        //    arguments = FfmpegGenerator.GetRemoveSegmentsParameters(jobItem.InputVideoFileName, jobItem.OutputVideoFileName, Segments.ToList());
        //}

        _ffmpegProcess = FfmpegGenerator.GetProcess(arguments, OutputHandler);
#pragma warning disable CA1416 // Validate platform compatibility
        _ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        _ffmpegProcess.BeginOutputReadLine();
        _ffmpegProcess.BeginErrorReadLine();

        return true;
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data))
        {
            return;
        }

        _log?.AppendLine(outLine.Data);
    }

    private ObservableCollection<BurnInJobItem> GetCurrentVideoAsJobItems(string outputVideoFileName)
    {
        var subtitle = new Subtitle(_subtitle);

        var srt = new SubRip();
        var subtitleFileName = Path.Combine(Path.GetTempFileName() + srt.Extension);
        if (_subtitleFormat is { Name: AdvancedSubStationAlpha.NameOfFormat })
        {
            var assa = new AdvancedSubStationAlpha();
            subtitleFileName = Path.Combine(Path.GetTempFileName() + assa.Extension);
            File.WriteAllText(subtitleFileName, assa.ToText(subtitle, string.Empty));
        }
        else
        {
            File.WriteAllText(subtitleFileName, srt.ToText(subtitle, string.Empty));
        }

        var jobItem = new BurnInJobItem(string.Empty, 1920, 1080) //TODO: use source video width/height
        {
            InputVideoFileName = VideoFileName,
            OutputVideoFileName = outputVideoFileName,
        };
        jobItem.AddSubtitleFileName(subtitleFileName);

        return new ObservableCollection<BurnInJobItem>(new[] { jobItem });
    }

    private string MakeOutputFileName(string videoFileName)
    {
        var nameNoExt = Path.GetFileNameWithoutExtension(videoFileName);
        var ext = SelectedVideoExtension;
        var suffix = Se.Settings.Video.BurnIn.BurnInSuffix;
        var fileName = Path.Combine(Path.GetDirectoryName(videoFileName)!, nameNoExt + suffix + ext);
        if (Se.Settings.Video.BurnIn.UseOutputFolder &&
            !string.IsNullOrEmpty(Se.Settings.Video.BurnIn.OutputFolder) &&
            Directory.Exists(Se.Settings.Video.BurnIn.OutputFolder))
        {
            fileName = Path.Combine(Se.Settings.Video.BurnIn.OutputFolder, nameNoExt + suffix + ext);
        }

        var i = 2;
        while (File.Exists(fileName))
        {
            fileName = Path.Combine(Se.Settings.Video.BurnIn.OutputFolder, $"{nameNoExt}{suffix}_{i}{ext}");
            i++;
        }

        return fileName;
    }

    public static int CalculateFontSize(int videoWidth, int videoHeight, double factor, int minSize = 8,
        int maxSize = 2000)
    {
        factor = Math.Clamp(factor, 0, 1);

        // Calculate the diagonal resolution
        var diagonalResolution = Math.Sqrt(videoWidth * videoWidth + videoHeight * videoHeight);

        // Calculate base size (when factor is 0.5)
        var baseSize = diagonalResolution * 0.019; // around 2% of diagonal as base size

        // Apply logarithmic scaling
        var scaleFactor = Math.Pow(maxSize / baseSize, 2 * (factor - 0.5));
        var fontSize = (int)Math.Round(baseSize * scaleFactor);

        // Clamp the font size between minSize and maxSize
        return Math.Clamp(fontSize, minSize, maxSize);
    }

    [RelayCommand]
    private async Task Add()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, "title", "Advanced Sub Station Alpha", "ass", "SubRip", "srt");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle.Paragraphs.Count == 0)
        {
            await MessageBox.Show(
                Window,
                "No subtitles found",
                "The selected subtitle file does not contain any subtitles.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var embeddedTrack = new EmbeddedTrack
        {
            Format = Path.GetExtension(fileName).TrimStart('.').ToUpperInvariant(),
            LanguageOrTitle = Path.GetFileNameWithoutExtension(fileName),
            Name = Path.GetFileName(fileName),
            FileName = fileName,
            New = true,
        };
        Tracks.Add(embeddedTrack);
    }

    [RelayCommand]
    private void AddCurrent()
    {
        if (Window == null || _currentSubtitle == null || _currentSubtitle.Paragraphs.Count == 0 || _subtitleFormat == null)
        {
            return;
        }

        var tempFileName = Path.Combine(Path.GetTempPath(), "EmbeddedSubtitleEdit_" + Guid.NewGuid() + _subtitleFormat.Extension);
        File.WriteAllText(tempFileName, _subtitleFormat.ToText(_currentSubtitle, string.Empty));
        var embeddedTrack = new EmbeddedTrack
        {
            Format = _subtitleFormat.Name,
            LanguageOrTitle = Path.GetFileNameWithoutExtension(tempFileName),
            Name = Path.GetFileName(tempFileName),
            FileName = tempFileName,
            New = true,
        };
    }


    [RelayCommand]
    private void Delete()
    {
        var selectedTrack = SelectedTrck;
        if (selectedTrack != null)
        {
            selectedTrack.Deleted = !selectedTrack.Deleted;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        foreach (var track in Tracks)
        {
            track.Deleted = true;
        }
    }

    [RelayCommand]
    private async Task Edit()
    {
        var selectedTrack = SelectedTrck;
        if (Window == null || selectedTrack == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditEmbeddedTrackWindow, EditEmbeddedTrackViewModel>(Window, vm =>
        {
            vm.Initialize(selectedTrack);
        });
    }

    [RelayCommand]
    private async Task BrowseVideoFile()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenVideoFile(Window, Se.Language.General.OpenVideoFileTitle);
        if (!string.IsNullOrEmpty(fileName))
        {
            VideoFileName = fileName;
            _ = Task.Run(() =>
            {
                var mediaInfo = FfmpegMediaInfo2.Parse(fileName);
                Dispatcher.UIThread.Invoke(() =>
                {
                    Tracks.Clear();
                    var tracks = FindTracks(fileName, mediaInfo);
                    foreach (var track in tracks)
                    {
                        Tracks.Add(track);
                    }
                });
            });
            _mediaInfo = FfmpegMediaInfo2.Parse(fileName);
        }
    }

    [RelayCommand]
    private async Task Generate()
    {
        if (Tracks.Count == 0)
        {
            await MessageBox.Show(
                Window!,
                "No segments added",
                $"Add one or more segments - e.g. via the waveform",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

        var outputVideoFileName = MakeOutputFileName(VideoFileName);
        outputVideoFileName = await _fileHelper.PickSaveFile(Window!, SelectedVideoExtension, outputVideoFileName, Se.Language.Video.SaveVideoAsTitle);
        if (string.IsNullOrEmpty(outputVideoFileName))
        {
            return;
        }

        JobItems = GetCurrentVideoAsJobItems(outputVideoFileName);

        if (JobItems.Count == 0)
        {
            return;
        }

        _doAbort = false;
        _log.Clear();
        IsGenerating = true;
        _processedFrames = 0;
        ProgressValue = 0;
        SaveSettings();

        InitAndStartJobItem(0);
    }

    private void LoadSettings()
    {
    }

    private void SaveSettings()
    {
        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsGenerating)
        {
            _doAbort = true;
            IsGenerating = false;
            return;
        }

        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {

    }


    internal void OnClosing()
    {
        SaveSettings();
        UiUtil.SaveWindowPosition(Window);
    }

    internal void OnLoaded()
    {
        StartTitleTimer();
        UiUtil.RestoreWindowPosition(Window);
        Task.Run(() =>
        {
            var tracks = FindTracks(VideoFileName, _mediaInfo);
            Dispatcher.UIThread.Invoke(() =>
            {
                foreach (var track in tracks)
                {
                    Tracks.Add(track);
                }
            });
        });
    }

    private List<EmbeddedTrack> FindTracks(string videoFileName, FfmpegMediaInfo2? mediaInfo)
    {
        var list = new List<EmbeddedTrack>();

        if (FileUtil.IsMatroskaFileFast(videoFileName))
        {
            var matroskaFile = new MatroskaFile(videoFileName);
            if (matroskaFile.IsValid)
            {
                var tracks = matroskaFile.GetTracks();
                foreach (var track in tracks)
                {
                    if (track.IsSubtitle)
                    {
                        var embeddedTrack = new EmbeddedTrack
                        {
                            Format = track.CodecId,
                            LanguageOrTitle = !string.IsNullOrEmpty(track.Language) ? track.Language : track.Name,
                            Name = track.Name,
                            FileName = string.Empty,
                            Forced = track.IsForced,
                            Default = track.IsDefault,
                        };
                        list.Add(embeddedTrack);
                    }
                }
            }
        }
        else if (videoFileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                 videoFileName.EndsWith(".m4v", StringComparison.OrdinalIgnoreCase) ||
                 videoFileName.EndsWith(".mov", StringComparison.OrdinalIgnoreCase))
        {
            var mp4Parser = new MP4Parser(videoFileName);
            if (mp4Parser.VttcSubtitle != null && mp4Parser.VttcSubtitle.Paragraphs.Count > 0)
            {
                var embeddedTrack = new EmbeddedTrack
                {
                    Format = "WebVTT",
                    Name = string.Empty,
                    FileName = string.Empty,
                };
                list.Add(embeddedTrack);
            }
            foreach (var track in mp4Parser.GetSubtitleTracks())
            {
                var embeddedTrack = new EmbeddedTrack
                {
                    Format = track.Mdia.Name,
                    Name = track.Name,
                    FileName = string.Empty,
                };
                list.Add(embeddedTrack);
            }
        }

        return list;
    }

    internal void AudioVisualizerOnNewSelectionInsert(object sender, ParagraphEventArgs e)
    {
    }


    private void SelectAndScrollToRow(int index)
    {
        //if (index < 0 || index >= Segments.Count)
        //{
        //    return;
        //}

        //Dispatcher.UIThread.Post(() =>
        //{
        //    SegmentGrid.SelectedIndex = index;
        //    SegmentGrid.ScrollIntoView(SegmentGrid.SelectedItem, null);
        //    UpdateSelection();
        //}, DispatcherPriority.Background);
    }

    internal void TracksGridChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateSelection();
    }

    private void UpdateSelection()
    {
    }

    internal void OnTracksGridKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            Delete();
            e.Handled = true;
        }
        else if (e.Key == Key.Insert)
        {
            _ = Add();
            e.Handled = true;
        }
    }
}