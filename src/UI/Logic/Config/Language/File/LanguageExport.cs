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

    public LanguageExport()
    {
        ExportImagesProfiles = "Export images profiles";
        LeftRightMargin = "Lefr/right margin";
        TopBottomMargin = "Top/bottom margin";
        TitleExportBluRaySup = "Export Blu-ray sup";
        LineSpacingPercent = "Line spacing %";
        PaddingLeftRight = "Padding left/right";
        PaddingTopBottom = "Padding top/bottom";
    }
}