namespace Nikse.SubtitleEdit.Logic.Config.Language.Edit;

public class LanguageEdit
{
    public LanguageMultipleReplace MultipleReplace { get; set; } = new();
    public LanguageEditFind Find { get; set; } = new();
    public string ShowHistory { get; set; }
    public string RestoreSelected { get; set; }

    public LanguageEdit()
    {
        ShowHistory = "Show history";
        RestoreSelected = "Restore selected";
    }
}