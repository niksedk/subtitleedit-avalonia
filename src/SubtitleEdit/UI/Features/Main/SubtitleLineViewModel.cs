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
    public Guid Id { get; set; }


    public SubtitleLineViewModel()
    {
        Text = string.Empty;
        OriginalText = string.Empty;
        Extra = string.Empty;
        Language = string.Empty;
        Region = string.Empty;
        Style = string.Empty;
        Actor = string.Empty;
        Layer = 0;
        Id = Guid.NewGuid();
    }

    public SubtitleLineViewModel(SubtitleLineViewModel p, bool generateNewId = false)
    {
        Text = p.Text;
        OriginalText = p.OriginalText;
        StartTime = p.StartTime;
        EndTime = p.EndTime;
        Duration = p.Duration;
        Language = p.Language;
        Region = p.Region;
        Style = p.Style;
        Actor = p.Actor;
        Layer = p.Layer;
        Number = p.Number;
        Extra = p.Extra;
        Id = generateNewId ? Guid.NewGuid() : p.Id;
    }

    public SubtitleLineViewModel(Paragraph paragraph)
    {
        Text = paragraph.Text;
        OriginalText = string.Empty;
        Extra = paragraph.Extra;
        Language = paragraph.Language;
        Region = paragraph.Region;
        Style = paragraph.Style;
        Actor = paragraph.Actor;
        Layer = paragraph.Layer;
        Number = paragraph.Number;
        StartTime = TimeSpan.FromMilliseconds(paragraph.StartTime.TotalMilliseconds);
        EndTime = TimeSpan.FromMilliseconds(paragraph.EndTime.TotalMilliseconds);
        UpdateDuration();
        Id = Guid.TryParse(paragraph.Id, out var guid) ? guid : Guid.NewGuid();
    }
    
    partial void OnStartTimeChanged(TimeSpan value)
    {
        UpdateDuration();
    }

    partial void OnEndTimeChanged(TimeSpan value)
    {
        UpdateDuration();
    }

    internal void UpdateDuration()
    {
        var newDuration = EndTime - StartTime;
        if (Math.Abs(newDuration.TotalMilliseconds - Duration.TotalMilliseconds) > 0.001)
        {
            Duration = EndTime - StartTime;
        }
    }
}