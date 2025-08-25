using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public class ShortcutManager : IShortcutManager
{
    private readonly HashSet<Key> _activeKeys = [];
    private readonly List<ShortCut> _shortcuts = [];

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

    public IRelayCommand? CheckShortcuts(string activeControl)
    {
        var keys = _activeKeys.Select(p => p.ToString()).ToList();
        var hashCode = ShortCut.CalculateHash(keys, activeControl);
        var inputWithNormalizedModifiers = CalculateNormalizedHash(keys, activeControl);

        foreach (var shortcut in _shortcuts)
        {
            if (shortcut.Keys.Count > 0)
            {
                if (hashCode == shortcut.HashCode ||
                    inputWithNormalizedModifiers == shortcut.HashCode ||
                    inputWithNormalizedModifiers == shortcut.NormalizedHashCode)
                {
                    return shortcut.Action;
                }
            }
        }

        return null;
    }

    public void ClearShortcuts()
    {
        _shortcuts.Clear();
    }

    public HashSet<Key> GetActiveKeys()
    {
        return [.. _activeKeys];
    }

    public static string CalculateNormalizedHash(List<string> inputKeys, string? control)
    {
        var keys = new List<string>();
        foreach (var key in inputKeys)
        {
            if (key is "LeftCtrl" or "RightCtrl" or "LWin" or "Ctrl")
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
            else if (key is "LWin" or "RWin")
            {
                keys.Add("Win");
            }
            else
            {
                keys.Add(key);
            }
        }

        return ShortCut.CalculateHash(keys, control);
    }
}
