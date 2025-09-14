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
    public string TitleExportCustomFormat { get; set; }

    public LanguageExport()
    {
        ExportImagesProfiles = "Export images profiles";
        LeftRightMargin = "Lefr/right margin";
        TopBottomMargin = "Top/bottom margin";
        TitleExportBluRaySup = "Export Blu-ray (sup)";
        LineSpacingPercent = "Line spacing %";
        PaddingLeftRight = "Padding left/right";
        PaddingTopBottom = "Padding top/bottom";
        PreviewTitle = "Preview - current size: {0}x{1}, target size: {2}x{3}, zoom: {4}%";
        TitleExportVobSub = "VobSub (sub/idx)";
        CustomTextFormatsDotDotDot = "_Custom text formats...";
        TitleExportCustomFormat = "Export custom text format";
    }
}