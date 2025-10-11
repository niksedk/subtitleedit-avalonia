using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

public class PreviewItem
{
    public int Number { get; set; }
    public bool Apply { get; set; }
    public string Text { get; set; }
    public SubtitleLineViewModel Subtitle { get; set; }

    public PreviewItem(int number, bool apply, string text, SubtitleLineViewModel subtitle)
    {
        Number = number;
        Apply = apply;
        Text = text;
        Subtitle = subtitle;
    }
}
