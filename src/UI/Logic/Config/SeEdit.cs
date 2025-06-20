namespace Nikse.SubtitleEdit.Logic.Config;

public class SeEdit
{
    public bool FindWholeWords { get; set; } 
    public string FindSearchType { get; set; }
    public SeEditMultipleReplace MultipleReplace { get; set; } = new SeEditMultipleReplace();

    public SeEdit()
    {
        FindWholeWords = false;
        FindSearchType = "Normal"; 
    }
}