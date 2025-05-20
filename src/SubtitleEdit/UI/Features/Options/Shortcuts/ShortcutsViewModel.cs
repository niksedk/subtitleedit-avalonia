using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HanumanInstitute.Validators;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class ShortcutsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _shortcuts;
    [ObservableProperty] private string? _selectedShortcut;
    [ObservableProperty] private ObservableCollection<string> _filters;
    [ObservableProperty] private string _selectedFilter;
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private bool _isControlsEnabled;
    [ObservableProperty] private bool _ctrlIsSelected;
    [ObservableProperty] private bool _altIsSelected;
    [ObservableProperty] private bool _shiftIsSelected;
    [ObservableProperty] private ShortcutTreeNode? _selectedNode;

    public ObservableCollection<ShortcutTreeNode> Nodes { get; }
    public bool OkPressed { get; set; }
    public ShortcutsWindow? Window { get; set; }
    public TreeView ShortcutsTreeView { get; internal set; }

    private List<ShortCut> _allShortcuts;
    public MainViewModel? _mainViewModel;

    public ShortcutsViewModel()
    {
        Shortcuts = new ObservableCollection<string>(GetShortcutKeys());
        Filters = new ObservableCollection<string>
        {
            Se.Language.General.All,
            Se.Language.Settings.Shortcuts.Assigned,
            Se.Language.Settings.Shortcuts.Unassigned,
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
        _mainViewModel = vm;
        _allShortcuts = ShortcutsMain.GetAllShortcuts(vm);
        UpdateVisibleShortcuts(string.Empty);
    }

    internal void UpdateVisibleShortcuts(string searchText)
    {
        Nodes.Clear();
        AddShortcuts(ShortcutCategory.General, Se.Language.Settings.Shortcuts.CategoryGeneral, searchText);
        AddShortcuts(ShortcutCategory.SubtitleGridAndTextBox, Se.Language.Settings.Shortcuts.CategorySubtitleGridAndTextBox, searchText);
        AddShortcuts(ShortcutCategory.SubtitleGrid, Se.Language.Settings.Shortcuts.CategorySubtitleGrid, searchText);
        AddShortcuts(ShortcutCategory.Waveform, Se.Language.Settings.Shortcuts.CategoryWaveform, searchText);
        ExpandAll();
    }

    private void AddShortcuts(ShortcutCategory category, string categoryName, string searchText)
    {
        List<ShortCut> shortcuts = _allShortcuts.Where(p => p.Category == category && Search(searchText, p)).ToList();

        var children = new ObservableCollection<ShortcutTreeNode>(
            shortcuts.Select(x => new ShortcutTreeNode(MakeDisplayName(x), x))
        );

        if (children.Count > 0)
        {
            var node = new ShortcutTreeNode(categoryName, children);
            Nodes.Add(node);
        }
    }

    private static string MakeDisplayName(ShortCut x)
    {
        var name = ShortcutsMain.CommandTranslationLookup.TryGetValue(x.Name, out var displayName) ? displayName : x.Name;

        if (x.Keys.Count > 0)
        {
            return name + " [" + string.Join("+", x.Keys) + "]";
        }

        return name;
    }

    [RelayCommand]
    private void CommandOk()
    {
        var shortcuts = new List<SeShortCut>();
        foreach (var node in Nodes.Where(p => p.SubNodes != null))
        {
            foreach (var child in node.SubNodes!)
            {
                if (child.ShortCut != null)
                {
                    shortcuts.Add(new SeShortCut(child.ShortCut));
                }
            }
        }

        Se.Settings.Shortcuts = shortcuts;
        Se.SaveSettings();

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void CommandCancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void UpdateShortcut()
    {
        var shortcut = SelectedShortcut;
        var node = SelectedNode;
        if (string.IsNullOrEmpty(shortcut) || node?.ShortCut is null)
        {
            return;
        }

        var keys = new List<string>();
        if (CtrlIsSelected)
        {
            keys.Add("Control");
        }

        if (AltIsSelected)
        {
            keys.Add("Alt");
        }

        if (ShiftIsSelected)
        {
            keys.Add("Shift");
        }

        keys.Add(shortcut);
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
        SelectedShortcut = null;
    }

    [RelayCommand]
    private void Expand()
    {
        ExpandAll();
    }

    [RelayCommand]
    private void Collapse()
    {
        CollapseAll();
    }

    private bool Search(string searchText, ShortCut p)
    {
        var filterOk = SelectedFilter == Se.Language.General.All ||
                       SelectedFilter == Se.Language.Settings.Shortcuts.Unassigned && p.Keys.Count == 0 ||
                       SelectedFilter == Se.Language.Settings.Shortcuts.Assigned && p.Keys.Count > 0;
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

    internal void ShortcutsTreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems == null || e.AddedItems.Count == 0 || e.AddedItems[0] is not ShortcutTreeNode node ||
            node.ShortCut == null)
        {
            IsControlsEnabled = false;
            return;
        }

        IsControlsEnabled = true;
        CtrlIsSelected = node.ShortCut!.Keys.Contains("Ctrl") ||
                         node.ShortCut!.Keys.Contains("Control") ||
                         node.ShortCut!.Keys.Contains(Key.LeftCtrl.ToStringInvariant()) ||
                         node.ShortCut!.Keys.Contains(Key.RightCtrl.ToStringInvariant());
        AltIsSelected = node.ShortCut!.Keys.Contains("Alt") ||
                        node.ShortCut!.Keys.Contains(Key.LeftAlt.ToStringInvariant()) ||
                        node.ShortCut!.Keys.Contains(Key.RightAlt.ToStringInvariant());
        ShiftIsSelected = node.ShortCut!.Keys.Contains("Shift") ||
                          node.ShortCut!.Keys.Contains(Key.LeftShift.ToStringInvariant()) ||
                          node.ShortCut!.Keys.Contains(Key.RightShift.ToStringInvariant());

        var modifiers = new List<string>()
        {
            "Control",
            "Ctrl",
            "Alt",
            "Shift",
            Key.LeftCtrl.ToStringInvariant(),
            Key.RightCtrl.ToStringInvariant(),
            Key.LeftAlt.ToStringInvariant(),
            Key.RightAlt.ToStringInvariant(),
            Key.LeftShift.ToStringInvariant(),
            Key.RightShift.ToStringInvariant(),
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

    public void ExpandAll()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var allTreeViewItems = FindAllTreeViewItems(ShortcutsTreeView);
            foreach (var item in allTreeViewItems)
            {
                item.IsExpanded = true;
            }
        }, DispatcherPriority.Background);
    }

    public void CollapseAll()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var allTreeViewItems = FindAllTreeViewItems(ShortcutsTreeView);
            foreach (var item in allTreeViewItems)
            {
                item.IsExpanded = false;
            }
        }, DispatcherPriority.Background);
    }

    private IEnumerable<TreeViewItem> FindAllTreeViewItems(Control parent)
    {
        var result = new List<TreeViewItem>();
        if (parent is TreeViewItem tvi)
        {
            result.Add(tvi);
        }

        foreach (var child in parent.GetLogicalDescendants())
        {
            if (child is TreeViewItem treeViewItem)
            {
                result.Add(treeViewItem);
            }
        }

        return result;
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