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

    [ObservableProperty] private bool _isControlsEnabled;
    [ObservableProperty] private bool _ctrlIsSelected;
    [ObservableProperty] private bool _altIsSelected;
    [ObservableProperty] private bool _shiftIsSelected;
    private List<ShortCut> _allShortcuts;
    [ObservableProperty] private ShortcutTreeNode? _selectedNode;

    public ObservableCollection<ShortcutTreeNode> Nodes { get; }

    public bool OkPressed { get; set; }
    public ShortcutsWindow? Window { get; set; }
    public TreeView ShortcutsTreeView { get; internal set; }

    public ShortcutsViewModel()
    {
        Shortcuts = new ObservableCollection<string>(GetShortcutKeys());
        _allShortcuts = new List<ShortCut>();
        Nodes = new ObservableCollection<ShortcutTreeNode>();
        ShortcutsTreeView = new TreeView();
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
        _allShortcuts = ShortcutsMain.GetAllShortcuts(vm);
        LoadShortcuts();
    }

    private void LoadShortcuts()
    {
        Nodes.Clear();
        AddShortcuts(_allShortcuts.Where(p => p.Category == ShortcutCategory.General).ToList(), "General");
        AddShortcuts(_allShortcuts.Where(p => p.Category == ShortcutCategory.SubtitleGridAndTextBox).ToList(),
            "SubtitleGridAndTextBox");
        AddShortcuts(_allShortcuts.Where(p => p.Category == ShortcutCategory.SubtitleGrid).ToList(), "SubtitleGrid");
        AddShortcuts(_allShortcuts.Where(p => p.Category == ShortcutCategory.Waveform).ToList(), "Waveform");
        ExpandAll();
    }

    private void AddShortcuts(List<ShortCut> shortcuts, string categoryName)
    {
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
        if (x.Keys.Count > 0)
        {
            return x.Name + " [" + string.Join("+", x.Keys) + "]";
        }

        return x.Name;
    }

    [RelayCommand]
    private void CommandOk()
    {
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
            keys.Add("Ctrl");
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
        SelectedNode.Title = MakeDisplayName(node.ShortCut!);
    }

    internal void UpdateVisibleShortcuts(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            LoadShortcuts();
            return;
        }

        Nodes.Clear();
        AddShortcuts(
            _allShortcuts.Where(p =>
                p.Category == ShortcutCategory.General &&
                p.Name.Contains(searchText, System.StringComparison.InvariantCultureIgnoreCase)).ToList(), "General");
        AddShortcuts(
            _allShortcuts.Where(p =>
                p.Category == ShortcutCategory.SubtitleGridAndTextBox && p.Name.Contains(searchText,
                    System.StringComparison.InvariantCultureIgnoreCase)).ToList(), "SubtitleGridAndTextBox");
        AddShortcuts(
            _allShortcuts.Where(p =>
                p.Category == ShortcutCategory.SubtitleGrid &&
                p.Name.Contains(searchText, System.StringComparison.InvariantCultureIgnoreCase)).ToList(),
            "SubtitleGrid");
        AddShortcuts(
            _allShortcuts.Where(p =>
                p.Category == ShortcutCategory.Waveform &&
                p.Name.Contains(searchText, System.StringComparison.InvariantCultureIgnoreCase)).ToList(), "Waveform");

        ExpandAll();
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


    // Expand all nodes
    public void ExpandAll()
    {
        // Wait for layout to complete to ensure all items are generated
        Dispatcher.UIThread.Post(() =>
        {
            var allTreeViewItems = FindAllTreeViewItems(ShortcutsTreeView);
            foreach (var item in allTreeViewItems)
            {
                item.IsExpanded = true;
            }
        }, DispatcherPriority.Background);
    }

    // Collapse all nodes
    public void CollapseAll()
    {
        // Wait for layout to complete to ensure all items are generated
        Dispatcher.UIThread.Post(() =>
        {
            var allTreeViewItems = FindAllTreeViewItems(ShortcutsTreeView);
            foreach (var item in allTreeViewItems)
            {
                item.IsExpanded = false;
            }
        }, DispatcherPriority.Background);
    }

    // Helper method to find all TreeViewItems using LogicalChildren
    private IEnumerable<TreeViewItem> FindAllTreeViewItems(Control parent)
    {
        var result = new List<TreeViewItem>();

        // If the parent is a TreeViewItem, add it to the result
        if (parent is TreeViewItem tvi)
        {
            result.Add(tvi);
        }

        // Get logical children and recursively process them
        foreach (var child in parent.GetLogicalDescendants())
        {
            if (child is TreeViewItem treeViewItem)
            {
                result.Add(treeViewItem);
            }
        }

        return result;
    }
}