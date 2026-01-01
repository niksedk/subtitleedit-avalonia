namespace Nikse.SubtitleEdit.Logic.Config;

public class SeFormats
{
    public string RosettaLanguage { get; set; }
    public bool RosettaLanguageAutoDetect { get; set; }    
    public string RosettaFontSize { get; set; }
    public string RosettaLineHeight { get; set; }

    public string TmpegEncXmlFontName { get; set; }
    public decimal TmpegEncXmlFontHeight { get; set; }
    public bool TmpegEncXmlFontBold { get; set; }


    public SeFormats()
    {
        RosettaLanguage = "en";
        RosettaLanguageAutoDetect = true;
        RosettaFontSize = "100%";
        RosettaLineHeight = "125%";

        TmpegEncXmlFontName = "Tahoma";
        TmpegEncXmlFontHeight = 0.069m;
        TmpegEncXmlFontBold = false;
    }
}