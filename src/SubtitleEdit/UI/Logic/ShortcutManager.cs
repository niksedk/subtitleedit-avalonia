using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

public class ShortcutManager : IShortcutManager
{
    private readonly HashSet<Key> _activeKeys = [];
    private readonly List<ShortCut> _shortcuts = [];
    public bool IsControlDown => _activeKeys.Contains(Key.LeftCtrl) || _activeKeys.Contains(Key.RightCtrl);
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

    public void RegisterShortcut(SeShortCut shortcut, Action action)
    {
        _shortcuts.Add(new ShortCut(shortcut.Keys, null, action));
    }

    public void RegisterShortcut(SeShortCut shortcut, Action action, string control)
    {
        _shortcuts.Add(new ShortCut(shortcut.Keys, control, action));
    }

    public Action? CheckShortcuts(string? control)
    {
        var input = new ShortCut(_activeKeys.Select(p => p.ToString()).ToList(), control, () => { });
        var inputWithNormalizedModifiers = NormalizeModifiers(input);

        if (_activeKeys.Count < 2)
        {
            return null; //TODO: remove
        }

        foreach (var shortcut in _shortcuts)
        {
            if (input.HashCode == shortcut.HashCode || inputWithNormalizedModifiers.HashCode == shortcut.HashCode)
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

    private static ShortCut NormalizeModifiers(ShortCut input)
    {
        var keys = new List<string>();
        foreach (var key in input.Keys)
        {
            if (key is "LeftCtrl" or "RightCtrl")
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

        return new ShortCut(keys, input.Control, input.Action);
    }
}
