namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageChangeCasing
{
    public string Title { get; set; }
    public string FixNames { get; internal set; }
    public string ExtraNames { get; internal set; }
    public string? EnterExtraNamesHint { get; internal set; }

    public LanguageChangeCasing()
    {
        Title = "Change casing";
        FixNames = "Fix names";
        ExtraNames = "Extra names";
        EnterExtraNamesHint = "Enter extra names to fix, separated by comma";
    }
}