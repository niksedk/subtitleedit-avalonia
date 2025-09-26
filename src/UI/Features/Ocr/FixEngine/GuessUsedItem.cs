namespace Nikse.SubtitleEdit.Features.Ocr.FixEngine;

public class GuessUsedItem
{
    public string From { get; set; }
    public string To { get; set; }
    public int LineIndex { get; set; }

    public GuessUsedItem()
    {
    }
    
    public GuessUsedItem(string from, string to, int lineLineIndex)
    {
        From = from;
        To = to;
        LineIndex = lineLineIndex;
    }

    public override string ToString()
    {
        return $"#{LineIndex}: {From} -> {To}";
    }
}
