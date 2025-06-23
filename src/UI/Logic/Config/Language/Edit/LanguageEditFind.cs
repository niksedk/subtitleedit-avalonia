namespace Nikse.SubtitleEdit.Logic.Config.Language.Edit;

public class LanguageEditFind
{
    public string SearchTextWatermark { get; set; }
    public string WholeWord { get; set; }
    public string CaseSensitive { get; set; }
    public string CaseInsensitive { get; set; }
    public string FindPrevious { get; set; }
    public string FindNext { get; internal set; }

    public LanguageEditFind()
    {
        SearchTextWatermark = "Search text...";
        WholeWord = "Whole word";
        CaseSensitive = "Case sensitive";
        CaseInsensitive = "Case insensitive";
        FindPrevious = "Find previous";
        FindNext = "Find next";
    }
}