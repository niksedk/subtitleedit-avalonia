namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageChangeCasing
{
    public string Title { get; set; }
    public string FixNames { get; set; }
    public string ExtraNames { get; set; }
    public string? EnterExtraNamesHint { get; set; }

    public LanguageChangeCasing()
    {
        Title = "Change casing";
        FixNames = "Fix names";
        ExtraNames = "Extra names";
        EnterExtraNamesHint = "Enter extra names to fix, separated by comma";
    }
}