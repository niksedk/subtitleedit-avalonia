using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Video.BlankVideo;

public partial class BlankVideoViewModel : ObservableObject
{
    [ObservableProperty] private string _videoFileName;
    [ObservableProperty] private string _videoFileSize;
    [ObservableProperty] private int _videoWidth;
    [ObservableProperty] private int _videoHeight;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _useSourceResolution;
    [ObservableProperty] private ObservableCollection<double> _frameRates;
    [ObservableProperty] private double _selectedFrameRate;
    [ObservableProperty] private int _durationMinutes;
    [ObservableProperty] private bool _useCheckedImage;
    [ObservableProperty] private bool _useSolidColor;
    [ObservableProperty] private bool _useBackgroundImage;
    [ObservableProperty] private string _backgroundImageFileName;
    [ObservableProperty] private bool _generateTimeCodes;
    [ObservableProperty] private ObservableCollection<BurnInJobItem> _jobItems;
    [ObservableProperty] private BurnInJobItem? _selectedJobItem;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private Subtitle _subtitle = new();
    private bool _loading = true;
    private readonly StringBuilder _log;
    private static readonly Regex FrameFinderRegex = new(@"[Ff]rame=\s*\d+", RegexOptions.Compiled);
    private long _startTicks;
    private long _processedFrames;
    private Process? _ffmpegProcess;
    private readonly Timer _timerAnalyze;
    private readonly Timer _timerGenerate;
    private bool _doAbort;
    private int _jobItemIndex = -1;
    private SubtitleFormat? _subtitleFormat;
    private string _inputVideoFileName;

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;
    private readonly IFileHelper _fileHelper;

    public BlankVideoViewModel(IFolderHelper folderHelper, IFileHelper fileHelper,
        IWindowService windowService)
    {
        _folderHelper = folderHelper;
        _fileHelper = fileHelper;
        _windowService = windowService;

        VideoWidth = 1920;
        VideoHeight = 1080;

        FrameRates = new ObservableCollection<double> { 23.976, 24, 25, 29.97, 30, 50, 59.94, 60 };
        SelectedFrameRate = FrameRates[0];

        VideoFileName = string.Empty;
        VideoFileSize = string.Empty;
        ProgressText = string.Empty;
        BackgroundImageFileName = string.Empty;
        JobItems = new ObservableCollection<BurnInJobItem>();

        _log = new StringBuilder();
        _timerGenerate = new();
        _timerGenerate.Elapsed += TimerGenerateElapsed;
        _timerGenerate.Interval = 100;

        _timerAnalyze = new();
        _timerAnalyze.Elapsed += TimerAnalyzeElapsed;
        _timerAnalyze.Interval = 100;

        _loading = false;
        _inputVideoFileName = string.Empty;
        LoadSettings();
        UpdateOutputProperties();
    }

    public void Initialize(string videoFileName, Subtitle subtitle, SubtitleFormat subtitleFormat)
    {
        VideoFileName = videoFileName;
        _inputVideoFileName = videoFileName;
        _subtitle = new Subtitle(subtitle, false);
        _subtitleFormat = subtitleFormat;
    }

    private void TimerAnalyzeElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_ffmpegProcess == null)
        {
            return;
        }

        if (_doAbort)
        {
            _timerAnalyze.Stop();
#pragma warning disable CA1416
            _ffmpegProcess.Kill(true);
#pragma warning restore CA1416

            IsGenerating = false;
            return;
        }

        if (!_ffmpegProcess.HasExited)
        {
            //var percentage = (int)Math.Round((double)_processedFrames / JobItems[_jobItemIndex].TotalFrames * 100.0,
            //    MidpointRounding.AwayFromZero);
            //percentage = Math.Clamp(percentage, 0, 100);

            //var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
            //var msPerFrame = (float)durationMs / _processedFrames;
            //var estimatedTotalMs = msPerFrame * JobItems[_jobItemIndex].TotalFrames;
            //var estimatedLeft = ProgressHelper.ToProgressTime(estimatedTotalMs - durationMs);

            //if (JobItems.Count == 1)
            //{
            //    ProgressText = $"Analyzing video... {percentage}%     {estimatedLeft}";
            //}
            //else
            //{
            //    ProgressText =
            //        $"Analyzing video {_jobItemIndex + 1}/{JobItems.Count}... {percentage}%     {estimatedLeft}";
            //}

            return;
        }

        _timerAnalyze.Stop();

        var jobItem = JobItems[_jobItemIndex];
        _ffmpegProcess = GetFfmpegProcess(jobItem);
#pragma warning disable CA1416 // Validate platform compatibility
        _ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        _ffmpegProcess.BeginOutputReadLine();
        _ffmpegProcess.BeginErrorReadLine();

        _timerGenerate.Start();
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
                ProgressText =
                    $"Generating video {_jobItemIndex + 1}/{JobItems.Count}... {percentage}%     {estimatedLeft}";
            }

            return;
        }

        _timerGenerate.Stop();
        ProgressValue = 100;
        ProgressText = string.Empty;

        var jobItem = JobItems[_jobItemIndex];

        if (!File.Exists(jobItem.OutputVideoFileName))
        {
            SeLogger.WhisperInfo("Output video file not found: " + jobItem.OutputVideoFileName + Environment.NewLine +
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
                await _folderHelper.OpenFolder(Window!, jobItem.OutputVideoFileName);
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

    private bool RunEncoding(BurnInJobItem jobItem)
    {
        _ffmpegProcess = GetFfmpegProcess(jobItem);
#pragma warning disable CA1416 // Validate platform compatibility
        _ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        _ffmpegProcess.BeginOutputReadLine();
        _ffmpegProcess.BeginErrorReadLine();

        return true;
    }

    private Process GetFfmpegProcess(BurnInJobItem jobItem)
    {
        var totalMs = _subtitle.Paragraphs.Max(p => p.EndTime.TotalMilliseconds);
        var ts = TimeSpan.FromMilliseconds(totalMs + 2000);
        var timeCode = string.Format($"{ts.Hours:00}\\\\:{ts.Minutes:00}\\\\:{ts.Seconds:00}");

        return VideoPreviewGenerator.GenerateTransparentVideoFile(
            jobItem.AssaSubtitleFileName,
            jobItem.OutputVideoFileName,
            jobItem.Width,
            jobItem.Height,
            SelectedFrameRate.ToString(CultureInfo.InvariantCulture),
            timeCode,
            OutputHandler);
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

    private ObservableCollection<BurnInJobItem> GetCurrentVideoAsJobItems()
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
            OutputVideoFileName = MakeOutputFileName(_subtitle.FileName),
        };
        jobItem.AddSubtitleFileName(subtitleFileName);

        return new ObservableCollection<BurnInJobItem>(new[] { jobItem });
    }

    private string MakeOutputFileName(string videoFileName)
    {
        var nameNoExt = Path.GetFileNameWithoutExtension(videoFileName);
        var ext = ".mkv"; // SelectedVideoExtension;
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
    private async Task OutputProperties()
    {
        await _windowService.ShowDialogAsync<BurnInSettingsWindow, BurnInSettingsViewModel>(Window!);
        UpdateOutputProperties();
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
        JobItems = GetCurrentVideoAsJobItems();

        if (JobItems.Count == 0)
        {
            return;
        }

        // check that all jobs have subtitles
        foreach (var jobItem in JobItems)
        {
            if (string.IsNullOrWhiteSpace(jobItem.SubtitleFileName))
            {
                await MessageBox.Show(Window!,
                    "Missing subtitle",
                    "Please add a subtitle to all batch items",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }
        }

        _doAbort = false;
        _log.Clear();
        IsGenerating = true;
        _processedFrames = 0;
        ProgressValue = 0;
        SaveSettings();

        InitAndStartJobItem(0);
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
        jobItem.Status = Se.Language.General.Generating;
        jobItem.OutputVideoFileName = MakeOutputFileName(jobItem.InputVideoFileName);

        var result = RunEncoding(jobItem);
        if (result)
        {
            _timerGenerate.Start();
        }
    }

    private void LoadSettings()
    {
        var settings = Se.Settings.Video.BurnIn;
        UseSourceResolution = settings.UseSourceResolution;
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Video.BurnIn;
        settings.UseSourceResolution = UseSourceResolution;

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

    private void UpdateOutputProperties()
    {
        if (Se.Settings.Video.Transparent.UseOutputFolder &&
            string.IsNullOrWhiteSpace(Se.Settings.Video.Transparent.OutputFolder))
        {
            Se.Settings.Video.Transparent.UseOutputFolder = true;
        }
    }
}