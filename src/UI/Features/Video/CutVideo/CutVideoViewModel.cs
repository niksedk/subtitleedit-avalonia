using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Video.CutVideo;

public partial class CutVideoViewModel : ObservableObject
{
    [ObservableProperty] private string _videoFileName;
    [ObservableProperty] private string _videoFileSize;
    [ObservableProperty] private int _videoWidth;
    [ObservableProperty] private int _videoHeight;
    [ObservableProperty] private ObservableCollection<double> _frameRates;
    [ObservableProperty] private double _selectedFrameRate;
    [ObservableProperty] private ObservableCollection<string> _videoExtensions;
    [ObservableProperty] private string _selectedVideoExtension;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private ObservableCollection<BurnInJobItem> _jobItems;
    [ObservableProperty] private BurnInJobItem? _selectedJobItem;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _useSourceResolution;
    [ObservableProperty] private bool _isAudioVisualizerVisible;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _segments;
    [ObservableProperty] private SubtitleLineViewModel _selectedSegment;
    [ObservableProperty] private int _selectedSegmentIndex;
    [ObservableProperty] private ObservableCollection<CutTypeDisplay> _cutTypes;
    [ObservableProperty] private CutTypeDisplay _selectedCutType;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public VideoPlayerControl VideoPlayer { get; internal set; }
    public AudioVisualizer AudioVisualizer { get; internal set; }
    public DataGrid SegmentGrid { get; internal set; }

    private Subtitle _subtitle = new();
    private bool _loading = true;
    private readonly StringBuilder _log;
    private static readonly Regex FrameFinderRegex = new(@"[Ff]rame=\s*\d+", RegexOptions.Compiled);
    private long _startTicks;
    private long _processedFrames;
    private Process? _ffmpegProcess;
    private Process? _ffmpegListKeyFramesProcess;
    private readonly Timer _timerGenerate;
    private bool _doAbort;
    private int _jobItemIndex = -1;
    private SubtitleFormat? _subtitleFormat;
    private string _inputVideoFileName;
    private bool _updateAudioVisualizer;
    private DispatcherTimer _positionTimer = new DispatcherTimer();

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;
    private readonly IFileHelper _fileHelper;
    private readonly IInsertService _insertService;

    public CutVideoViewModel(IFolderHelper folderHelper, IFileHelper fileHelper, IWindowService windowService, IInsertService insertService)
    {
        _folderHelper = folderHelper;
        _fileHelper = fileHelper;
        _windowService = windowService;
        _insertService = insertService;

        VideoWidth = 1920;
        VideoHeight = 1080;

        FrameRates = new ObservableCollection<double> { 23.976, 24, 25, 29.97, 30, 50, 59.94, 60 };
        SelectedFrameRate = FrameRates[0];

        VideoExtensions = new ObservableCollection<string>
        {
            ".mkv",
            ".mp4",
            ".webm",
        };
        SelectedVideoExtension = VideoExtensions[0];

        CutTypes = new ObservableCollection<CutTypeDisplay>(CutTypeDisplay.GetCutTypes());
        SelectedCutType = CutTypes[0];

        JobItems = new ObservableCollection<BurnInJobItem>();
        VideoPlayer = new VideoPlayerControl(new VideoPlayerInstanceNone());
        AudioVisualizer = new AudioVisualizer();
        SegmentGrid = new DataGrid();
        Segments = new ObservableCollection<SubtitleLineViewModel>();
        VideoFileName = string.Empty;
        VideoFileSize = string.Empty;
        ProgressText = string.Empty;
        SelectedSegment = new SubtitleLineViewModel();

        _log = new StringBuilder();
        _timerGenerate = new();
        _timerGenerate.Elapsed += TimerGenerateElapsed;
        _timerGenerate.Interval = 100;

        _loading = false;
        _inputVideoFileName = string.Empty;
        LoadSettings();
    }

    public void Initialize(string videoFileName, WavePeakData? wavePeakData)
    {
        VideoFileName = videoFileName;
        _inputVideoFileName = videoFileName;

        _ffmpegListKeyFramesProcess = FfmpegGenerator.ListKeyFrames(videoFileName, OutputHandlerKeyFrames);

#pragma warning disable CA1416 // Validate platform compatibility
        var startResult = _ffmpegListKeyFramesProcess.Start();
        if (!startResult)
        {
            SeLogger.Error("Failed to start ffmpeg process for listing key frames: " + _ffmpegListKeyFramesProcess.StartInfo.FileName + " " + _ffmpegListKeyFramesProcess.StartInfo.Arguments);
            return;
        }
#pragma warning restore CA1416 // Validate platform compatibility

        _ffmpegListKeyFramesProcess.BeginOutputReadLine();
        _ffmpegListKeyFramesProcess.BeginErrorReadLine();

        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(videoFileName))
            {
                _ = VideoPlayer.Open(videoFileName);
            }

            if (wavePeakData != null)
            {
                AudioVisualizer.WavePeaks = wavePeakData;
                IsAudioVisualizerVisible = true;
            }
            _updateAudioVisualizer = true;
        });
    }

    private void StartTitleTimer()
    {
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _positionTimer.Tick += (s, e) =>
        {
            UpdateAudioVisualizer(VideoPlayer.VideoPlayerInstance, AudioVisualizer, SelectedSegmentIndex);

            if (_updateAudioVisualizer)
            {
                AudioVisualizer.InvalidateVisual();
                _updateAudioVisualizer = false;
            }
        };

        _positionTimer.Start();
    }

    private void UpdateAudioVisualizer(
        IVideoPlayerInstance vp,
        AudioVisualizer av,
        int selectedParagraphIndex)
    {
        SubtitleLineViewModel? selectedParagraph = selectedParagraphIndex < 0
            ? null
            : (Segments.Count == 0 ? null : Segments[selectedParagraphIndex]);

        var subtitle = new ObservableCollection<SubtitleLineViewModel>();
        var orderedList = Segments.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
        var firstSelectedIndex = -1;
        for (var i = 0; i < orderedList.Count; i++)
        {
            var dp = orderedList[i];
            subtitle.Add(dp);
        }

        var mediaPlayerSeconds = vp.Position;
        var startPos = mediaPlayerSeconds - 0.01;
        if (startPos < 0)
        {
            startPos = 0;
        }

        av.CurrentVideoPositionSeconds = vp.Position;
        var isPlaying = vp.IsPlaying;

        var selectedSubtitles = new List<SubtitleLineViewModel>
        {
            selectedParagraph ??  new  SubtitleLineViewModel(),
        };

        if ((isPlaying || !av.IsScrolling) && (mediaPlayerSeconds > av.EndPositionSeconds ||
                                               mediaPlayerSeconds < av.StartPositionSeconds))
        {
            av.SetPosition(startPos, subtitle, mediaPlayerSeconds, 0,
                selectedSubtitles);
        }
        else
        {
            av.SetPosition(av.StartPositionSeconds, subtitle, mediaPlayerSeconds, firstSelectedIndex,
                selectedSubtitles);
        }
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

            if (double.TryParse(ptsValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
            {
                AudioVisualizer.ShotChanges.Add(seconds);
                _updateAudioVisualizer = true;
            }
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
        string arguments;

        if (SelectedCutType.CutType == CutType.MergeSegments)
        {
            arguments = FfmpegGenerator.GetMergeSegmentsParameters(jobItem.InputVideoFileName, jobItem.OutputVideoFileName, Segments.ToList());
        }
        else
        { 
            arguments = FfmpegGenerator.GetRemoveSegmentsParameters(jobItem.InputVideoFileName, jobItem.OutputVideoFileName, Segments.ToList());
        }

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
            ProgressValue = (double)_processedFrames * 100.0 / JobItems[_jobItemIndex].TotalFrames;
        }
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

        var jobItem = new BurnInJobItem(string.Empty, VideoWidth, VideoHeight)
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
        var fileNames = await _fileHelper.PickOpenSubtitleFiles(Window!, Se.Language.General.OpenSubtitles);
        if (fileNames.Length == 0)
        {
            return;
        }

        foreach (var fileName in fileNames)
        {
            var videoFileName = string.Empty;
            var jobItem = new BurnInJobItem(videoFileName, VideoWidth, VideoHeight);
            jobItem.AddSubtitleFileName(fileName);
            Dispatcher.UIThread.Invoke(() => { JobItems.Add(jobItem); });
        }
    }

    [RelayCommand]
    private async Task OpenOutputFolder()
    {
        await _folderHelper.OpenFolder(Window!, Se.Settings.Video.BurnIn.OutputFolder);
    }

    [RelayCommand]
    private void Remove()
    {
        if (SelectedJobItem != null)
        {
            var idx = JobItems.IndexOf(SelectedJobItem);
            JobItems.Remove(SelectedJobItem);
        }
    }

    [RelayCommand]
    private void Clear()
    {
        JobItems.Clear();
    }

    [RelayCommand]
    private async Task PickVideoFile()
    {
        if (SelectedJobItem == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenVideoFile(Window!, "Open video file");
        if (!string.IsNullOrEmpty(fileName))
        {
            SelectedJobItem.InputVideoFileName = fileName;
            SelectedJobItem.InputVideoFileName = Path.GetFileName(fileName);
        }
    }

    [RelayCommand]
    private async Task BrowseResolution()
    {
        var result =
            await _windowService
                .ShowDialogAsync<BurnInResolutionPickerWindow, BurnInResolutionPickerViewModel>(Window!);
        if (!result.OkPressed || result.SelectedResolution == null)
        {
            return;
        }

        if (result.SelectedResolution.ItemType == ResolutionItemType.PickResolution)
        {
            var videoFileName = await _fileHelper.PickOpenVideoFile(Window!, "Open video file");
            if (string.IsNullOrWhiteSpace(videoFileName))
            {
                return;
            }

            var mediaInfo = FfmpegMediaInfo.Parse(videoFileName);
            VideoWidth = mediaInfo.Dimension.Width;
            VideoHeight = mediaInfo.Dimension.Height;
            UseSourceResolution = false;
        }
        else if (result.SelectedResolution.ItemType == ResolutionItemType.UseSource)
        {
            UseSourceResolution = true;
        }
        else if (result.SelectedResolution.ItemType == ResolutionItemType.Resolution)
        {
            UseSourceResolution = false;
            VideoWidth = result.SelectedResolution.Width;
            VideoHeight = result.SelectedResolution.Height;
        }

        SaveSettings();
    }

    [RelayCommand]
    private async Task Generate()
    {
        if (Segments.Count == 0)
        {
            await MessageBox.Show(
                Window!,
                "No segments added",
                $"Add one or more segments - e.g. via the waveform",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

        var outputVideoFileName = Path.ChangeExtension(VideoFileName, SelectedVideoExtension);
        outputVideoFileName = await _fileHelper.PickSaveVideoFile(Window!, SelectedVideoExtension, outputVideoFileName, Se.Language.Video.SaveVideoAsTitle);
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
        var settings = Se.Settings.Video;
        SelectedCutType = CutTypes.FirstOrDefault(ct => ct.CutType.ToString() == settings.CutType) ?? CutTypes[0];
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Video;
        settings.CutType = SelectedCutType.CutType.ToString();
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
            return;
        }

        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void AudioVisualizerPositionChanged(object sender, AudioVisualizer.PositionEventArgs e)
    {
        VideoPlayer.Position = e.PositionInSeconds;
        _updateAudioVisualizer = true;
    }

    internal void OnClosing()
    {
        _positionTimer.Stop();

        VideoPlayer.VideoPlayerInstance.Close();

        if (_ffmpegListKeyFramesProcess != null && !_ffmpegListKeyFramesProcess.HasExited)
        {
            try
            {
                _ffmpegListKeyFramesProcess.Kill(true);
            }
            catch 
            {
                // ignore
            }
        }
    }

    internal void OnLoaded()
    {
        StartTitleTimer();
        _updateAudioVisualizer = true;
    }

    internal void AudioVisualizerOnNewSelectionInsert(object sender, ParagraphEventArgs e)
    {
        var index = _insertService.InsertInCorrectPosition(Segments, e.Paragraph);
        SelectAndScrollToRow(index);
        Renumber();
        _updateAudioVisualizer = true;
    }

    private void Renumber()
    {
        for (var index = 0; index < Segments.Count; index++)
        {
            Segments[index].Number = index + 1;
        }
    }

    private void SelectAndScrollToRow(int index)
    {
        if (index < 0 || index >= Segments.Count)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            SegmentGrid.SelectedIndex = index;
            SegmentGrid.ScrollIntoView(SegmentGrid.SelectedItem, null);
        }, DispatcherPriority.Background);
    }
}