namespace Nikse.SubtitleEdit.Logic.Config.Language.Translate;

public class LanguageTranslate
{
    public string Title { get; set; }
    public string TranslateViaCopyPaste { get; set; }
    public string MaxBlockSize { get; set; }
    public string LineSeparator { get; set; }

    public LanguageTranslate()
    {
        Title = "Auto-translate";
        TranslateViaCopyPaste = "Auto-translate via copy/paste";
        MaxBlockSize = "Max block size";
        LineSeparator = "Line separator";
    }
}