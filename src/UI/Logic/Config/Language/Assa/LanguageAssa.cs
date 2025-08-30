using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Assa;

public class LanguageAssa
{
    public string StylesTitle { get; set; }
    public string StylesInFile { get; set; }
    public string StylesSaved { get; set; }
    public string StylesTitleX { get; set; }
    public string PropertiesTitleX { get; set; }
    public string AttachmentsTitleX { get; set; }

    public LanguageAssa()
    {
        StylesTitle = "Advanced Sub Station Alpha styles";
        StylesInFile = "Styles in file";
        StylesSaved = "Styles saved";
        StylesTitleX = "Styles - {0}";
        PropertiesTitleX = "Properties - {0}";
        AttachmentsTitleX = "Attachments - {0}";
    }
}