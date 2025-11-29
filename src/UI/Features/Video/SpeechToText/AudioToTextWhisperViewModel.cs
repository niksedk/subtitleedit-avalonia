using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public partial class AudioToTextWhisperViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<IWhisperEngine> _engines;
    [ObservableProperty] private IWhisperEngine _selectedEngine;

    [ObservableProperty] private ObservableCollection<WhisperLanguage> _languages;
    [ObservableProperty] private WhisperLanguage? _selectedLanguage;

    [ObservableProperty] private ObservableCollection<WhisperModelDisplay> _models;
    [ObservableProperty] private WhisperModelDisplay? _selectedModel;

    [ObservableProperty] private ObservableCollection<WhisperJobItem> _batchItems;
    [ObservableProperty] private WhisperJobItem? _selectedBatchItem;

    [ObservableProperty] private bool _doTranslateToEnglish;
    [ObservableProperty] private bool _doAdjustTimings;
    [ObservableProperty] private bool _doPostProcessing;

    [ObservableProperty] private string _parameters;

    [ObservableProperty] private string _consoleLog;

    [ObservableProperty] private bool _isBatchMode;
    [ObservableProperty] private bool _isBatchModeVisible;
    [ObservableProperty] private bool _isSingleModeVisible;
    [ObservableProperty] private bool _isWhisperCppActive;
    [ObservableProperty] private bool _isWhisperPurfviewXxlActive;
    [ObservableProperty] private bool _isTranscribeEnabled;
    [ObservableProperty] private double _progressOpacity;

    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private string _elapsedText;
    [ObservableProperty] private string _estimatedText;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public Subtitle TranscribedSubtitle { get; private set; }
    public TextBox TextBoxConsoleLog { get; internal set; }
    public DataGrid BatchGrid { get; internal set; }

    private bool _unknownArgument;
    private bool _cudaOutOfMemory;
    private bool _incompleteModel;
    private bool _loadedFromStdOut;
    private string? _videoFileName;
    private string _waveFileName = string.Empty;
    private int _audioTrackNumber;
    private readonly List<string> _filesToDelete = new();
    private readonly ConcurrentQueue<string> _outputText = new();
    private long _startTicks = 0;
    private double _endSeconds;
    private double _showProgressPct = -1;
    private double _lastEstimatedMs = double.MaxValue;
    private readonly VideoInfo _videoInfo = new();
    private bool _abort;
    private readonly List<ResultText> _resultList = new();
    private bool _useCenterChannelOnly;

    private readonly Regex _timeRegexShort =
        new(@"^\[\d\d:\d\d[\.,]\d\d\d --> \d\d:\d\d[\.,]\d\d\d\]", RegexOptions.Compiled);

    private readonly Regex _timeRegexLong =
        new(@"^\[\d\d:\d\d:\d\d[\.,]\d\d\d --> \d\d:\d\d:\d\d[\.,]\d\d\d]", RegexOptions.Compiled);

    private readonly Regex _pctWhisper = new(@"^\d+%\|", RegexOptions.Compiled);
    private readonly Regex _pctWhisperFaster = new(@"^\s*\d+%\s*\|", RegexOptions.Compiled);
    private readonly System.Timers.Timer _timerWhisper = new();
    private Process _whisperProcess = new();
    private Process? _waveExtractProcess = new();
    private readonly System.Timers.Timer _timerWaveExtract = new();
    private Stopwatch _sw = new();
    private StringBuilder _ffmpegLog = new();
    private readonly Lock _lockObj = new();
    private int _batchIndex = -1;
    private string _error;

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;

    public AudioToTextWhisperViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;

        Engines = [new WhisperEngineCpp()];
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Engines.Add(new WhisperEnginePurfviewFasterWhisperXxl());
            Engines.Add(new WhisperEngineConstMe());
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Engines.Add(new WhisperEnginePurfviewFasterWhisperXxl());
        }

        Engines.Add(new WhisperEngineOpenAi());

        SelectedEngine = Engines[0];

        Languages = new ObservableCollection<WhisperLanguage>(SelectedEngine.Languages);
        SelectedLanguage = Enumerable.FirstOrDefault<WhisperLanguage>(Languages, p => p.Name == "English");

        Models = new ObservableCollection<WhisperModelDisplay>();

        BatchItems = new ObservableCollection<WhisperJobItem>();

        IsTranscribeEnabled = true;
        Parameters = string.Empty;
        ConsoleLog = string.Empty;
        ProgressText = string.Empty;
        ElapsedText = string.Empty;
        EstimatedText = string.Empty;
        TranscribedSubtitle = new Subtitle();
        TextBoxConsoleLog = new TextBox();
        BatchGrid = new DataGrid();
        _audioTrackNumber = 0;
        _error = string.Empty;

        LoadSettings();

        _timerWhisper.Interval = 100;
        _timerWhisper.Elapsed += OnTimerWhisperOnElapsed;

        _timerWaveExtract.Interval = 100;
        _timerWaveExtract.Elapsed += OnTimerWaveExtractOnElapsed;
    }

    private void LoadSettings()
    {
        DoTranslateToEnglish = false;
        DoAdjustTimings = Se.Settings.Tools.AudioToText.WhisperAutoAdjustTimings;
        DoPostProcessing = Se.Settings.Tools.AudioToText.PostProcessing;
        Parameters = Se.Settings.Tools.AudioToText.WhisperCustomCommandLineArguments;

        var selectedEngine = Enumerable.FirstOrDefault<IWhisperEngine>(Engines, p => p.Choice == Se.Settings.Tools.AudioToText.WhisperChoice);
        if (selectedEngine != null)
        {
            SelectedEngine = selectedEngine;
        }

        EngineChanged();
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.AudioToText.WhisperAutoAdjustTimings = DoAdjustTimings;
        Se.Settings.Tools.AudioToText.PostProcessing = DoPostProcessing;
        Se.Settings.Tools.AudioToText.WhisperCustomCommandLineArguments = Parameters;
        Se.Settings.Tools.AudioToText.WhisperCustomCommandLineArgumentsPurfviewBlank = Parameters == "--standard";
        Se.Settings.Tools.AudioToText.WhisperChoice = SelectedEngine.Choice;
        Se.Settings.Tools.AudioToText.WhisperModel = SelectedModel?.Model.Name ?? string.Empty;
        Se.Settings.Tools.AudioToText.WhisperLanguageCode = SelectedLanguage?.Code ?? string.Empty;
        Se.Settings.Tools.AudioToText.WhisperCustomCommandLineArguments = Parameters;

        Se.SaveSettings();
    }

    private void OnTimerWhisperOnElapsed(object? sender, ElapsedEventArgs args)
    {
        lock (_lockObj)
        {
            if (_abort)
            {
                _timerWhisper.Stop();
#pragma warning disable CA1416
                _whisperProcess.Kill(true);
#pragma warning restore CA1416

                Dispatcher.UIThread.Invoke<Task>(async () =>
                {
                    ProgressOpacity = 0;
                    var partialSub = new Subtitle();
                    partialSub.Paragraphs.AddRange(_resultList.OrderBy(p => p.Start)
                        .Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());

                    if (partialSub.Paragraphs.Count > 0)
                    {
                        var answer = await MessageBox.Show(
                            Window!,
                            $"Keep partial transcription?",
                            $"Do you want to keep {partialSub.Paragraphs.Count} lines?",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (answer != MessageBoxResult.Yes)
                        {
                            _resultList.Clear();
                            partialSub.Paragraphs.Clear();
                            Cancel();
                            return;
                        }
                    }

                    await MakeResult(partialSub);
                });

                return;
            }

            if (!_whisperProcess.HasExited)
            {
                var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
                ProgressText = GetProgressText();

                ElapsedText = $"Time elapsed: {new TimeCode(durationMs).ToShortDisplayString()}";
                if (_endSeconds <= 0)
                {
                    if (_showProgressPct > 0)
                    {
                        SetProgressBarPct(_showProgressPct);
                    }

                    return;
                }

                ShowProgressBar();

                _videoInfo.TotalSeconds = Math.Max(_endSeconds, _videoInfo.TotalSeconds);
                var msPerFrame = durationMs / (_endSeconds * 1000.0);
                var estimatedTotalMs = msPerFrame * _videoInfo.TotalMilliseconds;
                var msEstimatedLeft = estimatedTotalMs - durationMs;
                if (msEstimatedLeft > _lastEstimatedMs)
                {
                    msEstimatedLeft = _lastEstimatedMs;
                }
                else
                {
                    _lastEstimatedMs = msEstimatedLeft;
                }

                if (_showProgressPct > 0)
                {
                    SetProgressBarPct(_showProgressPct);
                }
                else
                {
                    SetProgressBarPct(_endSeconds * 100.0 / _videoInfo.TotalSeconds);
                }

                EstimatedText = ProgressHelper.ToProgressTime(msEstimatedLeft);

                return;
            }

            _timerWhisper.Stop();


            Dispatcher.UIThread.Invoke<Task>(async () =>
            {
                ProgressValue = 100;
                var settings = Se.Settings.Tools.AudioToText;
                LogToConsole($"Whisper ({settings.WhisperChoice}) done in {_sw.Elapsed}{Environment.NewLine}");

                _whisperProcess.Dispose();

                if (GetResultFromSrt(_waveFileName, _videoFileName!, out var resultTexts, _outputText, _filesToDelete))
                {
                    var subtitle = new Subtitle();
                    subtitle.Paragraphs.AddRange(resultTexts
                        .Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());

                    var postProcessedSubtitle = PostProcess(subtitle);
                    await MakeResult(postProcessedSubtitle);

                    return;
                }

                _outputText.Enqueue("Loading result from STDOUT" + Environment.NewLine);
                _loadedFromStdOut = true;
                var transcribedSubtitleFromStdOut = new Subtitle();
                transcribedSubtitleFromStdOut.Paragraphs.AddRange(_resultList.OrderBy(p => p.Start)
                    .Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());
                await MakeResult(transcribedSubtitleFromStdOut);
            });
        }
    }

    private string GetProgressText()
    {
        if (IsBatchMode)
        {
            return string.Format(Se.Language.Video.AudioToText.TranscribingXOfY, _batchIndex + 1, BatchItems.Count);
        }
        else
        {
            return Se.Language.Video.AudioToText.Transcribing;
        }
    }

    private void StartNext(Subtitle? transcribedSubtitle)
    {
        var currentItem = BatchItems[_batchIndex];
        if (transcribedSubtitle != null && transcribedSubtitle.Paragraphs.Count > 0)
        {
            currentItem.Status = Se.Language.General.Converted;
            var subtitleFileName = GetSubtitleFileName(currentItem.InputVideoFileName);
            var format = new SubRip();
            var text = format.ToText(transcribedSubtitle, string.Empty);
            File.WriteAllText(subtitleFileName, text);
        }

        _batchIndex++;
        if (_batchIndex < BatchItems.Count)
        {
            ProgressValue = 0;
            _startTicks = 0;
            _endSeconds = 0; ;
            _showProgressPct = -1;
            _lastEstimatedMs = double.MaxValue;
            _outputText.Clear();
            ConsoleLog = string.Empty;
            ProgressText = string.Empty;
            ElapsedText = string.Empty;
            EstimatedText = string.Empty;

            var jobItem = BatchItems[_batchIndex];
            _videoFileName = jobItem.InputVideoFileName;
            _videoInfo.TotalMilliseconds = jobItem.MediaInfo.Duration.TotalMilliseconds;
            _videoInfo.TotalSeconds = jobItem.MediaInfo.Duration.TotalSeconds;
            _videoInfo.Width = jobItem.MediaInfo.Dimension.Width;
            _videoInfo.Height = jobItem.MediaInfo.Dimension.Height;

            ProgressOpacity = 1;
            ProgressText = Se.Language.General.GeneratingWavFile;
            _startTicks = DateTime.UtcNow.Ticks;

            Dispatcher.UIThread.Post(() =>
            {
                if (BatchGrid == null)
                {
                    return;
                }

                BatchGrid.SelectedItem = jobItem;
                BatchGrid.ScrollIntoView(jobItem, null);
            });

            var startGenerateWaveFileOk = GenerateWavFile(_videoFileName, _audioTrackNumber);
            return;
        }

        var convertedJobs = Enumerable.Count<WhisperJobItem>(BatchItems, p => p.Status == Se.Language.General.Converted);
        var failed = Enumerable.Count<WhisperJobItem>(BatchItems, p => p.Status != Se.Language.General.Converted);

        Dispatcher.UIThread.Invoke<Task>(async () =>
        {
            var msg = $"Videos converted: " + convertedJobs;
            if (failed > 0)
            {
                msg += Environment.NewLine + $"Videos failed: " + failed;
            }

            await MessageBox.Show(
                Window!,
                Se.Language.Video.AudioToText.Title,
                msg,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            IsTranscribeEnabled = true;
        });
    }

    private string GetSubtitleFileName(string videoFileName)
    {
        var path = Path.GetDirectoryName(videoFileName);
        var fileName = Path.GetFileNameWithoutExtension(videoFileName);
        var extension = ".srt";
        var subtitleFileName = Path.Combine(path!, fileName + extension);
        int count = 2;
        while (File.Exists(subtitleFileName))
        {
            subtitleFileName = Path.Combine(path!, fileName + "_" + count + extension);
            count++;
        }

        return subtitleFileName;
    }

    private Subtitle PostProcess(Subtitle transcript)
    {
        if (SelectedLanguage is not WhisperLanguage language)
        {
            return transcript;
        }

        if (DoAdjustTimings || DoPostProcessing)
        {
            ProgressText = "Post-processing...";
        }

        var postProcessor = new AudioToTextPostProcessor(DoTranslateToEnglish ? "en" : language.Code)
        {
            ParagraphMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * 2,
        };

        WavePeakData2? wavePeaks = null;
        if (DoAdjustTimings)
        {
            wavePeaks = MakeWavePeaks();
        }

        if (DoAdjustTimings && wavePeaks != null)
        {
            transcript = WhisperTimingFixer.ShortenLongDuration(transcript);
            var wp = new WavePeakData(wavePeaks.SampleRate, wavePeaks.Peaks.Select(p => new WavePeak(p.Max, p.Min)).ToList());
            transcript = WhisperTimingFixer.ShortenViaWavePeaks(transcript, wp);
        }

        var settings = Se.Settings.Tools.AudioToText;
        transcript = postProcessor.Fix(
            AudioToTextPostProcessor.Engine.Whisper,
            transcript,
            DoPostProcessing,
            settings.WhisperPostProcessingAddPeriods,
            settings.WhisperPostProcessingMergeLines,
            settings.WhisperPostProcessingFixCasing,
            settings.WhisperPostProcessingFixShortDuration,
            settings.WhisperPostProcessingSplitLines);

        return transcript;
    }

    private WavePeakData2? MakeWavePeaks()
    {
        if (string.IsNullOrEmpty(_videoFileName) || !File.Exists(_videoFileName))
        {
            return null;
        }

        var targetFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
        try
        {
            var process = GetFfmpegProcess(_videoFileName, _audioTrackNumber, targetFile);
            if (process == null)
            {
                return null;
            }

#pragma warning disable CA1416
            process.Start();
#pragma warning restore CA1416

            while (!process.HasExited)
            {
                Task.Delay(100);
            }

            // check for delay in matroska files
            var delayInMilliseconds = 0;
            var audioTrackNames = new List<string>();
            var mkvAudioTrackNumbers = new Dictionary<int, int>();
            if (_videoFileName.ToLowerInvariant().EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using (var matroska = new MatroskaFile(_videoFileName))
                    {
                        if (matroska.IsValid)
                        {
                            foreach (var track in matroska.GetTracks())
                            {
                                if (track.IsAudio)
                                {
                                    if (track.CodecId != null && track.Language != null)
                                    {
                                        audioTrackNames.Add("#" + track.TrackNumber + ": " +
                                                            track.CodecId.Replace("\0", string.Empty) + " - " +
                                                            track.Language.Replace("\0", string.Empty));
                                    }
                                    else
                                    {
                                        audioTrackNames.Add("#" + track.TrackNumber);
                                    }

                                    mkvAudioTrackNumbers.Add(mkvAudioTrackNumbers.Count, track.TrackNumber);
                                }
                            }

                            if (mkvAudioTrackNumbers.Count > 0)
                            {
                                delayInMilliseconds =
                                    (int)matroska.GetAudioTrackDelayMilliseconds(mkvAudioTrackNumbers[0]);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    SeLogger.Error(exception, $"Error getting delay from mkv: {_videoFileName}");
                }
            }

            if (File.Exists(targetFile))
            {
                using var waveFile = new WavePeakGenerator2(targetFile);
                if (!string.IsNullOrEmpty(_videoFileName) && File.Exists(_videoFileName))
                {
                    return waveFile.GeneratePeaks(delayInMilliseconds,
                        WavePeakGenerator2.GetPeakWaveFileName(_videoFileName));
                }
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    public bool GetResultFromSrt(string waveFileName, string videoFileName, out List<ResultText> resultTexts,
        ConcurrentQueue<string> outputText, List<string> filesToDelete)
    {
        if (SelectedEngine is not IWhisperEngine engine)
        {
            resultTexts = new List<ResultText>();
            return false;
        }

        var srtFileName = waveFileName + ".srt";
        if (!File.Exists(srtFileName) && waveFileName.EndsWith(".wav"))
        {
            srtFileName = waveFileName.Remove(waveFileName.Length - 4) + ".srt";
        }

        var whisperFolder = engine.GetAndCreateWhisperFolder();
        if (!string.IsNullOrEmpty(whisperFolder) && !File.Exists(srtFileName) && !string.IsNullOrEmpty(videoFileName))
        {
            srtFileName = Path.Combine(whisperFolder, Path.GetFileNameWithoutExtension(videoFileName)) + ".srt";
        }

        if (!File.Exists(srtFileName))
        {
            srtFileName = Path.Combine(whisperFolder, Path.GetFileNameWithoutExtension(waveFileName)) + ".srt";
        }

        var vttFileName = Path.Combine(whisperFolder, Path.GetFileName(waveFileName) + ".vtt");
        if (!File.Exists(vttFileName))
        {
            vttFileName = Path.Combine(whisperFolder, Path.GetFileNameWithoutExtension(waveFileName)) + ".vtt";
        }

        if (!File.Exists(srtFileName) && !File.Exists(vttFileName))
        {
            resultTexts = new List<ResultText>();
            return false;
        }

        var sub = new Subtitle();
        if (File.Exists(srtFileName))
        {
            var rawText = FileUtil.ReadAllLinesShared(srtFileName, Encoding.UTF8);
            new SubRip().LoadSubtitle(sub, rawText, srtFileName);
            outputText?.Enqueue($"Loading result from {srtFileName}{Environment.NewLine}");
        }
        else
        {
            var rawText = FileUtil.ReadAllLinesShared(srtFileName, Encoding.UTF8);
            new WebVTT().LoadSubtitle(sub, rawText, srtFileName);
            outputText?.Enqueue($"Loading result from {vttFileName}{Environment.NewLine}");
        }

        sub.RemoveEmptyLines();

        var results = new List<ResultText>();
        foreach (var p in sub.Paragraphs)
        {
            results.Add(new ResultText
            {
                Start = (decimal)p.StartTime.TotalSeconds,
                End = (decimal)p.EndTime.TotalSeconds,
                Text = p.Text
            });
        }

        resultTexts = results;

        if (File.Exists(srtFileName))
        {
            filesToDelete?.Add(srtFileName);
        }

        if (File.Exists(vttFileName))
        {
            filesToDelete?.Add(vttFileName);
        }

        return true;
    }

    private void ShowProgressBar()
    {
        if (ProgressOpacity == 0)
        {
            ProgressValue = 0;
            ProgressOpacity = 1;
        }
    }

    private void SetProgressBarPct(double pct)
    {
        if (pct > 100)
        {
            pct = 100;
        }

        if (pct < 0)
        {
            pct = 0;
        }

        if (pct > ProgressValue)
        {
            ProgressValue = pct;
        }
        // _taskbarList.SetProgressValue(_windowHandle, Math.Max(0, Math.Min((int)pct, 100)), 100);
    }

    private async Task MakeResult(Subtitle? transcribedSubtitle)
    {
        // Small delay to ensure all output is captured and flushed
        await Task.Delay(100);

        var sbLog = new StringBuilder();
        foreach (var s in _outputText)
        {
            sbLog.AppendLine(s);
        }

        Se.WriteWhisperLog(sbLog.ToString().Trim());

        var anyLinesTranscribed = transcribedSubtitle != null && transcribedSubtitle.Paragraphs.Count > 0;

        if (IsBatchMode)
        {
            StartNext(transcribedSubtitle);
            return;
        }
        else if (_loadedFromStdOut)
        {
            await MessageBox.Show(Window!, "No result file",
                "No result file was generated by Whisper, but some text was captured from standard output.");

            if (Window != null)
            {
                _fileHelper.OpenFileWithDefaultProgram(Window, Se.GetWhisperLogFilePath());
            }
        }

        if (anyLinesTranscribed)
        {
            TranscribedSubtitle = transcribedSubtitle!;
            OkPressed = true;
            Window?.Close();
        }
        else if (_abort)
        {
            Window?.Close();
        }
        else
        {
            var settings = Se.Settings.Tools.AudioToText;
            IsTranscribeEnabled = true;

            if (_incompleteModel)
            {
                await MessageBox.Show(Window!, "Incomplete model",
                    "The model is incomplete. Please download the full model.");
            }
            else if (_unknownArgument && !string.IsNullOrEmpty(settings.WhisperCustomCommandLineArguments))
            {
                await MessageBox.Show(Window!, $"Unknown argument: {settings.WhisperCustomCommandLineArguments}",
                    "Unknown argument. Please check the advanced settings.");
            }
            else if (_cudaOutOfMemory)
            {
                await MessageBox.Show(Window!, $"CUDA failed",
                    "Whisper ran out of CUDA memory - try a smaller model or run on CPU.");
            }
            else
            {
                await MessageBox.Show(Window!, "No result", "No result from whisper. Please check the log");
            }
        }
    }

    private void OnTimerWaveExtractOnElapsed(object? sender, ElapsedEventArgs e)
    {
        lock (_lockObj)
        {
            if (_waveExtractProcess == null)
            {
                return;
            }

            if (_abort)
            {
                _timerWaveExtract.Stop();

#pragma warning disable CA1416
                _waveExtractProcess.Kill(true);
#pragma warning restore CA1416

                ProgressOpacity = 0;
                IsTranscribeEnabled = true;
                return;
            }

            if (!_waveExtractProcess.HasExited)
            {
                var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
                ElapsedText = $"Time elapsed: {new TimeCode(durationMs).ToShortDisplayString()}";

                return;
            }

            _timerWaveExtract.Stop();

            if (!File.Exists(_waveFileName))
            {
                SeLogger.WhisperInfo("Generated wave file not found: " + _waveFileName + Environment.NewLine +
                                     "ffmpeg: " + _waveExtractProcess.StartInfo.FileName + Environment.NewLine +
                                     "Parameters: " + _waveExtractProcess.StartInfo.Arguments + Environment.NewLine +
                                     "OS: " + Environment.OSVersion + Environment.NewLine +
                                     "64-bit: " + Environment.Is64BitOperatingSystem + Environment.NewLine +
                                     "ffmpeg exit code: " + _waveExtractProcess.ExitCode + Environment.NewLine +
                                     "ffmpeg log: " + _ffmpegLog);
                IsTranscribeEnabled = true;
                _waveExtractProcess = null;
                return;
            }

            _waveExtractProcess = null;
            if (string.IsNullOrEmpty(_videoFileName))
            {
                IsTranscribeEnabled = true;
                Dispatcher.UIThread.Invoke(async () =>
                {
                    await MessageBox.Show(Window!, "No video file", "No video file found!");
                });

                return;
            }

            var startOk = TranscribeViaWhisper(_waveFileName, _videoFileName);
            if (!startOk)
            {
                IsTranscribeEnabled = true;
                Dispatcher.UIThread.Invoke(async () =>
                {
                    if (string.IsNullOrEmpty(_error))
                    {
                        await MessageBox.Show(Window!, "Unknown error", "Unable to start Whisper!");
                    }
                    else
                    {
                        await MessageBox.Show(Window!, "Error", "Unable to start Whisper: " + _error);
                    }
                });
            }
        }
    }

    public void DeleteTempFiles()
    {
        foreach (var file in _filesToDelete)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch
            {
                // ignore
            }
        }
    }

    private static bool IsModelEnglishOnly(WhisperModel model)
    {
        return model.Name.EndsWith(".en", StringComparison.InvariantCulture) ||
               model.Name == "distil-large-v2" ||
               model.Name == "distil-large-v3";
    }

    [RelayCommand]
    private void ViewWhisperLogFile()
    {
        var logFilePath = Se.GetWhisperLogFilePath();
        if (Window != null)
        {
            _fileHelper.OpenFileWithDefaultProgram(Window, logFilePath);
        }
    }

    [RelayCommand]
    private async Task ReDownloadWhisperCpp()
    {
        var vm = await _windowService.ShowDialogAsync<DownloadWhisperEngineWindow, DownloadWhisperEngineViewModel>(
            Window!, viewModel =>
            {
                viewModel.Engine = Engines.First(p => p.Name == WhisperEngineCpp.StaticName);
                viewModel.StartDownload();
            });
    }

    [RelayCommand]
    private async Task ReDownloadWhisperPurfviewXxl()
    {
        var vm = await _windowService.ShowDialogAsync<DownloadWhisperEngineWindow, DownloadWhisperEngineViewModel>(
            Window!, viewModel =>
            {
                viewModel.Engine = Engines.First(p => p.Name == WhisperEnginePurfviewFasterWhisperXxl.StaticName);
                viewModel.StartDownload();
            });
    }

    [RelayCommand]
    private void SingleMode()
    {
        IsBatchMode = false;
        IsSingleModeVisible = false;
        IsBatchModeVisible = true;
    }

    [RelayCommand]
    private void BatchMode()
    {
        IsBatchMode = true;
        IsSingleModeVisible = true;
        IsBatchModeVisible = false;
    }

    [RelayCommand]
    private async Task Add()
    {
        var fileNames = await _fileHelper.PickOpenVideoFiles(Window!, Se.Language.General.AddVideoFiles);
        if (fileNames.Length == 0)
        {
            return;
        }

        var error = false;

        foreach (var fileName in fileNames)
        {
            var mediaInfo = FfmpegMediaInfo.Parse(fileName);
            if (mediaInfo.Duration == null || mediaInfo.Dimension.Width == 0 || mediaInfo.Dimension.Height == 0)
            {
                error = true;
            }
            else
            {
                var batchItem = new WhisperJobItem(fileName, string.Empty, mediaInfo);
                Dispatcher.UIThread.Invoke(() => { BatchItems.Add(batchItem); });
            }
        }

        if (error)
        {
            await MessageBox.Show(Window!,
                "Unable to get video info",
                "File skipped as video info was unavailable",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }


    [RelayCommand]
    private void Remove()
    {
        if (SelectedBatchItem != null)
        {
            var idx = BatchItems.IndexOf(SelectedBatchItem);
            BatchItems.Remove(SelectedBatchItem);
        }
    }


    [RelayCommand]
    private void Clear()
    {
        BatchItems.Clear();
    }

    [RelayCommand]
    private async Task ShowAdvancedSettings()
    {
        var vm = await _windowService.ShowDialogAsync<WhisperAdvancedWindow, WhisperAdvancedViewModel>(Window!,
            viewModal =>
            {
                viewModal.Parameters = Parameters;
                viewModal.Engines = Enumerable.ToList<IWhisperEngine>(Engines);
                viewModal.EngineClickedCommand.Execute(SelectedEngine);
            });

        if (vm.OkPressed)
        {
            Parameters = vm.Parameters;
        }
    }

    [RelayCommand]
    private async Task ShowPostProcessingSettings()
    {
        var vm = await _windowService.ShowDialogAsync<WhisperPostProcessingWindow, WhisperPostProcessingViewModel>(
            Window!, viewModal =>
            {
                viewModal.AdjustTimings = Se.Settings.Tools.AudioToText.WhisperAutoAdjustTimings;
                viewModal.FixShortDuration = Se.Settings.Tools.AudioToText.WhisperPostProcessingFixShortDuration;
                viewModal.FixCasing = Se.Settings.Tools.AudioToText.WhisperPostProcessingFixCasing;
                viewModal.AddPeriods = Se.Settings.Tools.AudioToText.WhisperPostProcessingAddPeriods;
                viewModal.MergeShortLines = Se.Settings.Tools.AudioToText.WhisperPostProcessingMergeLines;
                viewModal.BreakSplitLongLines = Se.Settings.Tools.AudioToText.WhisperPostProcessingSplitLines;
            });

        if (vm.OkPressed)
        {
            Se.Settings.Tools.AudioToText.WhisperAutoAdjustTimings = vm.AdjustTimings;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingFixShortDuration = vm.FixShortDuration;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingFixCasing = vm.FixCasing;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingAddPeriods = vm.AddPeriods;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingMergeLines = vm.MergeShortLines;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingSplitLines = vm.BreakSplitLongLines;
        }
    }


    [RelayCommand]
    private async Task DownloadModel()
    {
        var vm = await _windowService.ShowDialogAsync<DownloadWhisperModelsWindow, DownloadWhisperModelsViewModel>(
            Window!, viewModel => { viewModel.SetModels(Models, SelectedEngine, SelectedModel); });

        if (vm.OkPressed)
        {
            RefreshDownloadStatus(vm.SelectedModel?.Model);
        }
    }

    [RelayCommand]
    private async Task Transcribe()
    {
        if (IsBatchMode && BatchItems.Count == 0)
        {
            await Add();

            if (IsBatchMode && BatchItems.Count == 0)
            {
                return;
            }
        }

        if (IsBatchMode && BatchItems.Count > 0)
        {
            _videoFileName = BatchItems[0].InputVideoFileName;
        }

        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        if (SelectedModel is not WhisperModelDisplay model)
        {
            return;
        }

        if (SelectedLanguage is not WhisperLanguage language)
        {
            return;
        }

        if (SelectedEngine is not { } engine)
        {
            return;
        }


        Se.Settings.Tools.AudioToText.WhisperChoice = engine.Choice;

        if (!engine.IsEngineInstalled())
        {
            var answer = await MessageBox.Show(
                Window!,
                $"Download {engine.Name}?",
                $"Download and use {engine.Name}?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            var vm = await _windowService.ShowDialogAsync<DownloadWhisperEngineWindow, DownloadWhisperEngineViewModel>(
                Window!, viewModel =>
                {
                    viewModel.Engine = engine;
                    viewModel.StartDownload();
                });

            if (!vm.OkPressed)
            {
                return;
            }
        }

        if (!engine.IsModelInstalled(model.Model))
        {
            var answer = await MessageBox.Show(
                Window!,
                $"Download {model}?",
                $"Download and use {model.Model.Name}?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            var vm = await _windowService.ShowDialogAsync<DownloadWhisperModelsWindow, DownloadWhisperModelsViewModel>(
                Window!, viewModel =>
                {
                    viewModel.SetModels(Models, SelectedEngine, SelectedModel);
                    viewModel.StartDownload();
                });

            RefreshDownloadStatus(vm.SelectedModel?.Model as WhisperModel);

            return;
        }

        if (language.Code != "en" && IsModelEnglishOnly(model.Model))
        {
            var answer = await MessageBox.Show(
                Window!,
                "Warning",
                "English model should only be used with English language.\nContinue anyway?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        IsTranscribeEnabled = false;
        ConsoleLog = string.Empty;

        if (!IsBatchMode)
        {
            var mediaInfo = FfmpegMediaInfo.Parse(_videoFileName);
            if (mediaInfo.Tracks.Count(p => p.TrackType == FfmpegTrackType.Audio) == 0)
            {
                var answer = await MessageBox.Show(
                    Window!,
                    "No audio track found",
                    $"No audio track was found in {_videoFileName}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                IsTranscribeEnabled = true;
                return;
            }

            BatchItems.Add(new WhisperJobItem(_videoFileName, string.Empty, mediaInfo));
        }
        _batchIndex = 0;

        if (BatchItems.Count == 0)
        {
            return;
        }

        if (IsBatchMode)
        {
            var jobItem = BatchItems[0];
            Dispatcher.UIThread.Post(() =>
            {
                if (BatchGrid == null)
                {
                    return;
                }

                BatchGrid.SelectedItem = jobItem;
                BatchGrid.ScrollIntoView(jobItem, null);
            });
        }


        _videoFileName = BatchItems[0].InputVideoFileName;
        _videoInfo.TotalMilliseconds = BatchItems[0].MediaInfo.Duration.TotalMilliseconds;
        _videoInfo.TotalSeconds = BatchItems[0].MediaInfo.Duration.TotalSeconds;
        _videoInfo.Width = BatchItems[0].MediaInfo.Dimension.Width;
        _videoInfo.Height = BatchItems[0].MediaInfo.Dimension.Height;

        ProgressOpacity = 1;
        ProgressText = "Generating wav file...";
        _startTicks = DateTime.UtcNow.Ticks;

        _batchIndex = 0;
        var startGenerateWaveFileOk = GenerateWavFile(_videoFileName, _audioTrackNumber);
        if (!startGenerateWaveFileOk)
        {
            if (string.IsNullOrEmpty(_error))
            {
                await MessageBox.Show(Window!, "Unknown error", "Unable to start Whisper!");
            }
            else
            {
                await MessageBox.Show(Window!, "Error", "Unable to start Whisper: " + _error);
            }
            _error = string.Empty;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (!IsTranscribeEnabled)
        {
            _abort = true;
            return;
        }

        Window?.Close();
    }

    public bool TranscribeViaWhisper(string waveFileName, string videoFileName)
    {
        if (SelectedEngine is not { } engine)
        {
            return false;
        }

        if (_videoFileName == null)
        {
            return false;
        }

        if (SelectedModel is not WhisperModelDisplay model)
        {
            return false;
        }

        if (SelectedLanguage is not WhisperLanguage language)
        {
            return false;
        }

        var settings = Se.Settings.Tools.AudioToText;
        settings.WhisperChoice = engine.Choice;
        SaveSettings();

        _showProgressPct = -1;
        IsTranscribeEnabled = false;
        ProgressOpacity = 1;
        ProgressText = GetProgressText();

        //if (_batchMode)
        //{

        //    LabelProgress.Text = string.Format("Transcribing {0} of {1}", _batchFileNumber, 0); // TODO: listViewInputFiles.Items.Count);
        //}

        _useCenterChannelOnly = Configuration.Settings.General.FFmpegUseCenterChannelOnly &&
                                FfmpegMediaInfo.Parse(_videoFileName).HasFrontCenterAudio(_audioTrackNumber);

        //Delete invalid preprocessor_config.json file
        if (settings.WhisperChoice is WhisperChoice.PurfviewFasterWhisperXxl)
        {
            var dir = Path.Combine(engine.GetAndCreateWhisperModelFolder(model.Model), model.Model.Folder);
            if (Directory.Exists(dir))
            {
                try
                {
                    var jsonFileName = Path.Combine(dir, "preprocessor_config.json");
                    if (File.Exists(jsonFileName))
                    {
                        var text = FileUtil.ReadAllTextShared(jsonFileName, Encoding.UTF8);
                        if (text.StartsWith("Entry not found", StringComparison.OrdinalIgnoreCase))
                        {
                            File.Delete(jsonFileName);
                        }
                    }

                    jsonFileName = Path.Combine(dir, "vocabulary.json");
                    if (File.Exists(jsonFileName))
                    {
                        var text = FileUtil.ReadAllTextShared(jsonFileName, Encoding.UTF8);
                        if (text.StartsWith("Entry not found", StringComparison.OrdinalIgnoreCase))
                        {
                            File.Delete(jsonFileName);
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        _resultList.Clear();

        var inputFile = waveFileName;
        if (!_useCenterChannelOnly &&
            (engine.Name == WhisperEnginePurfviewFasterWhisperXxl.StaticName) &&
            (videoFileName.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
             videoFileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)) &&
            _audioTrackNumber <= 0)
        {
            inputFile = videoFileName;
        }

        try
        {
            _whisperProcess = GetWhisperProcess(engine, inputFile, model.Model.Name, language.Code, DoTranslateToEnglish,
                OutputHandler);
        }
        catch (Exception e)
        {
            _error = e.Message;
            return false;
        }
        _sw = Stopwatch.StartNew();
        LogToConsole(
            $"Calling whisper ({settings.WhisperChoice}) with : {_whisperProcess.StartInfo.FileName} {_whisperProcess.StartInfo.Arguments}{Environment.NewLine}");

        _abort = false;

        ProgressText = GetProgressText();
        _timerWhisper.Start();

        return true;
    }

    private Process GetWhisperProcess(
        IWhisperEngine engine,
        string waveFileName,
        string model,
        string language,
        bool translate,
        DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var settings = Se.Settings.Tools.AudioToText;
        settings.WhisperCustomCommandLineArguments = settings.WhisperCustomCommandLineArguments.Trim();
        if (settings.WhisperCustomCommandLineArguments == "--standard" &&
            (engine.Name != WhisperEnginePurfviewFasterWhisperXxl.StaticName))
        {
            settings.WhisperCustomCommandLineArguments = string.Empty;
        }

        var translateToEnglish = translate ? GetWhisperTranslateParameter(engine) : string.Empty;
        if (language.ToLowerInvariant() == "english" || language.ToLowerInvariant() == "en")
        {
            language = "en";
            translateToEnglish = string.Empty;
        }

        if (settings.WhisperChoice is WhisperChoice.Cpp or WhisperChoice.CppCuBlas)
        {
            if (!settings.WhisperCustomCommandLineArguments.Contains("--print-progress"))
            {
                translateToEnglish += "--print-progress ";
            }
        }

        var outputSrt = string.Empty;
        var postParams = string.Empty;
        if (settings.WhisperChoice is WhisperChoice.Cpp or WhisperChoice.CppCuBlas or WhisperChoice.ConstMe)
        {
            outputSrt = "--output-srt ";
        }
        else if (settings.WhisperChoice == WhisperChoice.StableTs)
        {
            var srtFileName = Path.GetFileNameWithoutExtension(waveFileName);
            postParams = $" -o {srtFileName}.srt";
        }

        var w = engine.GetExecutable();
        var m = engine.GetModelForCmdLine(model);
        var parameters =
            $"--language {language} --model \"{m}\" {outputSrt}{translateToEnglish}{settings.WhisperCustomCommandLineArguments} \"{waveFileName}\"{postParams}";

        SeLogger.WhisperInfo($"{w} {parameters}");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo(w, parameters)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
            }
        };
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (!string.IsNullOrEmpty(Se.Settings.General.FfmpegPath) &&
                process.StartInfo.EnvironmentVariables["Path"] != null)
            {
                process.StartInfo.EnvironmentVariables["Path"] =
                    process.StartInfo.EnvironmentVariables["Path"]?.TrimEnd(';') + ";" +
                    Path.GetDirectoryName(Se.Settings.General.FfmpegPath);
            }
        }

        var whisperFolder = engine.GetAndCreateWhisperFolder();
        if (!string.IsNullOrEmpty(whisperFolder))
        {
            if (File.Exists(whisperFolder))
            {
                whisperFolder = Path.GetDirectoryName(whisperFolder);
            }

            if (whisperFolder != null)
            {
                process.StartInfo.WorkingDirectory = whisperFolder;
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (!string.IsNullOrEmpty(whisperFolder) && process.StartInfo.EnvironmentVariables["Path"] != null)
            {
                process.StartInfo.EnvironmentVariables["Path"] =
                    process.StartInfo.EnvironmentVariables["Path"]?.TrimEnd(';') + ";" + whisperFolder;
            }
        }

        if (settings.WhisperChoice != WhisperChoice.Cpp &&
            settings.WhisperChoice != WhisperChoice.CppCuBlas &&
            settings.WhisperChoice != WhisperChoice.ConstMe)
        {
            process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
            process.StartInfo.EnvironmentVariables["PYTHONUTF8"] = "1";
        }

        if (dataReceivedHandler != null)
        {
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += dataReceivedHandler;
            process.ErrorDataReceived += dataReceivedHandler;
        }

#pragma warning disable CA1416
        process.Start();
#pragma warning restore CA1416

        if (dataReceivedHandler != null)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        return process;
    }

    public static string GetWhisperTranslateParameter(IWhisperEngine engine)
    {
        return engine.Choice != WhisperEngineCpp.StaticName &&
               engine.Choice != WhisperEngineConstMe.StaticName
            ? "--task translate "
            : "--translate ";
    }

    private bool GenerateWavFile(string videoFileName, int audioTrackNumber)
    {
        if (string.IsNullOrEmpty(videoFileName))
        {
            return false;
        }

        if (videoFileName.EndsWith(".wav"))
        {
            try
            {
                using var waveFile = new WavePeakGenerator2(videoFileName);
                if (waveFile.Header != null && waveFile.Header.SampleRate == 16000)
                {
                    _videoFileName = videoFileName;
                    var startOk = TranscribeViaWhisper(_waveFileName, _videoFileName);
                    return startOk;
                }
            }
            catch
            {
                // ignore
            }
        }

        _ffmpegLog = new StringBuilder();
        _waveFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
        _filesToDelete.Add(_waveFileName);
        _waveExtractProcess = GetFfmpegProcess(videoFileName, audioTrackNumber, _waveFileName);
        if (_waveExtractProcess == null)
        {
            return false;
        }

        _waveExtractProcess.ErrorDataReceived += (sender, args) => { _ffmpegLog.AppendLine(args.Data); };

        _waveExtractProcess.StartInfo.RedirectStandardError = true;
#pragma warning disable CA1416
        var started = _waveExtractProcess.Start();
#pragma warning restore CA1416

        _waveExtractProcess.BeginErrorReadLine();
        _abort = false;
        _timerWaveExtract.Start();
        return true;
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data))
        {
            return;
        }

        if (SelectedLanguage is not WhisperLanguage language)
        {
            return;
        }

        if (outLine.Data.Contains("not all tensors loaded from model file"))
        {
            _incompleteModel = true;
        }

        if (outLine.Data.Contains("error: unknown argument: ", StringComparison.OrdinalIgnoreCase))
        {
            _unknownArgument = true;
        }
        else if (outLine.Data.Contains("error: unrecognized argument: ", StringComparison.OrdinalIgnoreCase))
        {
            _unknownArgument = true;
        }
        else if (outLine.Data.Contains("error: unrecognized arguments: ", StringComparison.OrdinalIgnoreCase))
        {
            _unknownArgument = true;
        }
        else if (outLine.Data.Contains("CUDA failed with error out of memory", StringComparison.OrdinalIgnoreCase))
        {
            _cudaOutOfMemory = true;
        }
        //if (outLine.Data.Contains("running on: CUDA", StringComparison.OrdinalIgnoreCase))
        //{
        //    _runningOnCuda = true;
        //}

        LogToConsole(outLine.Data.Trim() + Environment.NewLine);

        foreach (var line in outLine.Data.SplitToLines())
        {
            if (_timeRegexShort.IsMatch(line))
            {
                var start = line.Substring(1, 10);
                var end = line.Substring(14, 10);
                var text = line.Remove(0, 25).Trim();
                var rt = new ResultText
                {
                    Start = GetSeconds(start),
                    End = GetSeconds(end),
                    Text = Utilities.AutoBreakLine(text, language.Code),
                };

                if (_showProgressPct < 0)
                {
                    _endSeconds = (double)rt.End;
                }

                _resultList.Add(rt);
            }
            else if (_timeRegexLong.IsMatch(line))
            {
                var start = line.Substring(1, 12);
                var end = line.Substring(18, 12);
                var text = line.Remove(0, 31).Trim();
                var rt = new ResultText
                {
                    Start = GetSeconds(start),
                    End = GetSeconds(end),
                    Text = Utilities.AutoBreakLine(text, language.Code),
                };

                if (_showProgressPct < 0)
                {
                    _endSeconds = (double)rt.End;
                }

                _resultList.Add(rt);
            }
            else if (line.StartsWith("whisper_full: progress =", StringComparison.OrdinalIgnoreCase))
            {
                var arr = line.Split('=');
                if (arr.Length == 2)
                {
                    var pctString = arr[1].Trim().TrimEnd('%').TrimEnd();
                    if (double.TryParse(pctString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                            out var pct))
                    {
                        _endSeconds = _videoInfo.TotalSeconds * pct / 100.0;
                        _showProgressPct = pct;
                    }
                }
            }
            else if (_pctWhisper.IsMatch(line.TrimStart()))
            {
                var arr = line.Split('%');
                if (arr.Length > 1 && double.TryParse(arr[0], NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture, out var pct))
                {
                    _endSeconds = _videoInfo.TotalSeconds * pct / 100.0;
                    _showProgressPct = pct;
                }
            }
            else if (_pctWhisperFaster.IsMatch(line))
            {
                var arr = line.Split('%');
                if (arr.Length > 1 && double.TryParse(arr[0].Trim(), NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture, out var pct))
                {
                    _endSeconds = _videoInfo.TotalSeconds * pct / 100.0;
                    _showProgressPct = pct;
                }
            }
        }
    }

    private void LogToConsole(string s)
    {
        _outputText.Enqueue(s);
        ConsoleLog += s.Trim() + "\n";

        Dispatcher.UIThread.Post(() => { TextBoxConsoleLog.CaretIndex = TextBoxConsoleLog.Text?.Length ?? 0; },
           DispatcherPriority.Background);
    }

    private static decimal GetSeconds(string timeCode)
    {
        return (decimal)(TimeCode.ParseToMilliseconds(timeCode) / 1000.0);
    }

    private Process? GetFfmpegProcess(string videoFileName, int audioTrackNumber, string outWaveFile)
    {
        if (!File.Exists(Se.Settings.General.FfmpegPath) && Configuration.IsRunningOnWindows)
        {
            return null;
        }

        var audioParameter = string.Empty;
        if (audioTrackNumber > 0)
        {
            audioParameter = $"-map 0:a:{audioTrackNumber}";
        }

        var fFmpegWaveTranscodeSettings = "-i \"{0}\" -vn -ar 16000 -ac 1 -ab 32k -af volume=1.75 -f wav {2} \"{1}\"";
        if (_useCenterChannelOnly)
        {
            fFmpegWaveTranscodeSettings =
                "-i \"{0}\" -vn -ar 16000 -ab 32k -af volume=1.75 -af \"pan=mono|c0=FC\" -f wav {2} \"{1}\"";
        }

        //-i indicates the input
        //-vn means no video output
        //-ar 44100 indicates the sampling frequency.
        //-ab indicates the bit rate (in this example 160kb/s)
        //-af volume=1.75 will boot volume... 1.0 is normal
        //-ac 2 means 2 channels
        // "-map 0:a:0" is the first audio stream, "-map 0:a:1" is the second audio stream

        var exeFilePath = Se.Settings.General.FfmpegPath;
        if (!File.Exists(exeFilePath))
        {
            exeFilePath = "ffmpeg";
        }

        var parameters = string.Format(fFmpegWaveTranscodeSettings, videoFileName, outWaveFile, audioParameter);
        return new Process
        {
            StartInfo = new ProcessStartInfo(exeFilePath, parameters)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
            }
        };
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    private void RefreshDownloadStatus(WhisperModel? result)
    {
        if (SelectedEngine is not IWhisperEngine engine)
        {
            return;
        }

        if (SelectedModel is not WhisperModelDisplay oldModel)
        {
            return;
        }

        Models.Clear();
        foreach (var model in engine.Models)
        {
            Models.Add(new WhisperModelDisplay
            {
                Model = model,
                Engine = engine,
            });
        }

        if (result != null)
        {
            SelectedModel = Enumerable.FirstOrDefault<WhisperModelDisplay>(Models, m => m.Model.Name == result.Name);
        }
        else
        {
            SelectedModel = Enumerable.FirstOrDefault<WhisperModelDisplay>(Models, m => m.Model.Name == oldModel.Model.Name);
        }
    }

    internal void OnEngineChanged(object? sender, SelectionChangedEventArgs e)
    {
        EngineChanged();
    }

    private void EngineChanged()
    {
        var engine = SelectedEngine;

        Languages.Clear();
        foreach (var language in engine.Languages)
        {
            Languages.Add(language);
        }

        if (!string.IsNullOrEmpty(Se.Settings.Tools.AudioToText.WhisperLanguageCode))
        {
            var language = Enumerable.FirstOrDefault<WhisperLanguage>(Languages, p => p.Code == Se.Settings.Tools.AudioToText.WhisperLanguageCode);
            if (language != null)
            {
                SelectedLanguage = language;
            }
        }
        else
        {
            SelectedLanguage = Enumerable.FirstOrDefault<WhisperLanguage>(Languages, p => p.Name == "English");
        }

        Models.Clear();
        foreach (var model in engine.Models)
        {
            Models.Add(new WhisperModelDisplay
            {
                Model = model,
                Engine = engine,
            });
        }

        if (Models.Count > 0)
        {
            var model = Enumerable.FirstOrDefault<WhisperModelDisplay>(Models, p => p.Model.Name == Se.Settings.Tools.AudioToText.WhisperModel);
            if (model != null)
            {
                SelectedModel = model;
            }
            else
            {
                SelectedModel = Enumerable.FirstOrDefault<WhisperModelDisplay>(Models);
            }
        }

        var isPurfview = engine.Name == WhisperEnginePurfviewFasterWhisperXxl.StaticName;
        if (isPurfview &&
            string.IsNullOrWhiteSpace(Parameters) &&
            !Se.Settings.Tools.AudioToText.WhisperCustomCommandLineArgumentsPurfviewBlank)
        {
            Parameters = "--standard";
        }

        SaveSettings();
    }

    internal void Initialize(string? videoFileName)
    {
        _videoFileName = videoFileName;
        if (string.IsNullOrEmpty(_videoFileName) || !File.Exists(_videoFileName))
        {
            IsBatchModeVisible = false;
            IsSingleModeVisible = false;
            IsBatchMode = true;
        }
        else
        {
            IsBatchModeVisible = true;
            IsSingleModeVisible = false;
            IsBatchMode = false;
        }
    }

    internal void OnWindowLoaded()
    {
        UiUtil.RestoreWindowPosition(Window);
    }

    internal void OnWindowClosing(WindowClosingEventArgs e)
    {
        UiUtil.SaveWindowPosition(Window);
    }

    internal void WindowContextMenuOpening(object? sender, EventArgs e)
    {
        IsWhisperCppActive = SelectedEngine != null && SelectedEngine.Name == WhisperEngineCpp.StaticName;
        IsWhisperPurfviewXxlActive = SelectedEngine != null && SelectedEngine.Name == WhisperEnginePurfviewFasterWhisperXxl.StaticName;
    }
}