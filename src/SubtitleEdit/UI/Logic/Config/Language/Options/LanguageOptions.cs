namespace Nikse.SubtitleEdit.Logic.Config.Language.File;

public class LanguageOptions
{
    public LanguageSettings Settings { get; set; } = new();
    public LanguageSettingsShortcuts Shortcuts { get; set; } = new();

    public LanguageOptions()
    {
    }
}