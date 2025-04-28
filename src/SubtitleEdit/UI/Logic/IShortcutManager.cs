using System;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

public interface IShortcutManager
{
    void OnKeyPressed(object? sender, KeyEventArgs e);
    void OnKeyReleased(object? sender, KeyEventArgs e);
    void RegisterShortcut(SeShortCut shortcut, Action action);
    void RegisterShortcut(SeShortCut shortcut, Action action, string control);
    Action? CheckShortcuts(string? control);
    void ClearShortcuts();
    bool IsControlDown { get; }
    bool IsAltDown { get; }
    bool IsShiftDown { get; }
}