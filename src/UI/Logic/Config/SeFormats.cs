namespace Nikse.SubtitleEdit.Logic.Config;

public class SeFormats
{
    public string Language { get; set; }
    public bool LanguageAutoDetect { get; set; }
    
    public string FontSize { get; set; }
    public string LineHEight { get; set; }

    public SeFormats()
    {
        Language = "en";
        LanguageAutoDetect = true;
        FontSize = string.Empty;
        LineHEight = string.Empty;
    }
}