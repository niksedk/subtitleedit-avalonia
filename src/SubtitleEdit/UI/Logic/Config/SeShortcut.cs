using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeShortCut
{
    public string ActionName { get; set; }

    public List<string> Keys { get; set; }

    public SeShortCut()
    {
        Keys = new List<string>();
    }

    public SeShortCut(string action, List<string> keys)
    {
        ActionName = action;
        Keys = keys;
    }
}
