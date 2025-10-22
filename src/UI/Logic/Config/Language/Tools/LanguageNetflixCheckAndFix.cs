namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageNetflixCheckAndFix
{
    public string Title { get; set; }
    public string GenerateReport { get; set; }

    public LanguageNetflixCheckAndFix()
    {
        Title = "Netflix check and fix errors";
        GenerateReport = "Generate report";
    }
}