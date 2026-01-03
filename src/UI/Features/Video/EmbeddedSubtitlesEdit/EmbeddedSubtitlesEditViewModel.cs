using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;

public partial class EmbeddedSubtitlesEditViewModel : ObservableObject
{
    [ObservableProperty] private string _videoFileName;
    [ObservableProperty] private ObservableCollection<EmbeddedTrack> _tracks;
    [ObservableProperty] private EmbeddedTrack? _selectedTrck;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private bool _isGenerating;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public DataGrid TracksGrid { get; internal set; }

    private Subtitle _subtitle = new();
    private readonly StringBuilder _log;
    private long _startTicks;
    private long _processedFrames;
    private Process? _ffmpegProcess;
    private readonly Timer _timerGenerate;
    private bool _doAbort;
    private SubtitleFormat? _subtitleFormat;
    private DispatcherTimer _positionTimer = new DispatcherTimer();
    private Subtitle _currentSubtitle;
    private FfmpegMediaInfo2? _mediaInfo;
    private List<EmbeddedTrack> _originalTracks;
    private long _totalFrames = 0;
    private string _outputFileName;
    private static readonly Regex FrameFinderRegex = new(@"[Ff]rame=\s*\d+", RegexOptions.Compiled);

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;
    private readonly IFileHelper _fileHelper;

    public EmbeddedSubtitlesEditViewModel(IFolderHelper folderHelper, IFileHelper fileHelper, IWindowService windowService)
    {
        _folderHelper = folderHelper;
        _fileHelper = fileHelper;
        _windowService = windowService;

        Tracks = new ObservableCollection<EmbeddedTrack>();
        VideoFileName = string.Empty;
        ProgressText = string.Empty;
        TracksGrid = new DataGrid();

        _log = new StringBuilder();
        _timerGenerate = new();
        _timerGenerate.Elapsed += TimerGenerateElapsed;
        _timerGenerate.Interval = 100;
        _currentSubtitle = new Subtitle();
        _originalTracks = new List<EmbeddedTrack>();
        _outputFileName = string.Empty;
    }

    public void Initialize(string videoFileName, Subtitle subtitle, SubtitleFormat subtitleFormat, FfmpegMediaInfo2? mediaInfo)
    {
        VideoFileName = videoFileName;
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
            var percentage = (int)Math.Round((double)_processedFrames / _totalFrames * 100.0,
                MidpointRounding.AwayFromZero);
            percentage = Math.Clamp(percentage, 0, 100);

            var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
            var msPerFrame = (float)durationMs / _processedFrames;
            var estimatedTotalMs = msPerFrame * _totalFrames;
            var estimatedLeft = ProgressHelper.ToProgressTime(estimatedTotalMs - durationMs);

            ProgressText = $"Generating video... {percentage}%     {estimatedLeft}";

            return;
        }

        _timerGenerate.Stop();
        ProgressValue = 100;
        ProgressText = string.Empty;

        if (!File.Exists(_outputFileName))
        {
            SeLogger.Error("Output video file not found: " + _outputFileName + Environment.NewLine +
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
                    "Output video file not generated: " + _outputFileName + Environment.NewLine +
                    "Parameters: " + _ffmpegProcess.StartInfo.Arguments,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                IsGenerating = true;
                ProgressValue = 0;
            });

            return;
        }

        Dispatcher.UIThread.Invoke(async () =>
        {
            ProgressValue = 0;
            IsGenerating = false;
            await _folderHelper.OpenFolderWithFileSelected(Window!, _outputFileName);
        });
    }

    private bool RunEncoding()
    {
        var videoFileName = VideoFileName;
        if (string.IsNullOrEmpty(VideoFileName) || !File.Exists(VideoFileName))
        {
            return false;
        }

        string arguments = "";

        if (FileUtil.IsMatroskaFileFast(VideoFileName))
        {
            arguments = FfmpegGenerator.AlterEmbeddedTracksMatroska(Tracks.ToList(), _originalTracks, VideoFileName, _outputFileName);
        }
        else
        {
            return false;
        }

        _startTicks = DateTime.UtcNow.Ticks;
        _ffmpegProcess = FfmpegGenerator.GetProcess(arguments, OutputHandler);
#pragma warning disable CA1416 // Validate platform compatibility
        _ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        _ffmpegProcess.BeginOutputReadLine();
        _ffmpegProcess.BeginErrorReadLine();
        _timerGenerate.Start();

        return true;
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data))
        {
            return;
        }

        _log?.AppendLine(outLine.Data);

        var match = FrameFinderRegex.Match(outLine.Data);
        if (!match.Success)
        {
            return;
        }

        var arr = match.Value.Split('=');
        if (arr.Length != 2)
        {
            return;
        }

        if (long.TryParse(arr[1].Trim(), out var f))
        {
            _processedFrames = f;
            ProgressValue = (double)_processedFrames * 100.0 / _totalFrames;
        }
    }

    private string MakeOutputFileName(string videoFileName)
    {
        var nameNoExt = Path.GetFileNameWithoutExtension(videoFileName);
        var ext = Path.GetExtension(VideoFileName) ?? ".mkv";
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
        Tracks.Add(embeddedTrack);
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

        if (result != null && result.OkPressed)
        {
            selectedTrack.Name = result.Name;
            selectedTrack.LanguageOrTitle = result.TitleOrlanguage;
            selectedTrack.Forced = result.IsForced;

            if (result.IsDefault)
            {
                // unset default on all other tracks
                foreach (var track in Tracks)
                {
                    if (track != selectedTrack)
                    {
                        track.Default = false;
                    }
                }
            }

            selectedTrack.Default = result.IsDefault;
        }
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
                    _originalTracks.Clear();
                    var tracks = FindTracks(fileName, mediaInfo);
                    foreach (var track in tracks)
                    {
                        Tracks.Add(track);
                        _originalTracks.Add(new EmbeddedTrack(track));
                    }
                    SelectAndScrollToRow(0);
                });
            });
            _mediaInfo = FfmpegMediaInfo2.Parse(fileName);
        }
    }

    [RelayCommand]
    private async Task Generate()
    {
        if (_mediaInfo == null || _mediaInfo.Duration == null)
        {
            await MessageBox.Show(
                Window!,
                "Unable to get media info",
                $"Cannot generate video without valid media info",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }   

        if (Tracks.Count == 0)
        {
            await MessageBox.Show(
                Window!,
                "No tracks added",
                $"Add one or more tracks",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

        var outputVideoFileName = MakeOutputFileName(VideoFileName);
        outputVideoFileName = await _fileHelper.PickSaveFile(Window!, Path.GetExtension(VideoFileName) ?? ".mkv", outputVideoFileName, Se.Language.Video.SaveVideoAsTitle);
        if (string.IsNullOrEmpty(outputVideoFileName))
        {
            return;
        }

        _outputFileName = outputVideoFileName;
        _doAbort = false;
        _log.Clear();
        IsGenerating = true;
        _processedFrames = 0;
        ProgressValue = 0;
        _totalFrames = (long)Math.Round((double)_mediaInfo!.FramesRate * _mediaInfo.Duration!.TotalSeconds);

        IsGenerating = RunEncoding();
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
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
    }

    internal void OnClosing()
    {
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
                    _originalTracks.Add(new EmbeddedTrack(track));
                }

                SelectAndScrollToRow(0);
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
                            Number = Tracks.Count,
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
                    Number = Tracks.Count,
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
                    Number = Tracks.Count,
                    Format = track.Mdia.Name,
                    Name = track.Name,
                    FileName = string.Empty,
                };
                list.Add(embeddedTrack);
            }
        }

        return list;
    }

    private void SelectAndScrollToRow(int index)
    {
        if (index < 0 || index >= Tracks.Count)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            TracksGrid.SelectedIndex = index;
            TracksGrid.ScrollIntoView(TracksGrid.SelectedItem, null);
        }, DispatcherPriority.Background);
    }

    internal void TracksGridChanged(object? sender, SelectionChangedEventArgs e)
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