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
    public string FontsAndGraphics { get; set; }
    public string WrapStyle { get; set; }
    public string BorderAndShadowScaling { get; set; }
    public string OriginalScript { get; set; }
    public string Graphics { get; set; }
    public string CopyToStorageStyles { get; set; }
    public string CopyToFileStyles { get; set; }
    public string SetStyleAsDefault { get; set; }
    public string TakeUsagesFromDotDotDot { get; set; }
    public string NoAttachmentsFound { get; set; }
    public string DeleteStyleQuestion { get; set; }
    public string DeleteStylesQuestion { get; set; }
    public string OpenStyleImportFile { get; set; }
    public string Primary { get; set; }
    public string Secondary { get; set; }
    public string AssaDraw { get; set; }

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
        FontsAndGraphics = "Fonts and graphics";
        WrapStyle = "Wrap style";
        BorderAndShadowScaling = "Border and shadow scaling";
        OriginalScript = "Original script";
        Graphics = "Graphics";
        CopyToStorageStyles = "Copy to storage styles";
        CopyToFileStyles = "Copy to file styles";
        SetStyleAsDefault = "Set style as default";
        TakeUsagesFromDotDotDot = "Take usages from...";
        NoAttachmentsFound = "No attachments found in selected ASSA file.";
        DeleteStyleQuestion = "Delete style?";
        DeleteStylesQuestion = "Delete styles?";
        OpenStyleImportFile = "Open subtitle file to import styles from";
        Primary = "Primary";
        Secondary = "Secondary";
        AssaDraw = "ASSA Draw";
    }
}