namespace Nikse.SubtitleEdit.Features.Translate;

public class TranslateRow
{
    public int Number { get; set; }
    public string Show { get; set; }
    public string Duration { get; set; }
    public string Text { get; set; }
    public string TranslatedText { get; set; }

    public TranslateRow()
    {
        Show = string.Empty;
        Duration = string.Empty;
        Text = string.Empty;
        TranslatedText = string.Empty;
    }
}