using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits;

public class ApplyDurationLimitItem
{
    public bool Apply { get; set; }
    public string Name { get; set; }
    public int Number { get; set; }
    public string Before { get; set; }
    public string After { get; set; }
    public SubtitleLineViewModel SubtitleLine { get; set; }

    public ApplyDurationLimitItem(bool apply, string name, int number, string before, string after, SubtitleLineViewModel subtitleLine)
    {
        Apply = apply;
        Name = name;
        Number = number;
        Before = before;
        After = after;
        SubtitleLine = subtitleLine;
    }
}
