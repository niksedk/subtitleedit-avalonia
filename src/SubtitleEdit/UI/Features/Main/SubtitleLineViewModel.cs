using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class SubtitleLineViewModel : ObservableObject
{
    [ObservableProperty]
    private int number;

    [ObservableProperty]
    private TimeSpan startTime;

    [ObservableProperty]
    private TimeSpan endTime;

    [ObservableProperty]
    private TimeSpan duration;

    [ObservableProperty]
    private string text;

    public SubtitleLineViewModel()
    {
    }
    
    public SubtitleLineViewModel(Paragraph newParagraph)
    {
    }

    public Paragraph? Paragraph { get; set; }
}