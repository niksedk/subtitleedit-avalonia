using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class SubtitleLineViewModel : ObservableObject
{
    [ObservableProperty]
    private int _number;

    [ObservableProperty]
    private TimeSpan _startTime;

    [ObservableProperty]
    private TimeSpan _endTime;

    [ObservableProperty]
    private TimeSpan _duration;

    [ObservableProperty]
    private string _text;

    public SubtitleLineViewModel()
    {
        Text = string.Empty;
    }
    
    public SubtitleLineViewModel(Paragraph newParagraph)
    {
        Text = newParagraph.Text;
    }

    public Paragraph? Paragraph { get; set; }
}