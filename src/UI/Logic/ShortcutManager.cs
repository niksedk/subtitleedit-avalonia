using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public class ShortcutManager : IShortcutManager
{
    private readonly HashSet<Key> _activeKeys = [];
    private readonly List<ShortCut> _shortcuts = [];
    private readonly Dictionary<string, ShortCut> _lookupTable = new();
    private bool _isDirty = true;
    private bool _isControlPressed = false;

    public static string GetKeyDisplayName(string key)
    {
        bool isMac = OperatingSystem.IsMacOS();
        var shortcuts = Se.Language.Options.Shortcuts;

        return key switch
        {
            "Ctrl" or "Control" => isMac ? shortcuts.ControlMac : shortcuts.Control,
            "Alt" => isMac ? shortcuts.AltMac : shortcuts.Alt,
            "Shift" => isMac ? shortcuts.ShiftMac : shortcuts.Shift,
            "Win" or "Cmd" => isMac ? shortcuts.WinMac : shortcuts.Win,
            _ => key
        };
    }

    public void OnKeyPressed(object? sender, KeyEventArgs e)
    {
        // Avoid adding modifier keys to the active keys set to prevent redundancy 
        // with KeyEventArgs.KeyModifiers
        if (e.Key is not (Key.LeftCtrl or Key.RightCtrl or
                         Key.LeftShift or Key.RightShift or
                         Key.LeftAlt or Key.RightAlt or
                         Key.LWin or Key.RWin))
        {
            _activeKeys.Add(e.Key);
        }
        else if (e.Key is Key.LeftCtrl or Key.RightCtrl)
        {
            _isControlPressed = true;
        }
    }

    public void OnKeyReleased(object? sender, KeyEventArgs e)
    {
        _activeKeys.Remove(e.Key);
        if (e.Key is Key.LeftCtrl or Key.RightCtrl)
        {
            _isControlPressed = false;
        }
    }

    public void ClearKeys()
    {
        _activeKeys.Clear();
        _isControlPressed = false;
    }

    public void RegisterShortcut(ShortCut shortcut)
    {
        _shortcuts.Add(shortcut);
        _isDirty = true;
    }

    public void ClearShortcuts()
    {
        _shortcuts.Clear();
        _lookupTable.Clear();
        _isDirty = true;
    }

    private void RebuildLookupTable()
    {
        _lookupTable.Clear();
        // Sort by key count descending so that specific shortcuts (Ctrl+Shift+S) 
        // take precedence over general ones (Ctrl+S) if they share hashes
        var sorted = _shortcuts.Where(s => s.Keys.Count > 0)
                               .OrderByDescending(s => s.Keys.Count);

        foreach (var sc in sorted)
        {
            // Map the direct hash
            _lookupTable.TryAdd(sc.HashCode, sc);

            // Map the normalized hash (e.g., "LeftCtrl" -> "Control")
            if (!string.IsNullOrEmpty(sc.NormalizedHashCode))
            {
                _lookupTable.TryAdd(sc.NormalizedHashCode, sc);
            }
        }
        _isDirty = false;
    }

    public IRelayCommand? CheckShortcuts(KeyEventArgs keyEventArgs, string activeControl)
    {
        if (_isDirty)
        {
            RebuildLookupTable();
        }

        // Build the current state key list
        var currentInputKeys = new List<string>(_activeKeys.Count + 4);

        foreach (var key in _activeKeys)
        {
            currentInputKeys.Add(key.ToString());
        }

        // Add normalized modifiers based on the event state
        if (keyEventArgs.KeyModifiers.HasFlag(KeyModifiers.Control)) currentInputKeys.Add("Control");
        if (keyEventArgs.KeyModifiers.HasFlag(KeyModifiers.Alt)) currentInputKeys.Add("Alt");
        if (keyEventArgs.KeyModifiers.HasFlag(KeyModifiers.Shift)) currentInputKeys.Add("Shift");
        if (keyEventArgs.KeyModifiers.HasFlag(KeyModifiers.Meta)) currentInputKeys.Add("Win");

        // 1. Check primary hash
        var inputHash = ShortCut.CalculateHash(currentInputKeys, activeControl);
        if (_lookupTable.TryGetValue(inputHash, out var shortcut))
        {
            return shortcut.Action;
        }

        // 2. Fallback to normalized hash check
        var normalizedInputHash = CalculateNormalizedHash(currentInputKeys, activeControl);
        if (_lookupTable.TryGetValue(normalizedInputHash, out shortcut))
        {
            return shortcut.Action;
        }

        return null;
    }

    public static string CalculateNormalizedHash(List<string> inputKeys, string? control)
    {
        var keys = new List<string>(inputKeys.Count);
        foreach (var key in inputKeys)
        {
            keys.Add(key switch
            {
                "LeftCtrl" or "RightCtrl" or "Ctrl" => "Control",
                "LeftShift" or "RightShift" => "Shift",
                "LeftAlt" or "RightAlt" => "Alt",
                "LWin" or "RWin" => "Win",
                _ => key
            });
        }

        return ShortCut.CalculateHash(keys, control);
    }

    public HashSet<Key> GetActiveKeys() => [.. _activeKeys];

    public bool IsControlPressed() => _isControlPressed;
}