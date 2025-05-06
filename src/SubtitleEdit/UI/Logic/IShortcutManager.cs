using System;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

public interface IShortcutManager
{
    void OnKeyPressed(object? sender, KeyEventArgs e);
    void OnKeyReleased(object? sender, KeyEventArgs e);
    void RegisterShortcut(ShortCut shortcut);
 //   void RegisterShortcut(SeShortCut shortcut, Action action, string control);
    IRelayCommand? CheckShortcuts(string? control);

    void ClearShortcuts();
    bool IsControlDown { get; }
    bool IsAltDown { get; }
    bool IsShiftDown { get; }
}