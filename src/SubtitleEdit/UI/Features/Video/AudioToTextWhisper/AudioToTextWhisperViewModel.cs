using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper.Engines;
using System.Timers;
using Nikse.SubtitleEdit.Logic;
using System.Runtime.InteropServices;
using System;
using Avalonia.Threading;
using Avalonia.Controls;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Logic.Config;
using System.Globalization;
using Nikse.SubtitleEdit.Core.TextToSpeech;
using System.Threading;
using Nikse.SubtitleEdit.Features.Common;

namespace Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;

public partial class AudioToTextWhisperViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<IWhisperEngine> _engines;
    [ObservableProperty] private IWhisperEngine _selectedEngine;

    [ObservableProperty] private ObservableCollection<WhisperLanguage> _languages;
    [ObservableProperty] private WhisperLanguage? _selectedLanguage;

    [ObservableProperty] private ObservableCollection<WhisperModelDisplay> _models;
    [ObservableProperty] private WhisperModelDisplay? _selectedModel;

    [ObservableProperty] private bool _doTranslateToEnglish;
    [ObservableProperty] private bool _doAdjustTimings;
    [ObservableProperty] private bool _doPostProcessing;

    [ObservableProperty] private string _parameters;

    [ObservableProperty] private string _consoleLog;

    [ObservableProperty] private bool _isTranscribeEnabled;

    [ObservableProperty] private float _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private string _elapsedText;
    [ObservableProperty] private string _estimatedText;

    public AudioToTextWhisperWindow? Window { get; set; }

    public bool OkPressed { get; private set; }

    private bool _unknownArgument;
    private bool _cudaOutOfMemory;
    private bool _incompleteModel;
    private string? _videoFileName;
    private string _waveFileName = string.Empty;
    private int _audioTrackNumber;
    private readonly List<string> _filesToDelete = new();
    private readonly ConcurrentBag<string> _outputText = new();
    private long _startTicks = 0;
    private double _endSeconds;
    private double _showProgressPct = -1;
    private double _lastEstimatedMs = double.MaxValue;
    private readonly VideoInfo _videoInfo = new();
    private bool _abort;
    private readonly List<ResultText> _resultList = new();
    private bool _useCenterChannelOnly;
    private readonly Regex _timeRegexShort = new(@"^\[\d\d:\d\d[\.,]\d\d\d --> \d\d:\d\d[\.,]\d\d\d\]", RegexOptions.Compiled);
    private readonly Regex _timeRegexLong = new(@"^\[\d\d:\d\d:\d\d[\.,]\d\d\d --> \d\d:\d\d:\d\d[\.,]\d\d\d]", RegexOptions.Compiled);
    private readonly Regex _pctWhisper = new(@"^\d+%\|", RegexOptions.Compiled);
    private readonly Regex _pctWhisperFaster = new(@"^\s*\d+%\s*\|", RegexOptions.Compiled);
    private readonly System.Timers.Timer _timerWhisper = new();
    private Process _whisperProcess = new();
    private Process? _waveExtractProcess = new();
    private readonly System.Timers.Timer _timerWaveExtract = new();
    private Stopwatch _sw = new();
    private StringBuilder _ffmpegLog = new();

    IWindowService _windowService;

    public AudioToTextWhisperViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Engines = new ObservableCollection<IWhisperEngine>();
        Engines.Add(new WhisperEngineCpp());
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Engines.Add(new WhisperEnginePurfviewFasterWhisperXxl());
            Engines.Add(new WhisperEngineOpenAi());
            Engines.Add(new WhisperEngineConstMe());
        }
        SelectedEngine = Engines[0];

        Languages = new ObservableCollection<WhisperLanguage>(SelectedEngine.Languages);
        SelectedLanguage = Languages.FirstOrDefault(p => p.Name == "English");

        Models = new ObservableCollection<WhisperModelDisplay>();

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

        EngineChanged();
    }

    private void SaveSettings()
    {
        DoAdjustTimings = Se.Settings.Tools.AudioToText.WhisperAutoAdjustTimings;
        DoPostProcessing = Se.Settings.Tools.AudioToText.PostProcessing;
        Parameters = Se.Settings.Tools.AudioToText.WhisperCustomCommandLineArguments;

        Se.Settings.Tools.AudioToText.WhisperAutoAdjustTimings = DoAdjustTimings;
        Se.Settings.Tools.AudioToText.WhisperCustomCommandLineArguments = Parameters;
        Se.Settings.Tools.AudioToText.WhisperCustomCommandLineArgumentsPurfviewBlank = Parameters == "--standard";
        Se.Settings.Tools.AudioToText.WhisperChoice = SelectedEngine.Choice;
        Se.Settings.Tools.AudioToText.WhisperModel = SelectedModel?.Model.Name ?? string.Empty;

        Se.SaveSettings();
    }

    private void OnTimerWhisperOnElapsed(object? sender, ElapsedEventArgs args)
    {
        if (_abort)
        {
            _timerWhisper.Stop();
#pragma warning disable CA1416
            _whisperProcess.Kill(true);
#pragma warning restore CA1416

            Dispatcher.UIThread.Invoke(() =>
            {
                //ProgressBar.IsVisible = false;
                //var partialSub = new Subtitle();
                //partialSub.Paragraphs.AddRange(_resultList.OrderBy(p => p.Start).Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());

                //if (partialSub.Paragraphs.Count > 0)
                //{
                //    var answer = await Page.DisplayAlert(
                //        $"Keep partial transcription?",
                //        $"Do you want to keep {partialSub.Paragraphs.Count} lines?",
                //        "Yes",
                //        "No");

                //    if (!answer)
                //    {
                //        _resultList.Clear();
                //        partialSub.Paragraphs.Clear();
                //        await Shell.Current.GoToAsync("..");
                //        return;
                //    }
                //}

                //await MakeResult(partialSub);
            });

            return;
        }

        if (!_whisperProcess.HasExited)
        {
            //var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
            //Dispatcher.UIThread.Invoke(() =>
            //{
            //    LabelProgress.Text = "Transcribing...";
            //});

            //ElapsedText = $"Time elapsed: {new TimeCode(durationMs).ToShortDisplayString()}";
            //if (_endSeconds <= 0)
            //{
            //    if (_showProgressPct > 0)
            //    {
            //        SetProgressBarPct(_showProgressPct);
            //    }

            //    return;
            //}

            //ShowProgressBar();

            //_videoInfo.TotalSeconds = Math.Max(_endSeconds, _videoInfo.TotalSeconds);
            //var msPerFrame = durationMs / (_endSeconds * 1000.0);
            //var estimatedTotalMs = msPerFrame * _videoInfo.TotalMilliseconds;
            //var msEstimatedLeft = estimatedTotalMs - durationMs;
            //if (msEstimatedLeft > _lastEstimatedMs)
            //{
            //    msEstimatedLeft = _lastEstimatedMs;
            //}
            //else
            //{
            //    _lastEstimatedMs = msEstimatedLeft;
            //}

            //if (_showProgressPct > 0)
            //{
            //    SetProgressBarPct(_showProgressPct);
            //}
            //else
            //{
            //    SetProgressBarPct(_endSeconds * 100.0 / _videoInfo.TotalSeconds);
            //}

            //EstimatedText = ProgressHelper.ToProgressTime(msEstimatedLeft);

            return;
        }

        _timerWhisper.Stop();

        Dispatcher.UIThread.Invoke(() =>
        {
            //await ProgressBar.ProgressTo(1, 500, Easing.Linear);

            //LogToConsole($"Whisper ({_settings.WhisperChoice}) done in {_sw.Elapsed}{Environment.NewLine}");

            //_whisperProcess.Dispose();

            //if (GetResultFromSrt(_waveFileName, _videoFileName!, out var resultTexts, _outputText, _filesToDelete))
            //{
            //    var subtitle = new Subtitle();
            //    subtitle.Paragraphs.AddRange(resultTexts
            //        .Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());

            //    var postProcessedSubtitle = PostProcess(subtitle);
            //    await MakeResult(postProcessedSubtitle);

            //    return;
            //}

            //_outputText.Add("Loading result from STDOUT" + Environment.NewLine);

            //var transcribedSubtitleFromStdOut = new Subtitle();
            //transcribedSubtitleFromStdOut.Paragraphs.AddRange(_resultList.OrderBy(p => p.Start)
            //    .Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());
            //await MakeResult(transcribedSubtitleFromStdOut);
        });
    }

    private void OnTimerWaveExtractOnElapsed(object? sender, ElapsedEventArgs e)
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
            // ProgressBar.IsVisible = false;

            //  TranscribeButton.IsEnabled = true;
            //  return;
        }

        if (!_waveExtractProcess.HasExited)
        {
            var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
            ElapsedText = $"Time elapsed: {new TimeCode(durationMs).ToShortDisplayString()}";

            return;
        }

        _timerWaveExtract.Stop();

        //if (!File.Exists(_waveFileName))
        //{
        //    SeLogger.WhisperInfo("Generated wave file not found: " + _waveFileName + Environment.NewLine +
        //                         "ffmpeg: " + _waveExtractProcess.StartInfo.FileName + Environment.NewLine +
        //                         "Parameters: " + _waveExtractProcess.StartInfo.Arguments + Environment.NewLine +
        //                         "OS: " + Environment.OSVersion + Environment.NewLine +
        //                         "64-bit: " + Environment.Is64BitOperatingSystem + Environment.NewLine +
        //                         "ffmpeg exit code: " + _waveExtractProcess.ExitCode + Environment.NewLine +
        //                         "ffmpeg log: " + _ffmpegLog);
        //    TranscribeButton.IsEnabled = true;
        //    return;
        //}

        //if (string.IsNullOrEmpty(_videoFileName))
        //{
        //    TranscribeButton.IsEnabled = true;
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        Page?.DisplayAlert("No video file", "No video file found!", "OK");
        //    });

        //    return;
        //}

        //var startOk = TranscribeViaWhisper(_waveFileName, _videoFileName);
        //if (!startOk)
        //{
        //    TranscribeButton.IsEnabled = true;
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        Page?.DisplayAlert("Unknown error", "Unable to start Whisper!", "OK");
        //    });
        //}
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
    private async Task ShowAdvancedSettings()
    {
        var vm = await _windowService.ShowDialogAsync<WhisperAdvancedWindow, WhisperAdvancedViewModel>(Window!, viewModal =>
        {
            viewModal.Parameters = Parameters;
            viewModal.Engines = Engines.ToList();
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
        var vm = await _windowService.ShowDialogAsync<WhisperPostProcessingWindow, WhisperPostProcessingViewModel>(Window!, viewModal =>
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
        var vm = await _windowService.ShowDialogAsync<DownloadWhisperModelsWindow, DownloadWhisperModelsViewModel>(Window!, viewModel =>
        {
            viewModel.SetModels(Models, SelectedEngine, SelectedModel);
        });

        if (vm.OkPressed)
        {
            RefreshDownloadStatus(vm.SelectedModel?.Model);
        }
    }

    [RelayCommand]
    private async Task Transcribe()
    {
        if (SelectedEngine is null || string.IsNullOrEmpty(_videoFileName))
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

            var vm = await _windowService.ShowDialogAsync<DownloadWhisperEngineWindow, DownloadWhisperEngineViewModel>(Window!, viewModel =>
            {
                //viewModel.Engine = engine;
                viewModel.StartDownload();
            });
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

            var vm = await _windowService.ShowDialogAsync<DownloadWhisperModelsWindow, DownloadWhisperModelsViewModel>(Window!, viewModel =>
            {
                viewModel.SetModels(Models, SelectedEngine, SelectedModel);
            });

            RefreshDownloadStatus(vm.SelectedModel?.Model as WhisperModel);

            return;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
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

        ProgressText = "Transcribing...";

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

        _whisperProcess = GetWhisperProcess(engine, inputFile, model.Model.Name, language.Code, DoTranslateToEnglish, OutputHandler);
        _sw = Stopwatch.StartNew();
        LogToConsole($"Calling whisper ({settings.WhisperChoice}) with : {_whisperProcess.StartInfo.FileName} {_whisperProcess.StartInfo.Arguments}{Environment.NewLine}");

        _abort = false;

        ProgressText = "Transcribing...";
        _timerWhisper.Start();

        return true;
    }

    public static Process GetWhisperProcess(
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
        var parameters = $"--language {language} --model \"{m}\" {outputSrt}{translateToEnglish}{settings.WhisperCustomCommandLineArguments} \"{waveFileName}\"{postParams}";

        SeLogger.WhisperInfo($"{w} {parameters}");

        var process = new Process { StartInfo = new ProcessStartInfo(w, parameters) { WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true } };
#if WINDOWS        
        if (!string.IsNullOrEmpty(Se.Settings.General.FfmpegPath) && process.StartInfo.EnvironmentVariables["Path"] != null)
        {
            process.StartInfo.EnvironmentVariables["Path"] = process.StartInfo.EnvironmentVariables["Path"]?.TrimEnd(';') + ";" + Path.GetDirectoryName(Se.Settings.General.FfmpegPath);
        }
#endif

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

#if WINDOWS        
        if (!string.IsNullOrEmpty(whisperFolder) && process.StartInfo.EnvironmentVariables["Path"] != null)
        {
            process.StartInfo.EnvironmentVariables["Path"] = process.StartInfo.EnvironmentVariables["Path"]?.TrimEnd(';') + ";" + whisperFolder;
        }
#endif

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
               engine.Choice != WhisperEngineConstMe.StaticName ? "--task translate " : "--translate ";
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
                using var waveFile = new WavePeakGenerator(videoFileName);
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

        _waveExtractProcess.ErrorDataReceived += (sender, args) =>
        {
            _ffmpegLog.AppendLine(args.Data);
        };

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
                    if (double.TryParse(pctString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var pct))
                    {
                        _endSeconds = _videoInfo.TotalSeconds * pct / 100.0;
                        _showProgressPct = pct;
                    }
                }
            }
            else if (_pctWhisper.IsMatch(line.TrimStart()))
            {
                var arr = line.Split('%');
                if (arr.Length > 1 && double.TryParse(arr[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var pct))
                {
                    _endSeconds = _videoInfo.TotalSeconds * pct / 100.0;
                    _showProgressPct = pct;
                }
            }
            else if (_pctWhisperFaster.IsMatch(line))
            {
                var arr = line.Split('%');
                if (arr.Length > 1 && double.TryParse(arr[0].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var pct))
                {
                    _endSeconds = _videoInfo.TotalSeconds * pct / 100.0;
                    _showProgressPct = pct;
                }
            }
        }
    }

    private void LogToConsole(string s)
    {
        _outputText.Add(s);
        ConsoleLog += s + "\n";
        //MainThread.BeginInvokeOnMainThread(() =>
        //{
        //   // ConsoleTextScrollView.ScrollToAsync(0, ConsoleText.Height, true);
        //});
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
            fFmpegWaveTranscodeSettings = "-i \"{0}\" -vn -ar 16000 -ab 32k -af volume=1.75 -af \"pan=mono|c0=FC\" -f wav {2} \"{1}\"";
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
            SelectedModel = Models.FirstOrDefault(m => m.Model.Name == result.Name);
        }
        else
        {
            SelectedModel = Models.FirstOrDefault(m => m.Model.Name == oldModel.Model.Name);
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
            var language = Languages.FirstOrDefault(p => p.Code == Se.Settings.Tools.AudioToText.WhisperLanguageCode);
            if (language != null)
            {
                SelectedLanguage = language;
            }
        }
        else
        {
            SelectedLanguage = Languages.FirstOrDefault(p => p.Name == "English");
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
            var model = Models.FirstOrDefault(p => p.Model.Name == Se.Settings.Tools.AudioToText.WhisperModel);
            if (model != null)
            {
                SelectedModel = model;
            }
            else
            {
                SelectedModel = Models.FirstOrDefault();
            }
        }

        var isPurfview = engine.Name == WhisperEnginePurfviewFasterWhisperXxl.StaticName;
        if (isPurfview &&
            string.IsNullOrWhiteSpace(Parameters) &&
            !Se.Settings.Tools.AudioToText.WhisperCustomCommandLineArgumentsPurfviewBlank)
        {
            Parameters = "--standard";
        }
    }
}