namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageImport
{
    public string ImportTimeCodes { get; set; }
    public string ImagesDotDotDot { get; set; }
    public string TimeCodesDotDotDot { get; set; }
    public string SubtitleWithManuallyChosenEncodingDotDotDot { get; set; }
    public string TitleImportImages { get; set; }

    public LanguageImport()
    {
        ImportTimeCodes = "Import time codes...";
        ImagesDotDotDot = "Images...";
        TitleImportImages = "Import images";
        TimeCodesDotDotDot = "Time codes...";
        SubtitleWithManuallyChosenEncodingDotDotDot = "_Subtitle with manually chosen encoding...";
    }
}