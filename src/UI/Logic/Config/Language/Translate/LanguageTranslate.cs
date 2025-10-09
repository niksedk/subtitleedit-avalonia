namespace Nikse.SubtitleEdit.Logic.Config.Language.Translate;

public class LanguageTranslate
{
    public string Title { get; set; }
    public string TranslateViaCopyPaste { get; set; }

    public LanguageTranslate()
    {
        Title = "Auto-translate";
        TranslateViaCopyPaste = "Translate via copy/paste";
    }
}