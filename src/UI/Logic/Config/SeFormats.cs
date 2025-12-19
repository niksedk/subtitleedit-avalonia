namespace Nikse.SubtitleEdit.Logic.Config;

public class SeFormats
{
    public string RosettaLanguage { get; set; }
    public bool RosettaLanguageAutoDetect { get; set; }
    
    public string RosettaFontSize { get; set; }
    public string RosettaLineHeight { get; set; }

    public SeFormats()
    {
        RosettaLanguage = "en";
        RosettaLanguageAutoDetect = true;
        RosettaFontSize = "100%";
        RosettaLineHeight = "125%";
    }
}