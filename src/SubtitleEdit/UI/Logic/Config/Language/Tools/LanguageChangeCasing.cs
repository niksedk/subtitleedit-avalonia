namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageChangeCasing
{
    public string Title { get; set; }
    public string FixNames { get; internal set; }

    public LanguageChangeCasing()
    {
        Title = "Change casing";
        FixNames = "Fix names";
    }
}