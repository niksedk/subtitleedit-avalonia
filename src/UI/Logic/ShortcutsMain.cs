using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

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
        { nameof(_mvm.SelectAllLinesCommand) , Se.Language.Options.Shortcuts.ListSelectAll },
        { nameof(_mvm.InverseSelectionCommand) , Se.Language.Options.Shortcuts.ListInverseSelection },
        { nameof(_mvm.DeleteSelectedLinesCommand) , Se.Language.Options.Shortcuts.ListDeleteSelection },
        { nameof(_mvm.OpenDataFolderCommand) , Se.Language.Options.Shortcuts.OpenSeDataFolder },

        { nameof(_mvm.CommandFileOpenCommand) , Se.Language.Options.Shortcuts.FileOpen },
        { nameof(_mvm.CommandExitCommand) , Se.Language.Options.Shortcuts.FileExit },
        { nameof(_mvm.CommandFileNewCommand) , Se.Language.Options.Shortcuts.FileNew },
        { nameof(_mvm.CommandFileSaveCommand) , Se.Language.Options.Shortcuts.FileSave },
        { nameof(_mvm.CommandFileSaveAsCommand) , Se.Language.Options.Shortcuts.FileSaveAs },

        { nameof(_mvm.UndoCommand) , Se.Language.General.Undo },
        { nameof(_mvm.RedoCommand) , Se.Language.General.Redo },
        { nameof(_mvm.ShowFindCommand) , Se.Language.Options.Shortcuts.EditFind },
        { nameof(_mvm.FindNextCommand) , Se.Language.Options.Shortcuts.EditFindNext },
        { nameof(_mvm.ShowReplaceCommand) , Se.Language.Options.Shortcuts.EditReplace },
        { nameof(_mvm.ShowMultipleReplaceCommand) , Se.Language.Options.Shortcuts.EditMultipleReplace },

        { nameof(_mvm.ShowGoToLineCommand) , Se.Language.Options.Shortcuts.GeneralGoToLineNumber },
        { nameof(_mvm.ShowGoToVideoPositionCommand) , Se.Language.Options.Shortcuts.GeneralGoToVideoPosition },
        { nameof(_mvm.ToggleLinesItalicCommand) , Se.Language.Options.Shortcuts.GeneralToggleItalic },
        { nameof(_mvm.ToggleLinesBoldCommand) , Se.Language.Options.Shortcuts.GeneralToggleBold },

        { nameof(_mvm.TogglePlayPauseCommand) , Se.Language.Options.Shortcuts.TogglePlayPause },
        { nameof(_mvm.TogglePlayPause2Command) , Se.Language.Options.Shortcuts.TogglePlayPause },

        { nameof(_mvm.CommandShowLayoutCommand) , Se.Language.Options.Shortcuts.GeneralChooseLayout },
        { nameof(_mvm.CommandShowAutoTranslateCommand) , Se.Language.Options.Shortcuts.AutoTranslate },
        { nameof(_mvm.CommandShowSettingsCommand) , Se.Language.Options.Shortcuts.Settings },

        { nameof(_mvm.GoToNextLineCommand) , Se.Language.Options.Shortcuts.GeneralGoToNextSubtitle },
        { nameof(_mvm.GoToPreviousLineCommand) , Se.Language.Options.Shortcuts.GeneralGoToPrevSubtitle },
        { nameof(_mvm.SaveLanguageFileCommand) , "Save language file" },
    };

    private static List<AvailableShortcut> GetAllAvailableShortcuts(MainViewModel vm)
    {
        var shortcuts = new List<AvailableShortcut>();

        AddShortcut(shortcuts, vm.SelectAllLinesCommand, nameof(vm.SelectAllLinesCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.InverseSelectionCommand, nameof(vm.InverseSelectionCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.DeleteSelectedLinesCommand, nameof(vm.DeleteSelectedLinesCommand), ShortcutCategory.SubtitleGrid);

        AddShortcut(shortcuts, vm.CommandFileOpenCommand, nameof(vm.CommandFileOpenCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandExitCommand, nameof(vm.CommandExitCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileNewCommand, nameof(vm.CommandFileNewCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileSaveCommand, nameof(vm.CommandFileSaveCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileSaveAsCommand, nameof(vm.CommandFileSaveAsCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.UndoCommand, nameof(vm.UndoCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.RedoCommand, nameof(vm.RedoCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowFindCommand, nameof(vm.ShowFindCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.FindNextCommand, nameof(vm.FindNextCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowReplaceCommand, nameof(vm.ShowReplaceCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowMultipleReplaceCommand, nameof(vm.ShowMultipleReplaceCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.ShowGoToLineCommand, nameof(vm.ShowGoToLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowGoToVideoPositionCommand, nameof(vm.ShowGoToVideoPositionCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleLinesItalicCommand, nameof(vm.ToggleLinesItalicCommand), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.ToggleLinesBoldCommand, nameof(vm.ToggleLinesBoldCommand), ShortcutCategory.SubtitleGridAndTextBox);

        AddShortcut(shortcuts, vm.TogglePlayPauseCommand, nameof(vm.TogglePlayPauseCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.TogglePlayPause2Command, nameof(vm.TogglePlayPause2Command), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.CommandShowLayoutCommand, nameof(vm.CommandShowLayoutCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandShowAutoTranslateCommand, nameof(vm.CommandShowAutoTranslateCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandShowSettingsCommand, nameof(vm.CommandShowSettingsCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.GoToPreviousLineCommand, nameof(vm.GoToPreviousLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToNextLineCommand, nameof(vm.GoToNextLineCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.OpenDataFolderCommand, nameof(vm.OpenDataFolderCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SaveLanguageFileCommand, nameof(vm.SaveLanguageFileCommand), ShortcutCategory.General);

        return shortcuts;
    }

    public static List<SeShortCut> GetDefaultShorcuts(MainViewModel vm)
    {
        var commandOrWin = GetCommandOrWin();

        return new List<SeShortCut>
        {
            new(nameof(vm.UndoCommand), new List<string> { commandOrWin, "Z" }),
            new(nameof(vm.RedoCommand), new List<string> { commandOrWin, "Y" }),
            new(nameof(vm.ShowGoToLineCommand), new List<string> { commandOrWin, "G" }),
            new(nameof(vm.GoToPreviousLineCommand), new List<string> { "Alt", "Up" }),
            new(nameof(vm.GoToNextLineCommand), new List<string> { "Alt", "Down" }),
            new(nameof(vm.SelectAllLinesCommand), new List<string> { commandOrWin, "A" }, ShortcutCategory.SubtitleGrid),
            new(nameof(vm.InverseSelectionCommand), new List<string> { commandOrWin, "Shift", "I" }, ShortcutCategory.SubtitleGrid),
            new(nameof(vm.ToggleLinesItalicCommand), new List<string> { commandOrWin, "I" }, ShortcutCategory.SubtitleGrid),
            new(nameof(vm.DeleteSelectedLinesCommand), new List<string> { "Delete" }, ShortcutCategory.SubtitleGrid),
            new(nameof(vm.ShowFindCommand), new List<string> { commandOrWin, "F" }, ShortcutCategory.General),
            new(nameof(vm.FindNextCommand), new List<string> { "F3" }, ShortcutCategory.General),
            new(nameof(vm.ShowReplaceCommand), new List<string> { commandOrWin, "H" }, ShortcutCategory.General),
            new(nameof(vm.OpenDataFolderCommand), new List<string> { commandOrWin, "Alt", "Shift", "D" }, ShortcutCategory.General),
            new(nameof(vm.CommandFileNewCommand), new List<string> { commandOrWin, "N" }, ShortcutCategory.General),
            new(nameof(vm.CommandFileSaveCommand), new List<string> { commandOrWin, "S" }, ShortcutCategory.General),
            new(nameof(vm.TogglePlayPauseCommand), new List<string> { Avalonia.Input.Key.Space.ToString() }, ShortcutCategory.General),
            new(nameof(vm.TogglePlayPause2Command), new List<string> { commandOrWin, Avalonia.Input.Key.Space.ToString() }, ShortcutCategory.General),
       };
    }

    private static string GetCommandOrWin()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "Win";
        }

        return "Ctrl";
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