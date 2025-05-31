using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using HanumanInstitute.LibMpv;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceSettings;

public partial class ReviewSpeechViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ITtsEngine> _engines;
    [ObservableProperty] private ITtsEngine? _selectedEngine;
    [ObservableProperty] private ObservableCollection<Voice> _voices;
    [ObservableProperty] private Voice? _selectedVoice;
    [ObservableProperty] private ObservableCollection<TtsLanguage> _languages;
    [ObservableProperty] private TtsLanguage? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<string> _regions;
    [ObservableProperty] private string? _selectedRegion;
    [ObservableProperty] private ObservableCollection<string> _models;
    [ObservableProperty] private string? _selectedModel;
    [ObservableProperty] private ObservableCollection<ReviewRow> _lines;
    [ObservableProperty] private ReviewRow? _selectedLine;
    [ObservableProperty] private bool _autoContinue;
    [ObservableProperty] private bool _isPlayVisible;
    [ObservableProperty] private bool _isStopVisible;

    public ReviewSpeechWindow? Window { get; set; }

    public bool OkPressed { get; private set; }

    private MpvContext? _mpvContext;
    private Lock _playLock;
    private readonly Timer _timer;

    public ReviewSpeechViewModel()
    {
        Lines = new ObservableCollection<ReviewRow>();
        Engines = new ObservableCollection<ITtsEngine>();
        Voices = new ObservableCollection<Voice>();
        Languages = new ObservableCollection<TtsLanguage>();
        Regions = new ObservableCollection<string>();
        Models = new ObservableCollection<string>();

        IsPlayVisible = true;

        _playLock = new Lock();
        _timer = new Timer(100);
        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();
    }

    private void OnTimerOnElapsed(object? sender, ElapsedEventArgs args)
    {
        lock (_playLock)
        {
            if (_mpvContext == null)
            {
                IsPlayVisible = true;
                IsStopVisible = false;
            }
            else
            {
                var paused = _mpvContext.Pause.Get() ?? false;
                IsPlayVisible = paused;
                IsStopVisible = !paused;
            }
        }
    }

    private async Task PlayAudio(string fileName)
    {
        lock (_playLock)
        {
            _mpvContext?.Stop();
            _mpvContext?.Dispose();
            _mpvContext = new MpvContext();
        }
        await _mpvContext.LoadFile(fileName).InvokeAsync();
    }

    internal void Initialize(
        TtsStepResult[] stepResults,
        ITtsEngine[] engines,
        ITtsEngine engine,
        Voice[] voices,
        Voice voice,
        TtsLanguage[] languages,
        TtsLanguage? language,
        WavePeakData wavePeakData)
    {
        foreach (var p in stepResults)
        {
            Lines.Add(new ReviewRow
            {
                Include = true,
                Number = p.Paragraph.Number,
                Text = p.Text,
                Voice = p.Voice == null ? string.Empty : p.Voice.ToString(),
                Speed = Math.Round(p.SpeedFactor, 2).ToString(CultureInfo.CurrentCulture),
                Cps = Math.Round(p.Paragraph.GetCharactersPerSecond(), 2).ToString(CultureInfo.CurrentCulture),
                StepResult = p
            });
        }

        Engines.AddRange(engines);
        SelectedEngine = engine;

        Voices.AddRange(voices);
        SelectedVoice = voice;

        Languages.AddRange(languages);
        SelectedLanguage = language;
    }

    [RelayCommand]
    private void EditText()
    {

    }

    [RelayCommand]
    private void RegenerateAudio()
    {
    }

    [RelayCommand]
    private void Play()
    {
    }

    [RelayCommand]
    private void Stop()
    {
    }

    [RelayCommand]
    private void Ok()
    {
        Se.SaveSettings();
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    private void Close()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            Window?.Close();
        });
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void SelectedEngineChanged(object? sender, SelectionChangedEventArgs e)
    {
        var engine = SelectedEngine;
        if (engine == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(async () =>
        {
            //IsEngineSettingsVisible = false;
            var voices = await engine.GetVoices(SelectedLanguage?.Code ?? string.Empty);
            Voices.Clear();
            foreach (var vo in voices)
            {
                Voices.Add(vo);
            }
            //VoiceCount = Voices.Count;

            var lastVoice = Voices.FirstOrDefault(v => v.Name == Se.Settings.Video.TextToSpeech.Voice);
            if (lastVoice == null)
            {
                lastVoice = Voices.FirstOrDefault(p => p.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
                                                       p.Name.Contains("English", StringComparison.OrdinalIgnoreCase));
            }
            SelectedVoice = lastVoice ?? Voices.First();

            //HasLanguageParameter = engine.HasLanguageParameter;
            //HasApiKey = engine.HasApiKey;
            //HasRegion = engine.HasRegion;
            //HasModel = engine.HasModel;

            if (engine.HasLanguageParameter)
            {
                var languages = await engine.GetLanguages(SelectedVoice, null); // SelectedModel);
                Languages.Clear();
                foreach (var language in languages)
                {
                    Languages.Add(language);
                }

                SelectedLanguage = Languages.FirstOrDefault();
            }

            if (engine.HasRegion)
            {
                var regions = await engine.GetRegions();
                Regions.Clear();
                foreach (var region in regions)
                {
                    Regions.Add(region);
                }

                SelectedRegion = Regions.FirstOrDefault();
            }

            if (engine.HasModel)
            {
                var models = await engine.GetModels();
                Models.Clear();
                foreach (var model in models)
                {
                    Models.Add(model);
                }

                SelectedModel = Models.FirstOrDefault();
            }

            if (engine is AzureSpeech)
            {
                //   ApiKey = Se.Settings.Video.TextToSpeech.AzureApiKey;
                SelectedRegion = Se.Settings.Video.TextToSpeech.AzureRegion;
                if (string.IsNullOrEmpty(SelectedRegion))
                {
                    SelectedRegion = "westeurope";
                }
            }
            else if (engine is ElevenLabs)
            {
                //   ApiKey = Se.Settings.Video.TextToSpeech.ElevenLabsApiKey;
                SelectedModel = Se.Settings.Video.TextToSpeech.ElevenLabsModel;
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.First();
                }
                //IsEngineSettingsVisible = true;
            }
        });
    }
}