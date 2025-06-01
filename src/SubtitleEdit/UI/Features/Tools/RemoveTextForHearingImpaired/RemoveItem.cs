using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;
public class RemoveItem 
{
    public bool Apply { get; set; }
    public int Index { get; set; }
    public int IndexDisplay { get; set; }
    public string Before { get; set; }
    public string After { get; set; }
    public SubtitleLineViewModel Paragraph { get; set; }

    public RemoveItem(bool apply, int index, string before, string after, SubtitleLineViewModel paragraph)
    {
        Apply = apply;
        Index = index;
        IndexDisplay = index + 1;
        Before = before;
        After = after;
        Paragraph = paragraph;
    }
}
