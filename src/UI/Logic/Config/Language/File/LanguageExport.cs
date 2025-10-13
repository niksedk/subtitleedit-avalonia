namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageExport
{
    public string ExportImagesProfiles { get; set; }
    public string LeftRightMargin { get;  set; }
    public string TopBottomMargin { get; set; }
    public string TitleExportBluRaySup { get;    set; }
    public string LineSpacingPercent { get;  set; }
    public string PaddingLeftRight { get; set; }
    public string PaddingTopBottom { get; set; }
    public string PreviewTitle { get; set; }
    public string TitleExportVobSub { get; set; }
    public string CustomTextFormatsDotDotDot { get; set; }
    public string PlainTextDotDotDot { get; set; }
    public string CustomTextFormats { get; set; }
    public string TitleExportCustomFormat { get; set; }
    public string EditCustomFormat { get; set; }
    public string NewCustomFormat { get; set; }
    public string DeleteSelectedCustomTextFormatX { get; set; }
    public string TimeCodeFormat { get; set; }
    public string NewLineFormat { get; set; }
    public string PleaseEnterNameForTheCustomFormat { get; set; }
    public string TitleExportPlainText { get; set; }

    public LanguageExport()
    {
        ExportImagesProfiles = "Export images profiles";
        LeftRightMargin = "Left/right margin";
        TopBottomMargin = "Top/bottom margin";
        TitleExportBluRaySup = "Export Blu-ray (sup)";
        LineSpacingPercent = "Line spacing %";
        PaddingLeftRight = "Padding left/right";
        PaddingTopBottom = "Padding top/bottom";
        PreviewTitle = "Preview - current size: {0}x{1}, target size: {2}x{3}, zoom: {4}%";
        TitleExportVobSub = "VobSub (sub/idx)";
        CustomTextFormatsDotDotDot = "_Custom text formats...";
        PlainTextDotDotDot = "_Plain text...";
        CustomTextFormats = "Custom text formats";
        TitleExportCustomFormat = "Export custom text format";
        EditCustomFormat = "Edit custom text format";
        NewCustomFormat = "New custom text format";
        DeleteSelectedCustomTextFormatX = "Delete selected custom text format \"{0}\"?";
        TimeCodeFormat = "Time code format";
        NewLineFormat = "New line format";
        PleaseEnterNameForTheCustomFormat = "Please enter name for the custom format";
        TitleExportPlainText = "Export plain text";
    }
}