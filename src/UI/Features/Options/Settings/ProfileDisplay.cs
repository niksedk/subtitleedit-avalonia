using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class ProfileDisplay : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private int? _singleLineMaxLength;
    [ObservableProperty] private double? _optimalCharsPerSec;
    [ObservableProperty] private double? _maxCharsPerSec;
    [ObservableProperty] private double? _maxWordsPerMin;
    [ObservableProperty] private int? _minDurationMs;
    [ObservableProperty] private int? _maxDurationMs;
    [ObservableProperty] private int? _minGapMs;
    [ObservableProperty] private int? _maxLines;
    [ObservableProperty] private int? _unbreakLinesShorterThan;
    [ObservableProperty] private DialogStyleDisplay _dialogStyle;
    [ObservableProperty] private ContinuationStyleDisplay _continuationStyle;
    [ObservableProperty] private CpsLineLengthStrategyDisplay _cpsLineLengthStrategy;
    [ObservableProperty] private bool _isSelected;

    public ProfileDisplay()
    {
        Name = string.Empty;
        SingleLineMaxLength = null;
        OptimalCharsPerSec = null;
        MaxCharsPerSec = null;
        MaxWordsPerMin = null;
        MinDurationMs = null;
        MaxDurationMs = null;
        MinGapMs = null;
        MaxLines = null;
        UnbreakLinesShorterThan = null;
        DialogStyle = DialogStyleDisplay.List().First();
        ContinuationStyle = ContinuationStyleDisplay.List().First();
        CpsLineLengthStrategy = CpsLineLengthStrategyDisplay.List().First();
    }

    public ProfileDisplay(ProfileDisplay other)
    {
        Name = other.Name;
        SingleLineMaxLength = other.SingleLineMaxLength;
        OptimalCharsPerSec = other.OptimalCharsPerSec;
        MaxCharsPerSec = other.MaxCharsPerSec;
        MaxWordsPerMin = other.MaxWordsPerMin;
        MinDurationMs = other.MinDurationMs;
        MaxDurationMs = other.MaxDurationMs;
        MinGapMs = other.MinGapMs;
        MaxLines = other.MaxLines;
        UnbreakLinesShorterThan = other.UnbreakLinesShorterThan;
        DialogStyle = other.DialogStyle;
        ContinuationStyle = other.ContinuationStyle;
        CpsLineLengthStrategy = other.CpsLineLengthStrategy;
    }

    override public string ToString()
    {
        return Name;
    }
}
