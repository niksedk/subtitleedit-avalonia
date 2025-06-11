using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;
public class RemoveItem 
{
    public bool Apply { get; set; }
    public int Index { get; set; }
    public int IndexDisplay { get; set; }
    public string Before { get; set; }
    public string After { get; set; }
    public Paragraph Paragraph { get; set; }

    public RemoveItem(bool apply, int index, string before, string after, Paragraph paragraph)
    {
        Apply = apply;
        Index = index;
        IndexDisplay = index + 1;
        Before = before;
        After = after;
        Paragraph = paragraph;
    }
}
