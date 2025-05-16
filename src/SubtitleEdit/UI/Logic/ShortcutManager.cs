using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Logic;

public class ShortcutManager : IShortcutManager
{
    private readonly HashSet<Key> _activeKeys = [];
    private readonly List<ShortCut> _shortcuts = [];
    public bool IsControlDown => _activeKeys.Contains(Key.LeftCtrl) || _activeKeys.Contains(Key.RightCtrl) || _activeKeys.Contains(Key.LWin);
    public bool IsAltDown => _activeKeys.Contains(Key.LeftAlt) || _activeKeys.Contains(Key.RightAlt);
    public bool IsShiftDown => _activeKeys.Contains(Key.LeftShift) || _activeKeys.Contains(Key.RightShift);

    public void OnKeyPressed(object? sender, KeyEventArgs e)
    {
        _activeKeys.Add(e.Key);
    }

    public void OnKeyReleased(object? sender, KeyEventArgs e)
    {
        _activeKeys.Remove(e.Key);
    }

    public void ClearKeys()
    {
        _activeKeys.Clear();
    }

    public void RegisterShortcut(ShortCut shortcut)
    {
        _shortcuts.Add(shortcut);
    }

    public IRelayCommand? CheckShortcuts(string? control)
    {
        var keys = _activeKeys.Select(p => p.ToString()).ToList();
        var hashCode = ShortCut.CalculateHash(keys, control);
        var inputWithNormalizedModifiers = CalculateNormalizedHash(keys, control);

        if (_activeKeys.Count < 2)
        {
            return null; //TODO: remove
        }

        foreach (var shortcut in _shortcuts)
        {
            if (hashCode == shortcut.HashCode || 
                inputWithNormalizedModifiers == shortcut.HashCode|| 
                inputWithNormalizedModifiers == shortcut.NormalizedHashCode)
            {
                return shortcut.Action;
            }
        }

        return null;
    }

    public void ClearShortcuts()
    {
        _shortcuts.Clear();
    }

    public static int CalculateNormalizedHash(List<string> inputKeys, string? control)
    {
        var keys = new List<string>();
        foreach (var key in inputKeys)
        {
            if (key is "LeftCtrl" or "RightCtrl" or "LWin")
            {
                keys.Add("Control");
            }
            else if (key is "LeftShift" or "RightShift")
            {
                keys.Add("Shift");
            }
            else if (key is "LeftAlt" or "RightAlt")
            {
                keys.Add("Alt");
            }
            else
            {
                keys.Add(key);
            }
        }

        return ShortCut.CalculateHash(keys, control);
    }
}
