using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Main;
using System;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

public partial class CompareItem : ObservableObject
{
    [ObservableProperty] private int _number;
    [ObservableProperty] private TimeSpan _startTime;
    [ObservableProperty] private TimeSpan _endTime;
    [ObservableProperty] private TimeSpan _duration;
    [ObservableProperty] private string _text;
    [ObservableProperty] private string _originalText;

    [ObservableProperty] private IBrush _numberBackgroundBrush;
    [ObservableProperty] private IBrush _startTimeBackgroundBrush;
    [ObservableProperty] private IBrush _endTimeBackgroundBrush;
    [ObservableProperty] private IBrush _textBackgroundBrush;

    public bool IsDefault => Text == string.Empty && Number == 0 && Duration == TimeSpan.Zero && StartTime == TimeSpan.Zero;


    public CompareItem()
    {
        Text = string.Empty;
        OriginalText = string.Empty;
        StartTime = TimeSpan.Zero;
        EndTime = TimeSpan.Zero;
        Duration = TimeSpan.Zero;
    }

    public CompareItem(SubtitleLineViewModel line)
    {
        Text = line.Text;
        OriginalText = line.OriginalText;
        StartTime = line.StartTime;
        EndTime = line.EndTime;
        Duration = line.Duration;
        Number = line.Number;
    }
}
