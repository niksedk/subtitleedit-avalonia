using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

public class ShortCut
{
    public List<string> Keys { get; set; }
    public ShortcutCategory Category { get; set; }
    public string? Control { get; set; }
    public string Name { get; set; }
    public IRelayCommand Action { get; set; }
    public int HashCode { get; set; }

    public ShortCut(string name, List<string> keys, ShortcutCategory category, IRelayCommand action)
    {
        Name = name;
        Keys = keys;
        Category = category;
        Control = category.ToString();
        Action = action;
        HashCode = CalculateHash(keys, Control);
    }

    public static int CalculateHash(List<string> keys, string control)
    {
        var hashCode = keys.Aggregate(0, (hash, keyCode) => hash ^ keyCode.GetHashCode());
        hashCode ^= control.GetHashCode();
        
        return hashCode;
    }

    public ShortCut(ShortcutsMain.AvailableShortcut shortcut, SeShortCut keys)
    {
        Keys = keys.Keys;
        Action = shortcut.RelayCommand;
        Category = shortcut.Category;
        Name = shortcut.Name;
        Control = ShortcutCategory.General.ToString(); 
        HashCode = CalculateHash(Keys, Control);
    }

    public ShortCut(ShortcutsMain.AvailableShortcut shortcut)
    {
        Keys = new List<string>();
        Action = shortcut.RelayCommand;
        Category = shortcut.Category;
        Name = shortcut.Name;
        Control = string.Empty;
        HashCode = CalculateHash(Keys, Control);
    }
}