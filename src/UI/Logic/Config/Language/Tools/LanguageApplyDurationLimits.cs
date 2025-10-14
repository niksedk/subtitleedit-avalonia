namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageApplyDurationLimits
{
    public string Title { get; set; }
    public string FixMinDurationMs { get; set; }
    public string DoNotGoPastShotChange { get; set; }
    public string FixMaxDurationMs { get; set; }

    public LanguageApplyDurationLimits()
    {
        Title = "Apply duration limits";
        FixMinDurationMs = "Fix minimum duration (ms):";
        DoNotGoPastShotChange = "Do not go past shot change";
        FixMaxDurationMs = "Fix maximum duration (ms):";
    }
}