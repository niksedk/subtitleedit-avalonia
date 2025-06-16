namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageTools
{
    public LanguageFixCommonErrors FixCommonErrors { get; set; } = new();
    public LanguageAdjustDisplayDurations AdjustDurations { get; set; } = new();
    public LanguageChangeCasing ChangeCasing { get; set; } = new();

    public LanguageTools()
    {

    }
}