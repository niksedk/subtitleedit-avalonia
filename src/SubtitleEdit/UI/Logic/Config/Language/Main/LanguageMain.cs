namespace Nikse.SubtitleEdit.Logic.Config.Language.File;

public class LanguageMain
{

    public LanguageMainMenu Menu { get; set; } = new();

    public string CharactersPerSecond { get; set; }
    public string SingleLineLength { get; set; }
    public string TotalCharacters { get; set; }

    public LanguageMain()
    {
        CharactersPerSecond = "Chars/second: {0}";
        SingleLineLength = "Line length: {0}";
        TotalCharacters = "Total chars: {0}";
    }
}