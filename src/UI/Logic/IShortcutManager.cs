using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic;

public interface IShortcutManager
{
    void OnKeyPressed(object? sender, KeyEventArgs e);
    void OnKeyReleased(object? sender, KeyEventArgs e);
    void ClearKeys();
    void RegisterShortcut(ShortCut shortcut);
    IRelayCommand? CheckShortcuts(string activeControl);
    void ClearShortcuts();
    HashSet<Key> GetActiveKeys();
    bool IsControlPressed();
}