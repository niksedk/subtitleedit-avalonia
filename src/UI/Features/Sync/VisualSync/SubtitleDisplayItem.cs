using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Sync.VisualSync;

public class SubtitleDisplayItem
{
    public SubtitleLineViewModel Subtitle { get; set; }
    public string Text { get; set; }

    public SubtitleDisplayItem(SubtitleLineViewModel subtitle)
    {
        Subtitle = subtitle;
        var startTime = new TimeCode(subtitle.StartTime);
        Text = $"{startTime.ToDisplayString()}  {subtitle.Text}";
    }

    public override string ToString()
    {
        return Text;
    }
}