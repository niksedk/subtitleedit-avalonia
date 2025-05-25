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

    [ObservableProperty]
    private string _originalText;

    public Paragraph? Paragraph { get; set; }
    public string Extra { get;  set; }
    public string Language { get;  set; }
    public string Region { get;  set; }
    public string Style { get;  set; }
    public string Actor { get; set; }
    public int Layer { get; set; }


    public SubtitleLineViewModel()
    {
        Text = string.Empty;
        Extra = string.Empty;
        Language = string.Empty;
        Region = string.Empty;
        Style = string.Empty;
        Actor = string.Empty;
        Layer = 0;
    }

    public SubtitleLineViewModel(SubtitleLineViewModel p, bool generateNewId = false)
    {
        Text = p.Text;
        StartTime = p.StartTime;
        EndTime = p.EndTime;
        Duration = p.Duration;
        Language = p.Language;
        Region = p.Region;
        Style = p.Style;
        Actor = p.Actor;
        Layer = p.Layer;
        Extra = p.Extra;
    }

    public SubtitleLineViewModel(Paragraph newParagraph)
    {
        Text = newParagraph.Text;
        Extra = newParagraph.Extra;
        Language = newParagraph.Language;
        Region = newParagraph.Region;
        Style = newParagraph.Style;
        Actor = newParagraph.Actor;
        Layer = newParagraph.Layer;
        StartTime = TimeSpan.FromMilliseconds(newParagraph.StartTime.TotalMilliseconds);
        EndTime = TimeSpan.FromMilliseconds(newParagraph.EndTime.TotalMilliseconds);
        UpdateDuration();
    }

    internal void UpdateDuration()
    {
        Duration = EndTime - StartTime;
    }
}