using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.Validators;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class ShortcutsViewModel : ObservableObject
{
    public ObservableCollection<ShortcutTreeNode> FlatNodes { get; } = new();
    [ObservableProperty] private ObservableCollection<string> _shortcuts;
    [ObservableProperty] private string? _selectedShortcut;
    [ObservableProperty] private ObservableCollection<string> _filters;
    [ObservableProperty] private string _selectedFilter;
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private bool _isControlsEnabled;
    [ObservableProperty] private bool _ctrlIsSelected;
    [ObservableProperty] private bool _altIsSelected;
    [ObservableProperty] private bool _shiftIsSelected;
    [ObservableProperty] private bool _winIsSelected;
    [ObservableProperty] private ShortcutTreeNode? _selectedNode;

    public ObservableCollection<ShortcutTreeNode> Nodes { get; }
    public bool OkPressed { get; set; }
    public Window? Window { get; set; }
    public MainViewModel? MainViewModel { get; set; }
    public TreeView ShortcutsTreeView { get; internal set; }

    private List<ShortCut> _allShortcuts;
    private readonly IWindowService _windowService;

    // Add this flag to prevent updates during selection changes
    private bool _isLoadingSelection = false;

    public ShortcutsViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        SearchText = string.Empty;
        Shortcuts = new ObservableCollection<string>(GetShortcutKeys());
        Filters = new ObservableCollection<string>
        {
            Se.Language.General.All,
            Se.Language.Options.Shortcuts.Assigned,
            Se.Language.Options.Shortcuts.Unassigned,
        };
        SelectedFilter = _filters[0];
        Nodes = new ObservableCollection<ShortcutTreeNode>();
        ShortcutsTreeView = new TreeView();
        _allShortcuts = new List<ShortCut>();
    }

    private static IEnumerable<string> GetShortcutKeys()
    {
        var result = new List<string>();
        var all = Enum.GetValues(typeof(Key)).Cast<Key>().Select(p => p.ToString()).Distinct();
        foreach (var key in all)
        {
            if (key == Key.None.ToStringInvariant() ||
                key == Key.LeftCtrl.ToStringInvariant() ||
                key == Key.RightCtrl.ToStringInvariant() ||
                key == Key.LeftAlt.ToStringInvariant() ||
                key == Key.RightAlt.ToStringInvariant() ||
                key == Key.LeftShift.ToStringInvariant() ||
                key == Key.RightShift.ToStringInvariant())
            {
                continue;
            }

            result.Add(key);
        }

        return result;
    }

    public void LoadShortCuts(MainViewModel vm)
    {
        MainViewModel = vm;
        _allShortcuts = ShortcutsMain.GetAllShortcuts(vm);
        UpdateVisibleShortcuts(string.Empty);
    }

    internal void UpdateVisibleShortcuts(string searchText)
    {
        Nodes.Clear();
        FlatNodes.Clear();
        AddShortcuts(ShortcutCategory.General, Se.Language.Options.Shortcuts.CategoryGeneral, searchText);
        AddShortcuts(ShortcutCategory.SubtitleGridAndTextBox,
            Se.Language.Options.Shortcuts.CategorySubtitleGridAndTextBox, searchText);
        AddShortcuts(ShortcutCategory.SubtitleGrid, Se.Language.Options.Shortcuts.CategorySubtitleGrid, searchText);
        AddShortcuts(ShortcutCategory.Waveform, Se.Language.Options.Shortcuts.CategoryWaveform, searchText);
    }

    private void AddShortcuts(ShortcutCategory category, string categoryName, string searchText)
    {
        var shortcuts = _allShortcuts.Where(p => p.Category == category && Search(searchText, p)).ToList();

        var children = new ObservableCollection<ShortcutTreeNode>();
        foreach (var x in shortcuts)
        {
            var leaf = new ShortcutTreeNode(categoryName, MakeDisplayName(x), x);
            children.Add(leaf);
            FlatNodes.Add(leaf);
        }

        if (children.Count > 0)
        {
            var node = new ShortcutTreeNode(categoryName, categoryName, children);
            Nodes.Add(node);
        }
    }

    private static string MakeDisplayName(ShortCut x, bool includeShortCutKeys = true)
    {
        var name = ShortcutsMain.CommandTranslationLookup.TryGetValue(x.Name, out var displayName)
            ? displayName
            : x.Name;

        if (x.Keys.Count > 0 && includeShortCutKeys)
        {
            var keys = x.Keys.Select(k => GetKeyDisplayName(k)).ToList();
            return name + " [" + string.Join(" + ", keys) + "]";
        }

        return name;
    }

    private static string GetKeyDisplayName(string key)
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

    [RelayCommand]
    private void CommandOk()
    {
        var shortcuts = new List<SeShortCut>();
        foreach (var shortcut in _allShortcuts)
        {
            if (shortcut != null && !IsEmpty(shortcut))
            {
                shortcuts.Add(new SeShortCut(shortcut));
            }
        }

        Se.Settings.Shortcuts = shortcuts;
        Se.SaveSettings();

        OkPressed = true;
        Window?.Close();
    }

    private static bool IsEmpty(ShortCut shortcut)
    {
        var modifiers = new List<string>()
        {
            "Control",
            "Ctrl",
            "Alt",
            "Shift",
            "Win",
            Key.LeftCtrl.ToStringInvariant(),
            Key.RightCtrl.ToStringInvariant(),
            Key.LeftAlt.ToStringInvariant(),
            Key.RightAlt.ToStringInvariant(),
            Key.LeftShift.ToStringInvariant(),
            Key.RightShift.ToStringInvariant(),
            Key.LWin.ToStringInvariant(),
            Key.RWin.ToStringInvariant()
        };

        if (shortcut.Keys.Any(k => !modifiers.Contains(k)))
        {
            return false;
        }

        return true;
    }

    [RelayCommand]
    private void CommandCancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private async Task ShowGetKey()
    {
        var node = SelectedNode;
        if (node?.ShortCut == null || Window == null)
        {
            return;
        }

        var result =
            await _windowService
                .ShowDialogAsync<GetKeyWindow, GetKeyViewModel>(Window, vm =>
                {
                    vm.Initialize(string.Format(Se.Language.Options.Shortcuts.SetShortcutForX, MakeDisplayName(node.ShortCut, false)));
                });

        if (result.OkPressed && !string.IsNullOrEmpty(result.PressedKey))
        {
            SelectedShortcut = result.PressedKeyOnly;
            CtrlIsSelected = result.IsControlPressed;
            AltIsSelected = result.IsAltPressed;
            ShiftIsSelected = result.IsShiftPressed;
            WinIsSelected = result.IsWinPressed;
            UpdateShortcut();
        }
    }

    [RelayCommand]
    private void UpdateShortcut()
    {
        // Don't update if we're loading a new selection
        if (_isLoadingSelection)
        {
            return;
        }

        var shortcut = SelectedShortcut;
        var node = SelectedNode;
        if (node == null || node.ShortCut is null)
        {
            return;
        }

        var keys = new List<string>();

        if (ShiftIsSelected)
        {
            keys.Add("Shift");
        }

        if (CtrlIsSelected)
        {
            keys.Add("Ctrl");
        }

        if (AltIsSelected)
        {
            keys.Add("Alt");
        }

        if (WinIsSelected)
        {
            keys.Add("Win");
        }

        if (!string.IsNullOrEmpty(shortcut))
        {
            keys.Add(shortcut);
        }

        node.ShortCut.Keys = keys;
        node.Title = MakeDisplayName(node.ShortCut!);
    }

    [RelayCommand]
    private void ResetShortcut()
    {
        var shortcut = SelectedShortcut;
        var node = SelectedNode;
        if (string.IsNullOrEmpty(shortcut) || node?.ShortCut is null)
        {
            return;
        }

        node.ShortCut.Keys = new List<string>();
        node.Title = MakeDisplayName(node.ShortCut!);
        CtrlIsSelected = false;
        AltIsSelected = false;
        ShiftIsSelected = false;
        WinIsSelected = false;
        SelectedShortcut = null;
        UpdateShortcut();
    }

    [RelayCommand]
    private async Task ResetAllShortcuts()
    {
        if (MainViewModel == null)
        {
            return;
        }

        var answer = await MessageBox.Show(
                  Window!,
                  Se.Language.Options.Shortcuts.ResetShortcuts,
                  Se.Language.Options.Shortcuts.ResetShortcutsDetail,
                  MessageBoxButtons.YesNoCancel,
                  MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        Se.Settings.Shortcuts.Clear();
        Se.Settings.InitializeMainShortcuts(MainViewModel);
        _allShortcuts = ShortcutsMain.GetAllShortcuts(MainViewModel);
        UpdateVisibleShortcuts(string.Empty);
    }

    private bool Search(string searchText, ShortCut p)
    {
        var filterOk = SelectedFilter == Se.Language.General.All ||
                       SelectedFilter == Se.Language.Options.Shortcuts.Unassigned && p.Keys.Count == 0 ||
                       SelectedFilter == Se.Language.Options.Shortcuts.Assigned && p.Keys.Count > 0;
        if (!filterOk)
        {
            return false;
        }

        if (string.IsNullOrEmpty(searchText))
        {
            return true;
        }

        var title = MakeDisplayName(p);
        return title.Contains(searchText, StringComparison.InvariantCultureIgnoreCase);
    }

    internal void ShortcutsDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems == null || e.AddedItems.Count == 0 || e.AddedItems[0] is not ShortcutTreeNode node ||
            node.ShortCut == null)
        {
            IsControlsEnabled = false;
            return;
        }

        // Set flag to prevent UpdateShortcut from running during selection load
        _isLoadingSelection = true;

        try
        {
            IsControlsEnabled = true;
            CtrlIsSelected = node.ShortCut.Keys.Contains("Ctrl") ||
                             node.ShortCut.Keys.Contains("Control") ||
                             node.ShortCut.Keys.Contains(Key.LeftCtrl.ToStringInvariant()) ||
                             node.ShortCut.Keys.Contains(Key.RightCtrl.ToStringInvariant());
            AltIsSelected = node.ShortCut.Keys.Contains("Alt") ||
                            node.ShortCut.Keys.Contains(Key.LeftAlt.ToStringInvariant()) ||
                            node.ShortCut.Keys.Contains(Key.RightAlt.ToStringInvariant());
            ShiftIsSelected = node.ShortCut.Keys.Contains("Shift") ||
                              node.ShortCut.Keys.Contains(Key.LeftShift.ToStringInvariant()) ||
                              node.ShortCut.Keys.Contains(Key.RightShift.ToStringInvariant());
            WinIsSelected = node.ShortCut.Keys.Contains("Win") ||
                              node.ShortCut.Keys.Contains(Key.LWin.ToStringInvariant()) ||
                              node.ShortCut.Keys.Contains(Key.RWin.ToStringInvariant());

            var modifiers = new List<string>()
            {
                "Control",
                "Ctrl",
                "Alt",
                "Shift",
                "Win",
                Key.LeftCtrl.ToStringInvariant(),
                Key.RightCtrl.ToStringInvariant(),
                Key.LeftAlt.ToStringInvariant(),
                Key.RightAlt.ToStringInvariant(),
                Key.LeftShift.ToStringInvariant(),
                Key.RightShift.ToStringInvariant(),
                Key.LWin.ToStringInvariant(),
                Key.RWin.ToStringInvariant()
            };

            foreach (var key in node.ShortCut.Keys)
            {
                if (modifiers.Contains(key))
                {
                    continue;
                }

                SelectedShortcut = key;
                return;
            }

            SelectedShortcut = null;
        }
        finally
        {
            // Always reset the flag, even if an exception occurs
            _isLoadingSelection = false;
        }
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void ComboBoxFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateVisibleShortcuts(SearchText);
    }
}