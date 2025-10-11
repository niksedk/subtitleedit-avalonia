namespace Nikse.SubtitleEdit.Logic.Config;

public class SeEdit
{
    public SeEditMultipleReplace MultipleReplace { get; set; } = new SeEditMultipleReplace();
    public SeEditFind Find { get; set; } = new SeEditFind();
    public string ModifySelectionMode { get; internal set; }

    public SeEdit()
    {
        ModifySelectionMode = "New";
    }
}