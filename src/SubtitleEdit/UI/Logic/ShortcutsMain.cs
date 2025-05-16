using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

public static class ShortcutsMain
{
    public static List<ShortCut> GetUsedShortcuts(MainViewModel vm)
    {
        var shortcuts = new List<ShortCut>();
        var keys = Se.Settings.Shortcuts.ToDictionary(p => p.ActionName, p => p);
        foreach (var shortcut in GetAllAvailableShortcuts(vm))
        {
            if (keys.TryGetValue(shortcut.Name, out var match))
            {
                shortcuts.Add(new ShortCut(shortcut, match));
            }
        }

        return shortcuts;
    }

    public static List<ShortCut> GetAllShortcuts(MainViewModel vm)
    {
        var shortcuts = new List<ShortCut>();
        var keys = Se.Settings.Shortcuts.ToDictionary(p => p.ActionName, p => p);
        foreach (var shortcut in GetAllAvailableShortcuts(vm))
        {
            if (keys.TryGetValue(shortcut.Name, out var match))
            {
                shortcuts.Add(new ShortCut(shortcut, match));
            }
            else
            {
                shortcuts.Add(new ShortCut(shortcut));
            }
        }

        return shortcuts;
    }

    private static void AddShortcut<T>(IList<AvailableShortcut> list, T command, string name, ShortcutCategory category)
    where T : IRelayCommand
    {
        list.Add(new AvailableShortcut(command, name, category));
    }

    private static List<AvailableShortcut> GetAllAvailableShortcuts(MainViewModel vm)
    {
        var shortcuts = new List<AvailableShortcut>();

        AddShortcut(shortcuts, vm.CommandExitCommand, nameof(vm.CommandExitCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowGoToLineCommand, nameof(vm.ShowGoToLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToPreviousLineCommand, nameof(vm.GoToPreviousLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToNextLineCommand, nameof(vm.GoToNextLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandShowAutoTranslateCommand, nameof(vm.CommandShowAutoTranslateCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandShowSettingsCommand, nameof(vm.CommandShowSettingsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandShowLayoutCommand, nameof(vm.CommandShowLayoutCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.ToggleLinesItalicCommand, nameof(vm.ToggleLinesItalicCommand), ShortcutCategory.SubtitleGridAndTextBox);

        AddShortcut(shortcuts, vm.SelectAllLinesCommand, nameof(vm.SelectAllLinesCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.InverseSelectionCommand, nameof(vm.InverseSelectionCommand), ShortcutCategory.SubtitleGrid);

        return shortcuts;
    }

    public class AvailableShortcut
    {
        public string Name { get; set; }
        public ShortcutCategory Category { get; set; }

        public AvailableShortcut(IRelayCommand relayCommand, string shortcutName)
        {
            Name = shortcutName;
            RelayCommand = relayCommand;
            Category = ShortcutCategory.General;
        }
        public AvailableShortcut(IRelayCommand relayCommand, string shortcutName, ShortcutCategory category)
        {
            Name = shortcutName;
            RelayCommand = relayCommand;
            Category = category;
        }

        public IRelayCommand RelayCommand { get; set; }
    }
}