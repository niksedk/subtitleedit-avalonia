namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageSplitBreakLongLines
{
    public string Title { get; set; }
    public string SplitLongLines { get; set; }
    public string RebalanceLongLines { get; set; }

    public LanguageSplitBreakLongLines()
    {
        Title = "Split/rebalance long lines";
        SplitLongLines = "Split long lines (to multiple lines)";
        RebalanceLongLines = "Rebalance long lines";
    }
}