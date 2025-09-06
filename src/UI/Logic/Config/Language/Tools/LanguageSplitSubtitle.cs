namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageSplitSubtitle
{
    public string Title { get; set; }
    public string Split { get; internal set; }
    public string NumberOfEqualParts { get; internal set; }

    public LanguageSplitSubtitle()
    {
        Title = "Split subtitle";
        Split = "_Split";
        NumberOfEqualParts = "Number of equal parts";
    }
}