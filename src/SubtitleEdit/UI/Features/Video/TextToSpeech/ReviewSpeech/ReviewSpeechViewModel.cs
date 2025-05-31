using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceSettings;

public partial class ReviewSpeechViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ITtsEngine> _engines;
    [ObservableProperty] private ITtsEngine? _selectedEngine;
    [ObservableProperty] private ObservableCollection<Voice> _voices;
    [ObservableProperty] private Voice? _selectedVoice;
    [ObservableProperty] private TtsStepResult? _selectedRow;

    public ObservableCollection<TtsStepResult> Rows { get; set; }
    public ReviewSpeechWindow? Window { get; set; }

    public bool OkPressed { get; private set; }

    public ReviewSpeechViewModel()
    {
        Rows = new ObservableCollection<TtsStepResult>();
        Engines = new ObservableCollection<ITtsEngine>();
        Voices = new ObservableCollection<Voice>();
    }

    internal void Initialize(TtsStepResult[] stepResults, ITtsEngine[] engines, ITtsEngine engine, Voice[] voices, Voice voice, Core.Common.WavePeakData wavePeakData)
    {
        Rows.AddRange(stepResults);
        Engines.AddRange(engines);
        SelectedEngine = engine;
        Voices.AddRange(voices);
        SelectedVoice = voice;
    }

    [RelayCommand]
    private void Ok()
    {
        Se.SaveSettings();
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
}