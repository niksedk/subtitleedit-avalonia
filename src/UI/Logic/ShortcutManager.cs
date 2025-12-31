using System;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

public class ShortcutManager : IShortcutManager
{
    private readonly HashSet<Key> _activeKeys = [];
    private List<ShortCut> _shortcuts = [];
    private bool _sorted = false;
    private bool _isControlDown = false;

    public static string GetKeyDisplayName(string key)
    {
        if (OperatingSystem.IsMacOS())
        {
            return key switch
            {
                "Ctrl" or "Control" => Se.Language.Options.Shortcuts.ControlMac,
                "Alt" => Se.Language.Options.Shortcuts.AltMac,
                "Shift" => Se.Language.Options.Shortcuts.ShiftMac,
                "Win" or "Cmd" => Se.Language.Options.Shortcuts.WinMac,
                _ => key
            };
        }

        return key switch
        {
            "Ctrl" or "Control" => Se.Language.Options.Shortcuts.Control,
            "Alt" => Se.Language.Options.Shortcuts.Alt,
            "Shift" => Se.Language.Options.Shortcuts.Shift,
            "Win" => Se.Language.Options.Shortcuts.Win,
            _ => key
        };
    }

    public void OnKeyPressed(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl &&
            e.Key != Key.LeftShift && e.Key != Key.RightShift &&
            e.Key != Key.LeftAlt && e.Key != Key.RightAlt)
        {
            _activeKeys.Add(e.Key);  // do not add modifier keys
        }
        else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isControlDown = true;
        }
    }

    public void OnKeyReleased(object? sender, KeyEventArgs e)
    {
        _activeKeys.Remove(e.Key);
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isControlDown = false;
        }
    }

    public void ClearKeys()
    {
        _activeKeys.Clear();
        _isControlDown = false;
    }

    public void RegisterShortcut(ShortCut shortcut)
    {
        _shortcuts.Add(shortcut);
    }

    public IRelayCommand? CheckShortcuts(KeyEventArgs keyEventArgs, string activeControl)
    {
        if (!_sorted)
        {
            _sorted = true;
            _shortcuts = _shortcuts.OrderByDescending(p => p.Keys.Count).ToList();
        }

        var activeKeysIncludeingModefiers = new HashSet<Key>(_activeKeys);
        if (keyEventArgs.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            activeKeysIncludeingModefiers.Add(Key.LeftCtrl);
        }
        if (keyEventArgs.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            activeKeysIncludeingModefiers.Add(Key.LeftAlt);
        }
        if (keyEventArgs.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            activeKeysIncludeingModefiers.Add(Key.LeftShift);
        }

        var keys = activeKeysIncludeingModefiers.Select(p => p.ToString()).ToList();
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
        _isControlDown = false;
    }

    public HashSet<Key> GetActiveKeys()
    {
        return [.. _activeKeys];
    }

    public bool IsControlPressed()
    {
        return _isControlDown;
    }

    public static string CalculateNormalizedHash(List<string> inputKeys, string? control)
    {
        var keys = new List<string>();
        foreach (var key in inputKeys)
        {
            if (key is "LeftCtrl" or "RightCtrl" or "Ctrl")
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
