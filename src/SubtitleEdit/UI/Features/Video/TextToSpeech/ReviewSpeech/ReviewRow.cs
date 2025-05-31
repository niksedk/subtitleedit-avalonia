using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

public partial class ReviewRow : ObservableObject
{
    [ObservableProperty] private bool _include;
    [ObservableProperty] private int _number;
    [ObservableProperty] private string _voice;
    [ObservableProperty] private string _cps;
    [ObservableProperty] private string _speed;
    [ObservableProperty] private Color _speedBackgroundColor;
    [ObservableProperty] private string _text;

    public TtsStepResult StepResult { get; set; }

    public ReviewRow()
    {
        Include = true;
        Number = 0;
        Voice = string.Empty;
        Cps = string.Empty;
        Speed = string.Empty;
        SpeedBackgroundColor = Colors.AliceBlue; //TODO: (Color)Application.Current!.Resources[ThemeNames.BackgroundColor];
        Text = string.Empty;
        StepResult = new TtsStepResult();
    }
}