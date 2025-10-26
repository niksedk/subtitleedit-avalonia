using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PickColor;
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
    [ObservableProperty] private bool _isConfigureVisible;
    [ObservableProperty] private ShortcutTreeNode? _selectedNode;

    public bool OkPressed { get; set; }
    public Window? Window { get; set; }
    public MainViewModel? MainViewModel { get; set; }
    public TreeView ShortcutsTreeView { get; internal set; }

    private List<ShortCut> _allShortcuts;
    private readonly IWindowService _windowService;
    private List<IRelayCommand> _configurableCommands;
    private Color _color1;
    private Color _color2;
    private Color _color3;
    private Color _color4;
    private Color _color5;
    private Color _color6;
    private Color _color7;
    private Color _color8;
    private string _surround1Left;
    private string _surround1Right;

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
        ShortcutsTreeView = new TreeView();
        _allShortcuts = new List<ShortCut>();
        _configurableCommands = new List<IRelayCommand>();
        _color1 = Se.Settings.Color1.FromHexToColor();
        _color2 = Se.Settings.Color2.FromHexToColor();
        _color3 = Se.Settings.Color3.FromHexToColor();
        _color4 = Se.Settings.Color4.FromHexToColor();
        _color5 = Se.Settings.Color5.FromHexToColor();
        _color6 = Se.Settings.Color6.FromHexToColor();
        _color7 = Se.Settings.Color7.FromHexToColor();
        _color8 = Se.Settings.Color8.FromHexToColor();
        _surround1Left = Se.Settings.Surround1Left;
        _surround1Right = Se.Settings.Surround1Right;
    }

    private static IEnumerable<string> GetShortcutKeys()
    {
        var result = new List<string>();
        var all = Enum.GetValues(typeof(Key)).Cast<Key>().Select(p => p.ToString()).Distinct();
        foreach (var key in all)
        {
            if (key == Key.None.ToString() ||
                key == Key.LeftCtrl.ToString() ||
                key == Key.RightCtrl.ToString() ||
                key == Key.LeftAlt.ToString() ||
                key == Key.RightAlt.ToString() ||
                key == Key.LeftShift.ToString() ||
                key == Key.RightShift.ToString())
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

        _configurableCommands.Add(vm.SetColor1Command);
        _configurableCommands.Add(vm.SetColor2Command);
        _configurableCommands.Add(vm.SetColor3Command);
        _configurableCommands.Add(vm.SetColor4Command);
        _configurableCommands.Add(vm.SetColor5Command);
        _configurableCommands.Add(vm.SetColor6Command);
        _configurableCommands.Add(vm.SetColor7Command);
        _configurableCommands.Add(vm.SetColor8Command);
    }

    internal void UpdateVisibleShortcuts(string searchText)
    {
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
            var leaf = new ShortcutTreeNode(categoryName, MakeDisplayName(x, false), MakeDisplayShortCut(x), x);
            children.Add(leaf);
            FlatNodes.Add(leaf);
        }
    }

    private static string MakeDisplayName(ShortCut x, bool includeShortCutKeys = true)
    {
        var name = ShortcutsMain.CommandTranslationLookup.TryGetValue(x.Name, out var displayName)
            ? displayName
            : x.Name;

        if (includeShortCutKeys)
        {
            return name + " " + MakeDisplayShortCut(x);
        }

        return name;
    }

    private static string MakeDisplayShortCut(ShortCut shortCut)
    {
        if (shortCut.Keys.Count > 0)
        {
            var keys = shortCut.Keys.Select(k => GetKeyDisplayName(k)).ToList();
            return "[" + string.Join(" + ", keys) + "]";
        }

        return string.Empty;
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

        Se.Settings.Color1 = _color1.FromColorToHex();
        Se.Settings.Color2 = _color2.FromColorToHex();
        Se.Settings.Color3 = _color3.FromColorToHex();
        Se.Settings.Color4 = _color4.FromColorToHex();
        Se.Settings.Color5 = _color5.FromColorToHex();
        Se.Settings.Color6 = _color6.FromColorToHex();
        Se.Settings.Color7 = _color7.FromColorToHex();
        Se.Settings.Color8 = _color8.FromColorToHex();
        Se.Settings.Surround1Left = _surround1Left;
        Se.Settings.Surround1Right = _surround1Right;

        Se.SaveSettings();

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private async Task Configure()
    {
        var node = SelectedNode;
        if (Window == null || MainViewModel == null || node?.ShortCut == null)
        {
            return;
        }

        if (node.ShortCut.Action == MainViewModel.SetColor1Command)
        {
            var result = await _windowService.ShowDialogAsync<PickColorWindow, PickColorViewModel>(Window, vm =>
            {
                vm.Initialize(_color1);
            });
            if (result.OkPressed)
            {
                _color1 = result.SelectedColor;
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SetColor2Command)
        {
            var result = await _windowService.ShowDialogAsync<PickColorWindow, PickColorViewModel>(Window, vm =>
            {
                vm.Initialize(_color2);
            });
            if (result.OkPressed)
            {
                _color2 = result.SelectedColor;
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SetColor3Command)
        {
            var result = await _windowService.ShowDialogAsync<PickColorWindow, PickColorViewModel>(Window, vm =>
            {
                vm.Initialize(_color3);
            });
            if (result.OkPressed)
            {
                _color3 = result.SelectedColor;
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SetColor4Command)
        {
            var result = await _windowService.ShowDialogAsync<PickColorWindow, PickColorViewModel>(Window, vm =>
            {
                vm.Initialize(_color4);
            });
            if (result.OkPressed)
            {
                _color4 = result.SelectedColor;
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SetColor5Command)
        {
            var result = await _windowService.ShowDialogAsync<PickColorWindow, PickColorViewModel>(Window, vm =>
            {
                vm.Initialize(_color5);
            });
            if (result.OkPressed)
            {
                _color5 = result.SelectedColor;
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SetColor6Command)
        {
            var result = await _windowService.ShowDialogAsync<PickColorWindow, PickColorViewModel>(Window, vm =>
            {
                vm.Initialize(_color6);
            });
            if (result.OkPressed)
            {
                _color6 = result.SelectedColor;
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SetColor7Command)
        {
            var result = await _windowService.ShowDialogAsync<PickColorWindow, PickColorViewModel>(Window, vm =>
            {
                vm.Initialize(_color7);
            });
            if (result.OkPressed)
            {
                _color7 = result.SelectedColor;
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SetColor8Command)
        {
            var result = await _windowService.ShowDialogAsync<PickColorWindow, PickColorViewModel>(Window, vm =>
            {
                vm.Initialize(_color8);
            });
            if (result.OkPressed)
            {
                _color8 = result.SelectedColor;
            }
        }
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
            Key.LeftCtrl.ToString(),
            Key.RightCtrl.ToString(),
            Key.LeftAlt.ToString(),
            Key.RightAlt.ToString(),
            Key.LeftShift.ToString(),
            Key.RightShift.ToString(),
            Key.LWin.ToString(),
            Key.RWin.ToString()
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
        node.DisplayShortcut = MakeDisplayShortCut(node.ShortCut);
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
            IsConfigureVisible = false;
            return;
        }

        IsConfigureVisible = _configurableCommands.Contains(node.ShortCut.Action);

        // Set flag to prevent UpdateShortcut from running during selection load
        _isLoadingSelection = true;

        try
        {
            IsControlsEnabled = true;
            CtrlIsSelected = node.ShortCut.Keys.Contains("Ctrl") ||
                             node.ShortCut.Keys.Contains("Control") ||
                             node.ShortCut.Keys.Contains(Key.LeftCtrl.ToString()) ||
                             node.ShortCut.Keys.Contains(Key.RightCtrl.ToString());
            AltIsSelected = node.ShortCut.Keys.Contains("Alt") ||
                            node.ShortCut.Keys.Contains(Key.LeftAlt.ToString()) ||
                            node.ShortCut.Keys.Contains(Key.RightAlt.ToString());
            ShiftIsSelected = node.ShortCut.Keys.Contains("Shift") ||
                              node.ShortCut.Keys.Contains(Key.LeftShift.ToString()) ||
                              node.ShortCut.Keys.Contains(Key.RightShift.ToString());
            WinIsSelected = node.ShortCut.Keys.Contains("Win") ||
                              node.ShortCut.Keys.Contains(Key.LWin.ToString()) ||
                              node.ShortCut.Keys.Contains(Key.RWin.ToString());

            var modifiers = new List<string>()
            {
                "Control",
                "Ctrl",
                "Alt",
                "Shift",
                "Win",
                Key.LeftCtrl.ToString(),
                Key.RightCtrl.ToString(),
                Key.LeftAlt.ToString(),
                Key.RightAlt.ToString(),
                Key.LeftShift.ToString(),
                Key.RightShift.ToString(),
                Key.LWin.ToString(),
                Key.RWin.ToString()
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

    internal void ShortcutsDataGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        _ = ShowGetKey();
    }
}