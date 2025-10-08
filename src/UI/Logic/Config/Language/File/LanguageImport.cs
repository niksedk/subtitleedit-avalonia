namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageImport
{
    public string ImportSubtitleWithManuallyChosenEncoding { get; set; }
    public object ImportTimeCodes { get; internal set; }

    public LanguageImport()
    {
        ImportSubtitleWithManuallyChosenEncoding = "_Import subtitle with manually chosen encoding...";
        ImportTimeCodes = "Import time codes...";
    }
}