using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.ObjectModel;
using System.Threading;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

public partial class TextToSpeechViewModel : ObservableObject
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

    [ObservableProperty] private bool _hasLanguageParameter;
    [ObservableProperty] private bool _hasApiKey;
    [ObservableProperty] private string _apiKey;
    [ObservableProperty] private bool _hasRegion;
    [ObservableProperty] private string _region;
    [ObservableProperty] private bool _hasModel;
    [ObservableProperty] private int _voiceCount;
    [ObservableProperty] private string _voiceCountInfo;
    [ObservableProperty] private string _voiceTestText;
    [ObservableProperty] private bool _doReviewAudioClips;
    [ObservableProperty] private bool _doGenerateVideoFile;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _isEngineSettingsVisible;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _doneOrCancelText;

    //public TextToSpeechPage? Page { get; set; }
    //public MediaElement Player { get; set; }
    //public Label LabelAudioEncodingSettings { get; set; }
    //public Label LabelEngineSettings { get; set; }

    private Subtitle _subtitle = new();
    private readonly IFileHelper _fileHelper;
    private readonly string _waveFolder;
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationToken _cancellationToken;
    private WavePeakData _wavePeakData;
    private FfmpegMediaInfo? _mediaInfo;
    private string _videoFileName = string.Empty;
    private bool _isMerging;

    public TextToSpeechWindow? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    public TextToSpeechViewModel()
    {
        Engines = new ObservableCollection<ITtsEngine>();
        Voices = new ObservableCollection<Voice>();
        Regions = new ObservableCollection<string>();
        Models = new ObservableCollection<string>();
        Languages = new ObservableCollection<TtsLanguage>();
        ApiKey = string.Empty;
        Region = string.Empty;
        VoiceCountInfo = string.Empty;
        VoiceTestText = "The quick brown fox jumps over the lazy dog.";
        ProgressText = string.Empty;
        DoneOrCancelText = string.Empty;

        _cancellationTokenSource = new CancellationTokenSource();
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
}