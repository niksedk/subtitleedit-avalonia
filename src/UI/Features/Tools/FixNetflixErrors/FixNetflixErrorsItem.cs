using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Tools.FixNetflixErrors;

public class FixNetflixErrorsItem
{
    public bool Apply { get; set; }
    public int Index { get; set; }
    public int IndexDisplay { get; set; }
    public string Before { get; set; }
    public string After { get; set; }
    public Paragraph Paragraph { get; set; }

    public FixNetflixErrorsItem(bool apply, int index, string before, string after, Paragraph paragraph)
    {
        Apply = apply;
        Index = index;
        IndexDisplay = index + 1;
        Before = before;
        After = after;
        Paragraph = paragraph;
    }
}
