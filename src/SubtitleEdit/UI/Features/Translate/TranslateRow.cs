using System;

namespace Nikse.SubtitleEdit.Features.Translate;

public class TranslateRow
{
    public int Number { get; set; }
    public TimeSpan Show { get; set; }
    public TimeSpan Hide { get; set; }
    public string Duration { get; set; }
    public string Text { get; set; }
    public string TranslatedText { get; set; }

    public double DurationTotalMilliseconds => (Hide - Show).TotalMilliseconds;

    public TranslateRow()
    {
        Duration = string.Empty;
        Text = string.Empty;
        TranslatedText = string.Empty;
    }
}