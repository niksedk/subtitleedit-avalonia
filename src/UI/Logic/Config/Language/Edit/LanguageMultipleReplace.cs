namespace Nikse.SubtitleEdit.Logic.Config.Language.Edit;

public class LanguageMultipleReplace
{
    public string Title { get; set; }
    public string EditRule { get; internal set; }
    public string NewRule { get; internal set; }

    public LanguageMultipleReplace()
    {
        Title = "Multiple replace";
        EditRule = "Edit rule";
        NewRule = "New rule";
    }
}