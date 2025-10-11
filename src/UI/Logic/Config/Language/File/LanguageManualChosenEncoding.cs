namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageManualChosenEncoding
{
    public string Title { get; set; }
    public string SearchEncodings { get; set; }
    public object CodePage { get; set; }

    public LanguageManualChosenEncoding()
    {
        Title = "Import subtitle with manually chosen encoding";
        SearchEncodings = "Search encodings";
        CodePage = "Code page";
    }
}
