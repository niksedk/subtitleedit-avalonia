using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.EncodingSettings;
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

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public partial class BurnInViewModel : ObservableObject
{
    [ObservableProperty] private string _videoFileName;
    [ObservableProperty] private string _videoFileSize;
    [ObservableProperty] private ObservableCollection<string> _fontNames;
    [ObservableProperty] private string _selectedFontName;
    [ObservableProperty] private double _fontFactor;
    [ObservableProperty] private string _fontFactorText;
    [ObservableProperty] private bool _fontIsBold;
    [ObservableProperty] private decimal _selectedFontOutline;
    [ObservableProperty] private string _fontOutlineText;
    [ObservableProperty] private ObservableCollection<decimal> _fontShadowWidths;
    [ObservableProperty] private decimal _selectedFontShadowWidth;
    [ObservableProperty] private string _fontShadowText;
    [ObservableProperty] private ObservableCollection<FontBoxItem> _fontBoxTypes;
    [ObservableProperty] private FontBoxItem _selectedFontBoxType;
    [ObservableProperty] private Color _fontTextColor;
    [ObservableProperty] private Color _fontBoxColor;
    [ObservableProperty] private Color _fontOutlineColor;
    [ObservableProperty] private Color _fontShadowColor;
    [ObservableProperty] private int _fontMarginHorizontal;
    [ObservableProperty] private int _fontMarginVertical;
    [ObservableProperty] private bool _fontFixRtl;
    [ObservableProperty] private ObservableCollection<AlignmentItem> _fontAlignments;
    [ObservableProperty] private AlignmentItem _selectedFontAlignment;
    [ObservableProperty] private string _fontAssaInfo;
    [ObservableProperty] private int _videoWidth;
    [ObservableProperty] private int _videoHeight;
    [ObservableProperty] private ObservableCollection<VideoEncodingItem> _videoEncodings;
    [ObservableProperty] private VideoEncodingItem _selectedVideoEncoding;
    [ObservableProperty] private ObservableCollection<PixelFormatItem> _videoPixelFormats;
    [ObservableProperty] private PixelFormatItem? _selectedVideoPixelFormat;
    [ObservableProperty] private ObservableCollection<string> _videoPresets;
    [ObservableProperty] private string? _selectedVideoPreset;
    [ObservableProperty] private string _videoPresetText;
    [ObservableProperty] private ObservableCollection<string> _videoCrf;
    [ObservableProperty] private string? _selectedVideoCrf;
    [ObservableProperty] private string _videoCrfText;
    [ObservableProperty] private string _videoCrfHint;
    [ObservableProperty] private ObservableCollection<string> _videoTuneFor;
    [ObservableProperty] private string? _selectedVideoTuneFor;
    [ObservableProperty] private ObservableCollection<string> _videoExtension;
    [ObservableProperty] private int _selectedVideoExtension;
    [ObservableProperty] private ObservableCollection<string> _audioEncodings;
    [ObservableProperty] private string _selectedAudioEncoding;
    [ObservableProperty] private bool _audioIsStereo;
    [ObservableProperty] private ObservableCollection<string> _audioSampleRates;
    [ObservableProperty] private string _selectedAudioSampleRate;
    [ObservableProperty] private ObservableCollection<string> _audioBitRates;
    [ObservableProperty] private string _selectedAudioBitRate;
    [ObservableProperty] private string _outputSourceFolder;
    [ObservableProperty] private bool _useOutputFolderVisible;
    [ObservableProperty] private bool _useSourceFolderVisible;
    [ObservableProperty] private bool _isCutActive;
    [ObservableProperty] private TimeSpan _cutFrom;
    [ObservableProperty] private TimeSpan _cutTo;
    [ObservableProperty] private bool _useTargetFileSize;
    [ObservableProperty] private int _targetFileSize;
    [ObservableProperty] private string _buttonModeText;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private ObservableCollection<BurnInJobItem> _jobItems;
    [ObservableProperty] private BurnInJobItem? _selectedJobItem;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _isBatchMode;

    public BurnInWindow? Window { get; set; }
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
    private FfmpegMediaInfo? _mediaInfo;
    private bool _useSourceResolution;
    private SubtitleFormat? _subtitleFormat;

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;
    private readonly IFileHelper _fileHelper;

    public BurnInViewModel(IFolderHelper folderHelper, IFileHelper fileHelper, IWindowService windowService)
    {
        _folderHelper = folderHelper;
        _fileHelper = fileHelper;
        _windowService = windowService;

        FontNames = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        SelectedFontName = FontNames.FirstOrDefault(p => p == Se.Settings.Video.BurnIn.FontName) ?? FontNames[0];

        // font factors between 0-1
        FontFactor = 0.4;
        FontFactorText = string.Empty;

        VideoPresets = new ObservableCollection<string>();

        FontBoxTypes = new ObservableCollection<FontBoxItem>
        {
            new(FontBoxType.None, Se.Language.General.None),
            new(FontBoxType.OneBox, Se.Language.Video.BurnIn.OneBox),
            new(FontBoxType.BoxPerLine, Se.Language.Video.BurnIn.BoxPerLine),
        };
        SelectedFontBoxType = FontBoxTypes[0];

        FontMarginHorizontal = 10;
        FontMarginVertical = 10;

        FontAlignments = new ObservableCollection<AlignmentItem>(AlignmentItem.Alignments);
        SelectedFontAlignment = AlignmentItem.Alignments[7];

        FontAssaInfo = string.Empty;

        VideoWidth = 1920;
        VideoHeight = 1080;

        AudioEncodings = new ObservableCollection<string>
        {
            "copy",
            "aac",
            "ac3",
            "mp3",
            "opus",
            "vorbis",
        };
        SelectedAudioEncoding = "copy";

        AudioSampleRates = new ObservableCollection<string>
        {
            "44100 Hz",
            "48000 Hz",
            "88200 Hz",
            "96000 Hz",
            "192000 Hz",
        };
        SelectedAudioSampleRate = AudioSampleRates[1];

        AudioBitRates = new ObservableCollection<string>
        {
            "64k",
            "96k",
            "128k",
            "160k",
            "192k",
            "256k",
            "320k",
        };
        SelectedAudioBitRate = AudioBitRates[2];

        VideoEncodings = new ObservableCollection<VideoEncodingItem>(VideoEncodingItem.VideoEncodings);
        SelectedVideoEncoding = VideoEncodings[0];

        JobItems = new ObservableCollection<BurnInJobItem>();

        VideoFileName = string.Empty;
        VideoFileSize = string.Empty;

        _log = new StringBuilder();
        _timerGenerate = new();
        _timerGenerate.Elapsed += TimerGenerateElapsed;
        _timerGenerate.Interval = 100;

        _timerAnalyze = new();
        _timerAnalyze.Elapsed += TimerAnalyzeElapsed;
        _timerAnalyze.Interval = 100;

        LoadSettings();
    }

    public void Initialize(string videoFileName, Subtitle subtitle, SubtitleFormat subtitleFormat)
    {
        VideoFileName = videoFileName;
        _subtitle = subtitle;
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
            return;
        }

        if (!_ffmpegProcess.HasExited)
        {
            var percentage = (int)Math.Round((double)_processedFrames / JobItems[_jobItemIndex].TotalFrames * 100.0, MidpointRounding.AwayFromZero);
            percentage = Math.Clamp(percentage, 0, 100);

            var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
            var msPerFrame = (float)durationMs / _processedFrames;
            var estimatedTotalMs = msPerFrame * JobItems[_jobItemIndex].TotalFrames;
            var estimatedLeft = ProgressHelper.ToProgressTime(estimatedTotalMs - durationMs);

            if (JobItems.Count == 1)
            {
                ProgressText = $"Analyzing video... {percentage}%     {estimatedLeft}";
            }
            else
            {
                ProgressText = $"Analyzing video {_jobItemIndex + 1}/{JobItems.Count}... {percentage}%     {estimatedLeft}";
            }

            return;
        }

        _timerAnalyze.Stop();

        var jobItem = JobItems[_jobItemIndex];
        _ffmpegProcess = GetFfmpegProcess(jobItem, 2);
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
            return;
        }

        if (!_ffmpegProcess.HasExited)
        {
            var percentage = (int)Math.Round((double)_processedFrames / JobItems[_jobItemIndex].TotalFrames * 100.0, MidpointRounding.AwayFromZero);
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

                await MessageBox.Show(Window!,
                    "Generating done",
                    "Number of files generated: " + JobItems.Count,
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
        jobItem.UseTargetFileSize = UseTargetFileSize;
        jobItem.TargetFileSize = UseTargetFileSize ? TargetFileSize : 0;
        jobItem.AssaSubtitleFileName = MakeAssa(jobItem.SubtitleFileName);
        jobItem.Status = Se.Language.General.Generating;

        bool result;
        if (jobItem.UseTargetFileSize)
        {
            result = RunTwoPassEncoding(jobItem);
            if (result)
            {
                _timerAnalyze.Start();
            }
        }
        else
        {
            result = RunOnePassEncoding(jobItem);
            if (result)
            {
                _timerGenerate.Start();
            }
        }
    }

    private bool RunTwoPassEncoding(BurnInJobItem jobItem)
    {
        var bitRate = GetVideoBitRate(jobItem);
        jobItem.VideoBitRate = bitRate.ToString(CultureInfo.InvariantCulture) + "k";

        if (bitRate < 10)
        {
            Dispatcher.UIThread.Invoke(async () =>
            {
                await MessageBox.Show(Window!,
                    "Unable to generate video",
                    $"Bit rate too low: {bitRate}k",
                    MessageBoxButtons.OK);
            });
            return false;
        }

        _ffmpegProcess = GetFfmpegProcess(jobItem, 1);

#pragma warning disable CA1416 // Validate platform compatibility
        _ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        _ffmpegProcess.BeginOutputReadLine();
        _ffmpegProcess.BeginErrorReadLine();
        _startTicks = DateTime.UtcNow.Ticks;

        return true;
    }

    private int GetVideoBitRate(BurnInJobItem item)
    {
        var audioMb = 0;
        if (SelectedAudioEncoding == "copy")
        {
            audioMb = GetAudioFileSizeInMb(item);
        }

        // (MiB * 8192 [converts MiB to kBit]) / video seconds = kBit/s total bitrate
        var bitRate = (int)Math.Round(((double)TargetFileSize - audioMb) * 8192.0 / item.TotalSeconds);
        if (SelectedAudioEncoding != "copy" && !string.IsNullOrWhiteSpace(SelectedAudioBitRate))
        {
            var audioBitRate = int.Parse(SelectedAudioBitRate.RemoveChar('k').TrimEnd());
            bitRate -= audioBitRate;
        }

        return bitRate;
    }

    private int GetAudioFileSizeInMb(BurnInJobItem item)
    {
        var ffmpegLocation = Configuration.Settings.General.FFmpegLocation;
        if (!Configuration.IsRunningOnWindows && (string.IsNullOrEmpty(ffmpegLocation) || !File.Exists(ffmpegLocation)))
        {
            ffmpegLocation = "ffmpeg";
        }

        var tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".aac");
        var process = new Process
        {
            StartInfo =
            {
                FileName = ffmpegLocation,
                Arguments = $"-i \"{item.InputVideoFileName}\" -vn -acodec copy \"{tempFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };


#pragma warning disable CA1416
        _ = process.Start();
#pragma warning restore CA1416
        process.WaitForExit();
        try
        {
            var length = (int)Math.Round(new FileInfo(tempFileName).Length / 1024.0 / 1024);
            try
            {
                File.Delete(tempFileName);
            }
            catch
            {
                // ignore
            }

            return length;
        }
        catch
        {
            return 0;
        }
    }

    private bool RunOnePassEncoding(BurnInJobItem jobItem)
    {
        _ffmpegProcess = GetFfmpegProcess(jobItem);
#pragma warning disable CA1416 // Validate platform compatibility
        _ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        _ffmpegProcess.BeginOutputReadLine();
        _ffmpegProcess.BeginErrorReadLine();

        return true;
    }

    private Process GetFfmpegProcess(BurnInJobItem jobItem, int? passNumber = null, bool preview = false)
    {
        var audioCutTracks = string.Empty;
        //if (listViewAudioTracks.Visible)
        //{
        //    for (var index = 0; index < listViewAudioTracks.Items.Count; index++)
        //    {
        //        var listViewItem = listViewAudioTracks.Items[index];
        //        if (!listViewItem.Checked)
        //        {
        //            audioCutTracks += $"-map 0:a:{index} ";
        //        }
        //    }
        //}

        var pass = string.Empty;
        if (passNumber.HasValue)
        {
            pass = passNumber.Value.ToString(CultureInfo.InvariantCulture);
        }

        var cutStart = string.Empty;
        var cutEnd = string.Empty;
        if (IsCutActive && !preview)
        {
            var start = CutFrom;
            cutStart = $"-ss {start.Hours:00}:{start.Minutes:00}:{start.Seconds:00}";

            var end = CutTo;
            var duration = end - start;
            cutEnd = $"-t {duration.Hours:00}:{duration.Minutes:00}:{duration.Seconds:00}";
        }

        return VideoPreviewGenerator.GenerateHardcodedVideoFile(
            jobItem.InputVideoFileName,
            jobItem.AssaSubtitleFileName,
            jobItem.OutputVideoFileName,
            jobItem.Width,
            jobItem.Height,
            SelectedVideoEncoding.Codec,
            SelectedVideoPreset ?? string.Empty,
            SelectedVideoPixelFormat?.Codec ?? string.Empty,
            SelectedVideoCrf ?? string.Empty,
            SelectedAudioEncoding,
            AudioIsStereo,
            SelectedAudioSampleRate.Replace("Hz", string.Empty).Trim(),
            SelectedVideoTuneFor ?? string.Empty,
            SelectedAudioBitRate,
            pass,
            jobItem.VideoBitRate,
            OutputHandler,
            cutStart,
            cutEnd,
            audioCutTracks);
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

        _mediaInfo = FfmpegMediaInfo.Parse(VideoFileName);
        VideoWidth = _mediaInfo.Dimension.Width;
        VideoHeight = _mediaInfo.Dimension.Height;

        var jobItem = new BurnInJobItem(VideoFileName, _mediaInfo.Dimension.Width, _mediaInfo.Dimension.Height)
        {
            InputVideoFileName = VideoFileName,
            OutputVideoFileName = MakeOutputFileName(VideoFileName),
            UseTargetFileSize = UseTargetFileSize,
            TargetFileSize = TargetFileSize,
        };
        jobItem.AddSubtitleFileName(subtitleFileName);

        return new ObservableCollection<BurnInJobItem>(new[] { jobItem });
    }

    private string MakeAssa(string subtitleFileName)
    {
        if (string.IsNullOrWhiteSpace(subtitleFileName) || !File.Exists(subtitleFileName))
        {
            JobItems[_jobItemIndex].Status = "Skipped";
            return string.Empty;
        }

        var isAssa = subtitleFileName.EndsWith(".ass", StringComparison.OrdinalIgnoreCase);

        var subtitle = Subtitle.Parse(subtitleFileName);

        if (!isAssa)
        {
            SetStyleForNonAssa(subtitle);
        }

        var assa = new AdvancedSubStationAlpha();
        var assaFileName = Path.Combine(Path.GetTempFileName() + assa.Extension);
        File.WriteAllText(assaFileName, assa.ToText(subtitle, string.Empty));
        return assaFileName;
    }

    private void SetStyleForNonAssa(Subtitle sub)
    {
        sub.Header = AdvancedSubStationAlpha.DefaultHeader;
        var style = AdvancedSubStationAlpha.GetSsaStyle("Default", sub.Header);
        style.FontSize = CalculateFontSize(JobItems[_jobItemIndex].Width, JobItems[_jobItemIndex].Height, FontFactor);
        style.Bold = FontIsBold;
        style.FontName = SelectedFontName;
        style.Background = ColorUtils.FromArgb(255, (int)(FontShadowColor.R * 255.0), (int)(FontShadowColor.G * 255.0), (int)(FontShadowColor.B * 255.0));
        style.Primary = ColorUtils.FromArgb(255, (int)(FontTextColor.R * 255.0), (int)(FontTextColor.G * 255.0), (int)(FontTextColor.B * 255.0));
        style.Outline = ColorUtils.FromArgb(255, (int)(FontOutlineColor.R * 255.0), (int)(FontOutlineColor.G * 255.0), (int)(FontOutlineColor.B * 255.0));
        style.OutlineWidth = SelectedFontOutline;
        style.ShadowWidth = SelectedFontShadowWidth;
        style.Alignment = SelectedFontAlignment.Code;
        style.MarginLeft = FontMarginHorizontal;
        style.MarginRight = FontMarginHorizontal;
        style.MarginVertical = FontMarginVertical;

        if (SelectedFontBoxType.BoxType == FontBoxType.None)
        {
            style.BorderStyle = "0"; // bo box
        }
        else if (SelectedFontBoxType.BoxType == FontBoxType.BoxPerLine)
        {
            style.BorderStyle = "3"; // box - per line
        }
        else
        {
            style.BorderStyle = "4"; // box - multi line
        }

        sub.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(sub.Header, new List<SsaStyle> { style });
        sub.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + ((int)VideoWidth).ToString(CultureInfo.InvariantCulture), "[Script Info]", sub.Header);
        sub.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + ((int)VideoHeight).ToString(CultureInfo.InvariantCulture), "[Script Info]", sub.Header);
    }

    private static string MakeOutputFileName(string videoFileName)
    {
        var nameNoExt = Path.GetFileNameWithoutExtension(videoFileName);
        var ext = Path.GetExtension(videoFileName).ToLowerInvariant();
        if (ext != ".mp4" && ext != ".mkv")
        {
            ext = ".mkv";
        }
        ;

        var suffix = Se.Settings.Video.BurnIn.BurnInSuffix;
        var fileName = Path.Combine(Path.GetDirectoryName(videoFileName)!, nameNoExt + suffix + ext);
        if (Se.Settings.Video.BurnIn.UseOutputFolder &&
            !string.IsNullOrEmpty(Se.Settings.Video.BurnIn.OutputFolder) &&
            Directory.Exists(Se.Settings.Video.BurnIn.OutputFolder))
        {
            fileName = Path.Combine(Se.Settings.Video.BurnIn.OutputFolder, nameNoExt + suffix + ext);
        }

        if (File.Exists(fileName))
        {
            fileName = fileName.Remove(fileName.Length - ext.Length) + "_" + Guid.NewGuid() + ext;
        }

        return fileName;
    }

    public static int CalculateFontSize(int videoWidth, int videoHeight, double factor, int minSize = 8, int maxSize = 2000)
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
    private async Task Help()
    {
        var codec = SelectedVideoEncoding.Codec;

        if (codec == "libx265")
        {
            await Window!.Launcher.LaunchUriAsync(new Uri("http://trac.ffmpeg.org/wiki/Encode/H.265"));
        }
        else if (codec == "libvpx-vp9")
        {
            await Window!.Launcher.LaunchUriAsync(new Uri("http://trac.ffmpeg.org/wiki/Encode/VP9"));
        }
        else if (codec is "h264_nvenc" or "hevc_nvenc")
        {
            await Window!.Launcher.LaunchUriAsync(new Uri("https://trac.ffmpeg.org/wiki/HWAccelIntro"));
        }
        else if (codec == "prores_ks")
        {
            await Window!.Launcher.LaunchUriAsync(new Uri("https://ottverse.com/ffmpeg-convert-to-apple-prores-422-4444-hq"));
        }
        else
        {
            await Window!.Launcher.LaunchUriAsync(new Uri("http://trac.ffmpeg.org/wiki/Encode/H.264"));
        }
    }

    [RelayCommand]
    private async Task Add()
    {
        var fileNames = await _fileHelper.PickOpenVideoFiles(Window!, Se.Language.General.AddVideoFiles);
        if (fileNames == null || fileNames.Length == 0)
        {
            return;
        }

        foreach (var fileName in fileNames)
        {
            var mediaInfo = FfmpegMediaInfo.Parse(fileName);
            var fileInfo = new FileInfo(fileName);
            var jobItem = new BurnInJobItem(fileName, mediaInfo.Dimension.Width, mediaInfo.Dimension.Height)
            {
                OutputVideoFileName = MakeOutputFileName(fileName),
                TotalFrames = mediaInfo.GetTotalFrames(),
                TotalSeconds = mediaInfo.Duration.TotalSeconds,
                Width = mediaInfo.Dimension.Width,
                Height = mediaInfo.Dimension.Height,
                Size = Utilities.FormatBytesToDisplayFileSize(fileInfo.Length),
                Resolution = mediaInfo.Dimension.ToString(),
            };
            jobItem.AddSubtitleFileName(TryGetSubtitleFileName(fileName));

            Dispatcher.UIThread.Invoke(() =>
            {
                JobItems.Add(jobItem);
            });
        }
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
    private async Task PickSubtitle()
    {
        if (SelectedJobItem == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, "Open subtitle file");
        if (string.IsNullOrEmpty(fileName))
        {
            SelectedJobItem.SubtitleFileName = fileName;
        }
    }

    [RelayCommand]
    private async Task OutputProperties()
    {
        await _windowService.ShowDialogAsync<BatchConvertSettingsWindow, BatchConvertSettingsViewModel>(Window!);
        UpdateOutputProperties();
    }

    [RelayCommand]
    private void BrowseResolution()
    {
    }

    [RelayCommand]
    private void BrowseCutFrom()
    {
    }

    [RelayCommand]
    private void BrowseCutTo()
    {
    }

    [RelayCommand]
    private void BrowseVideoResolution()
    {
    }

    [RelayCommand]
    private async Task Generate()
    {
        if (IsCutActive && CutFrom >= CutTo)
        {
            await MessageBox.Show(Window!,
                "Cut settings error",
                "Cut end time must be after cut start time",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

        if (!IsBatchMode)
        {
            JobItems = GetCurrentVideoAsJobItems();
        }

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
        _processedFrames = 0;
        ProgressValue = 0;
        SaveSettings();

        InitAndStartJobItem(0);
    }

    private void LoadSettings()
    {
        var settings = Se.Settings.Video.BurnIn;
        FontFactor = settings.FontFactor;
        FontIsBold = settings.FontBold;
        SelectedFontOutline = settings.OutlineWidth;
        SelectedFontShadowWidth = settings.ShadowWidth;
        SelectedFontName = settings.FontName;
        FontTextColor = settings.NonAssaTextColor.FromHexToColor();
        FontOutlineColor = settings.NonAssaOutlineColor.FromHexToColor();
        FontBoxColor = settings.NonAssaBoxColor.FromHexToColor();
        FontShadowColor = settings.NonAssaShadowColor.FromHexToColor();
        FontFixRtl = settings.NonAssaFixRtlUnicode;
        SelectedFontAlignment = FontAlignments.First(p => p.Code == settings.NonAssaAlignment);
        OutputSourceFolder = settings.OutputFolder;
        UseOutputFolderVisible = settings.UseOutputFolder;
        UseSourceFolderVisible = !settings.UseOutputFolder;
        _useSourceResolution = settings.UseSourceResolution;
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Video.BurnIn;
        settings.FontFactor = FontFactor;
        settings.FontBold = FontIsBold;
        settings.OutlineWidth = SelectedFontOutline;
        settings.ShadowWidth = SelectedFontShadowWidth;
        settings.FontName = SelectedFontName;
        settings.NonAssaTextColor = FontTextColor.FromColorToHex();
        settings.NonAssaOutlineColor = FontOutlineColor.FromColorToHex();
        settings.NonAssaBoxColor = FontBoxColor.FromColorToHex();
        settings.NonAssaShadowColor = FontShadowColor.FromColorToHex();
        settings.NonAssaFixRtlUnicode = FontFixRtl;
        settings.NonAssaAlignment = SelectedFontAlignment.Code;
        settings.OutputFolder = OutputSourceFolder;
        settings.UseOutputFolder = UseOutputFolderVisible;
        settings.UseSourceResolution = _useSourceResolution;

        Se.SaveSettings();
    }

    [RelayCommand]
    private void SingleMode()
    {
        IsBatchMode = false;
    }

    [RelayCommand]
    private void BatchMode()
    {
        IsBatchMode = true;
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

    private static string TryGetSubtitleFileName(string fileName)
    {
        var srt = Path.ChangeExtension(fileName, ".srt");
        if (File.Exists(srt))
        {
            return srt;
        }

        var assa = Path.ChangeExtension(fileName, ".ass");
        if (File.Exists(srt))
        {
            return assa;
        }

        var dir = Path.GetDirectoryName(fileName);
        if (string.IsNullOrEmpty(dir))
        {
            return string.Empty;
        }

        var searchPath = Path.GetFileNameWithoutExtension(fileName);
        var files = Directory.GetFiles(dir, searchPath + "*");
        var subtitleExtensions = SubtitleFormat.AllSubtitleFormats.Select(p => p.Extension).Distinct();
        foreach (var ext in subtitleExtensions)
        {
            foreach (var file in files)
            {
                if (file.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
                {
                    return file;
                }
            }
        }

        return string.Empty;
    }


    internal void VideoEncodingChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_loading)
        {
            return;
        }

        FillPreset(SelectedVideoEncoding.Codec);
        FillCrf(SelectedVideoEncoding.Codec);
    }

    private void FillPreset(string videoCodec)
    {
        VideoPresetText = "Preset";
        SelectedVideoPreset = null;

        var items = new List<string>
        {
           "ultrafast",
           "superfast",
           "veryfast",
           "faster",
           "fast",
           "medium",
           "slow",
           "slower",
           "veryslow",
        };

        var defaultItem = "medium";

        if (videoCodec == "h264_nvenc")
        {
            items = new List<string>
            {
                "default",
                "slow",
                "medium",
                "fast",
                "hp",
                "hq",
                "bd",
                "ll",
                "llhq",
                "llhp",
                "lossless",
                "losslesshp",
                "p1",
                "p2",
                "p3",
                "p4",
                "p5",
                "p6",
                "p7",
            };
        }
        else if (videoCodec == "hevc_nvenc")
        {
            items = new List<string>
            {
                "default",
                "slow",
                "medium",
                "fast",
                "hp",
                "hq",
                "bd",
                "ll",
                "llhq",
                "llhp",
                "lossless",
                "losslesshp",
                "p1",
                "p2",
                "p3",
                "p4",
                "p5",
                "p6",
                "p7",
            };
        }
        else if (videoCodec == "h264_amf")
        {
            items = new List<string> { string.Empty };
        }
        else if (videoCodec == "hevc_amf")
        {
            items = new List<string> { string.Empty };
        }
        else if (videoCodec == "libvpx-vp9")
        {
            items = new List<string> { string.Empty };
        }
        else if (videoCodec == "prores_ks")
        {
            items = new List<string>
            {
                "proxy",
                "lt",
                "standard",
                "hq",
                "4444",
                "4444xq",
            };

            defaultItem = "standard";

            VideoPresetText = "Profile";
        }

        VideoPresets.Clear();
        VideoPresets.AddRange(items);
        if (VideoPresets.Contains(defaultItem))
        {
            SelectedVideoPreset = defaultItem;
        }
    }

    public void FillCrf(string videoCodec)
    {
        SelectedVideoCrf = null;
        VideoCrfText = "CRF";

        var items = new List<string> { " " };

        if (videoCodec == "libx265")
        {
            for (var i = 0; i < 51; i++)
            {
                items.Add(i.ToString(CultureInfo.InvariantCulture));
            }

            VideoCrf.Clear();
            VideoCrf.AddRange(items);
            SelectedVideoCrf = "28";
        }
        else if (videoCodec == "libvpx-vp9")
        {
            for (var i = 4; i <= 63; i++)
            {
                items.Add(i.ToString(CultureInfo.InvariantCulture));
            }

            VideoCrf.Clear();
            VideoCrf.AddRange(items);
            SelectedVideoCrf = "10";
        }
        else if (videoCodec == "h264_nvenc" ||
                 videoCodec == "hevc_nvenc")
        {
            for (var i = 0; i <= 51; i++)
            {
                items.Add(i.ToString(CultureInfo.InvariantCulture));
            }

            VideoCrfText = "CQ";
            VideoCrfHint = "0=best quality, 51=best speed";
            VideoCrf.Clear();
            VideoCrf.AddRange(items);
            SelectedVideoCrf = null;
        }
        else if (videoCodec == "h264_amf" ||
                 videoCodec == "hevc_amf")
        {
            for (var i = 0; i <= 10; i++)
            {
                items.Add(i.ToString(CultureInfo.InvariantCulture));
            }

            VideoCrfText = "Quality";
            VideoCrfHint = "0=best quality, 10=best speed";
            VideoCrf.Clear();
            VideoCrf.AddRange(items);
            SelectedVideoCrf = null;
        }
        else if (videoCodec == "prores_ks")
        {
            items = new List<string>();
            VideoCrf.Clear();
            VideoCrf.AddRange(items);
        }
        else
        {
            for (var i = 17; i <= 28; i++)
            {
                items.Add(i.ToString(CultureInfo.InvariantCulture));
            }

            VideoCrf.Clear();
            VideoCrf.AddRange(items);
            SelectedVideoCrf = "23";
        }
    }

    private void UpdateOutputProperties()
    {

        if (!Se.Settings.Tools.BatchConvert.SaveInSourceFolder &&
            string.IsNullOrWhiteSpace(Se.Settings.Tools.BatchConvert.OutputFolder))
        {
            Se.Settings.Tools.BatchConvert.SaveInSourceFolder = true;
        }

        string text;
        if (Se.Settings.Video.BurnIn.SaveInSourceFolder)
        {
            text = string.Format("Outputfolder: {0}", "Source folder");
        }
        else
        {
            text = string.Format("Outputfolder: {0}", Se.Settings.Video.BurnIn.OutputFolder);
        }

        text += Environment.NewLine +
                Se.Settings.Tools.BatchConvert.Overwrite;

        //TODO:       OutputPropertiesText = text;
    }
}