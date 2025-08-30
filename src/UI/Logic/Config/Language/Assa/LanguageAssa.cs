namespace Nikse.SubtitleEdit.Logic.Config.Language.Assa;

public class LanguageAssa
{
    public string StylesTitle { get; set; }
    public string StylesInFile { get; set; }
    public string StylesSaved { get; set; }
    public string StylesTitleX { get; set; }
    public string PropertiesTitleX { get; set; }
    public string AttachmentsTitleX { get; set; }
    public string SmartWrappingTopWide { get; set; }
    public string EndOfLineWrapping { get; set; }
    public string NoWrapping { get; set; }
    public string SmartWrappingBottomWide { get; set; }

    public LanguageAssa()
    {
        StylesTitle = "Advanced Sub Station Alpha styles";
        StylesInFile = "Styles in file";
        StylesSaved = "Styles saved";
        StylesTitleX = "Styles - {0}";
        PropertiesTitleX = "Properties - {0}";
        AttachmentsTitleX = "Attachments - {0}";
        SmartWrappingTopWide = "0: Smart wrapping (top wide)";
        EndOfLineWrapping = "1: End-of-line word wrapping, only \\N breaks";
        NoWrapping = "2: No wrapping, both \\N an \\n breaks";
        SmartWrappingBottomWide = "3: Smart wrapping (bottom wide)";
    }
}