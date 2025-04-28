using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public class ShortCut
{
    public List<string> Keys { get; set; }
    public string? Control { get; set; }
    public Action Action { get; set; }
    public int HashCode { get; set; }

    public ShortCut(List<string> keys, string? control, Action action)
    {
        Keys = keys;
        Control = control;
        Action = action;
        HashCode = keys.Aggregate(0, (hash, keyCode) => hash ^ keyCode.GetHashCode());
        if (control != null)
        {
            HashCode ^= control.GetHashCode();
        }
    }
}