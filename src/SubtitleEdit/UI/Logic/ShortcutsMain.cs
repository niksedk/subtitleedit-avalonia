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

    private static MainViewModel _mvm = null!;

    public static Dictionary<string, string> CommandTranslationLookup = new Dictionary<string, string>
    {
        { nameof(_mvm.SelectAllLinesCommand) , Se.Language.Settings.Shortcuts.ListSelectAll },
        { nameof(_mvm.InverseSelectionCommand) , Se.Language.Settings.Shortcuts.ListInverseSelection },
        { nameof(_mvm.DeleteSelectedLinesCommand) , Se.Language.Settings.Shortcuts.ListDeleteSelection },
        { nameof(_mvm.OpenDataFolderCommand) , Se.Language.Settings.Shortcuts.OpenSeDataFolder },

        { nameof(_mvm.CommandFileOpenCommand) , Se.Language.Settings.Shortcuts.FileOpen },
        { nameof(_mvm.CommandExitCommand) , Se.Language.Settings.Shortcuts.FileExit },
        { nameof(_mvm.CommandFileNewCommand) , Se.Language.Settings.Shortcuts.FileNew },
        { nameof(_mvm.CommandFileSaveCommand) , Se.Language.Settings.Shortcuts.FileSave },
        { nameof(_mvm.CommandFileSaveAsCommand) , Se.Language.Settings.Shortcuts.FileSaveAs },

        { nameof(_mvm.UndoCommand) , "Undo" },
        { nameof(_mvm.RedoCommand) , "Redo" },
        { nameof(_mvm.FindNextCommand) , Se.Language.Settings.Shortcuts.EditFind },
        { nameof(_mvm.ShowReplaceCommand) , Se.Language.Settings.Shortcuts.EditReplace },
        { nameof(_mvm.ShowMultipleReplaceCommand) , Se.Language.Settings.Shortcuts.EditMultipleReplace },

        { nameof(_mvm.ShowGoToLineCommand) , Se.Language.Settings.Shortcuts.GeneralGoToLineNumber },
        { nameof(_mvm.ToggleLinesItalicCommand) , Se.Language.Settings.Shortcuts.GeneralToggleItalic },
        { nameof(_mvm.ToggleLinesBoldCommand) , Se.Language.Settings.Shortcuts.GeneralToggleBold },

        { nameof(_mvm.CommandShowLayoutCommand) , Se.Language.Settings.Shortcuts.GeneralChooseLayout },
        { nameof(_mvm.CommandShowAutoTranslateCommand) , Se.Language.Settings.Shortcuts.AutoTranslate },
        { nameof(_mvm.CommandShowSettingsCommand) , Se.Language.Settings.Shortcuts.Settings },

        { nameof(_mvm.GoToNextLineCommand) , Se.Language.Settings.Shortcuts.GeneralGoToNextSubtitle },
        { nameof(_mvm.GoToPreviousLineCommand) , Se.Language.Settings.Shortcuts.GeneralGoToPrevSubtitle },
    };

    private static List<AvailableShortcut> GetAllAvailableShortcuts(MainViewModel vm)
    {
        var shortcuts = new List<AvailableShortcut>();

        AddShortcut(shortcuts, vm.SelectAllLinesCommand, nameof(vm.SelectAllLinesCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.InverseSelectionCommand, nameof(vm.InverseSelectionCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.DeleteSelectedLinesCommand, nameof(vm.DeleteSelectedLinesCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.OpenDataFolderCommand, nameof(vm.OpenDataFolderCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.CommandFileOpenCommand, nameof(vm.CommandFileOpenCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandExitCommand, nameof(vm.CommandExitCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileNewCommand, nameof(vm.CommandFileNewCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileSaveCommand, nameof(vm.CommandFileSaveCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileSaveAsCommand, nameof(vm.CommandFileSaveAsCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.UndoCommand, nameof(vm.UndoCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.RedoCommand, nameof(vm.RedoCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.FindNextCommand, nameof(vm.FindNextCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowReplaceCommand, nameof(vm.ShowReplaceCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowMultipleReplaceCommand, nameof(vm.ShowMultipleReplaceCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.ShowGoToLineCommand, nameof(vm.ShowGoToLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleLinesItalicCommand, nameof(vm.ToggleLinesItalicCommand), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.ToggleLinesBoldCommand, nameof(vm.ToggleLinesBoldCommand), ShortcutCategory.SubtitleGridAndTextBox);

        AddShortcut(shortcuts, vm.CommandShowLayoutCommand, nameof(vm.CommandShowLayoutCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandShowAutoTranslateCommand, nameof(vm.CommandShowAutoTranslateCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandShowSettingsCommand, nameof(vm.CommandShowSettingsCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.GoToPreviousLineCommand, nameof(vm.GoToPreviousLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToNextLineCommand, nameof(vm.GoToNextLineCommand), ShortcutCategory.General);

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