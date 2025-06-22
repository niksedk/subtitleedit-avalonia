namespace Nikse.SubtitleEdit.Logic.Config.Language.Edit;

public class LanguageMultipleReplace
{

    public string Title { get; set; }
    public string EditRule { get; internal set; }
    public string NewRule { get; internal set; }
    public string EditCategory { get; set; }
    public string NewCategory { get; set; }
    public string CategoryName { get; set; }

    public LanguageMultipleReplace()
    {
        Title = "Multiple replace";
        EditRule = "Edit rule";
        EditCategory = "Edit category";
        NewRule = "New rule";
        NewCategory = "New category";
        CategoryName = "Category name";
    }
}