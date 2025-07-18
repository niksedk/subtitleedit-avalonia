namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageExport
{
    public string LeftRightMargin { get;  set; }
    public string TopBottomMargin { get; set; }
    public string TitleExportBluRaySup { get;    set; }

    public LanguageExport()
    {
        LeftRightMargin = "Lefr/right margin";
        TopBottomMargin = "Top/bottom margin";
        TitleExportBluRaySup = "Export Blu-ray sup";
    }
}