using System;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

public interface IShortcutManager
{
    void OnKeyPressed(object? sender, KeyEventArgs e);
    void OnKeyReleased(object? sender, KeyEventArgs e);
    void ClearKeys();
    void RegisterShortcut(ShortCut shortcut);
    IRelayCommand? CheckShortcuts(string? control);

    void ClearShortcuts();
}