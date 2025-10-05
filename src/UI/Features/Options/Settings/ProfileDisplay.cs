using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class ProfileDisplay : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _selectedProfile;
    [ObservableProperty] private int? _singleLineMaxLength;
    [ObservableProperty] private double? _optimalCharsPerSec;
    [ObservableProperty] private double? _maxCharsPerSec;
    [ObservableProperty] private double? _maxWordsPerMin;
    [ObservableProperty] private int? _minDurationMs;
    [ObservableProperty] private int? _maxDurationMs;
    [ObservableProperty] private int? _minGapMs;
    [ObservableProperty] private int? _maxLines;
    [ObservableProperty] private int? _unbreakLinesShorterThan;
    [ObservableProperty] private ObservableCollection<DialogStyleDisplay> _dialogStyles;
    [ObservableProperty] private DialogStyleDisplay _dialogStyle;
    [ObservableProperty] private ObservableCollection<ContinuationStyleDisplay> _continuationStyles;
    [ObservableProperty] private ContinuationStyleDisplay _continuationStyle;
    [ObservableProperty] private ObservableCollection<CpsLineLengthStrategyDisplay> _cpsLineLengthStrategies;
    [ObservableProperty] private CpsLineLengthStrategyDisplay _cpsLineLengthStrategy;

    public ProfileDisplay()
    {
        Name = string.Empty;
        SelectedProfile = string.Empty;
        SingleLineMaxLength = null;
        OptimalCharsPerSec = null;
        MaxCharsPerSec = null;
        MaxWordsPerMin = null;
        MinDurationMs = null;
        MaxDurationMs = null;
        MinGapMs = null;
        MaxLines = null;
        UnbreakLinesShorterThan = null;
        DialogStyles = new(DialogStyleDisplay.List());
        DialogStyle = DialogStyles.Count > 0 ? DialogStyles[0] : null!;
        ContinuationStyles = new(ContinuationStyleDisplay.List());
        ContinuationStyle = ContinuationStyles.Count > 0 ? ContinuationStyles[0] : null!;
        CpsLineLengthStrategies = new(CpsLineLengthStrategyDisplay.List());
        CpsLineLengthStrategy = CpsLineLengthStrategies.Count > 0 ? CpsLineLengthStrategies[0] : null!;

    }

    override public string ToString()
    {
        return Name;
    }
}
