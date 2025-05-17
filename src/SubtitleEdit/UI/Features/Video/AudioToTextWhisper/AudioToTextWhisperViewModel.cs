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
    private readonly Timer _timerWhisper = new();
    private Process _whisperProcess = new();
    private Process? _waveExtractProcess = new();
    private readonly Timer _timerWaveExtract = new();
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
            viewModal.FixShortDuration = Se.Settings.Tools.AudioToText.WhisperPostProcessingFixShortDuration;
            viewModal.FixCasing = Se.Settings.Tools.AudioToText.WhisperPostProcessingFixCasing;
            viewModal.AddPeriods = Se.Settings.Tools.AudioToText.WhisperPostProcessingAddPeriods;
            viewModal.MergeShortLines = Se.Settings.Tools.AudioToText.WhisperPostProcessingMergeLines;
            viewModal.BreakSplitLongLines = Se.Settings.Tools.AudioToText.WhisperPostProcessingSplitLines;
        });

        if (vm.OkPressed)
        {
            Se.Settings.Tools.AudioToText.WhisperPostProcessingFixShortDuration = vm.FixShortDuration;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingFixCasing = vm.FixCasing;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingAddPeriods = vm.AddPeriods;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingMergeLines = vm.MergeShortLines;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingSplitLines = vm.BreakSplitLongLines;
        }
    }

    [RelayCommand]
    private void Transcribe()
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

    internal void OnEngineChanged(object? sender, SelectionChangedEventArgs e)
    {
        var engine = SelectedEngine;

        Languages.Clear();
        foreach (var language in engine.Languages)
        {
            Languages.Add(language);
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

        var isPurfview = engine.Name == WhisperEnginePurfviewFasterWhisperXxl.StaticName;
        if (isPurfview &&
            string.IsNullOrWhiteSpace(Parameters) &&
            !Se.Settings.Tools.AudioToText.WhisperCustomCommandLineArgumentsPurfviewBlank)
        {
            Parameters = "--standard";
        }
    }
}