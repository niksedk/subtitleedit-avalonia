using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeEdit
{
    public bool FindWholeWords { get; set; } 
    public string FindSearchType { get; set; }

    public SeEdit()
    {
        FindWholeWords = false;
        FindSearchType = "Normal"; 
    }
}