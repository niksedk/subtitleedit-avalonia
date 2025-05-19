using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class ShortcutsViewModel : ObservableObject
{

    [ObservableProperty] private ObservableCollection<ShortcutItem> _shortcuts;
    [ObservableProperty] private ShortcutItem? _selectedShortcut;
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
        Shortcuts = new ObservableCollection<ShortcutItem>();
        _allShortcuts = new List<ShortCut>();
        Nodes = new ObservableCollection<ShortcutTreeNode>();
        ShortcutsTreeView = new TreeView();
    }

    public void LoadShortCuts(MainViewModel vm)
    {
        _allShortcuts = ShortcutsMain.GetAllShortcuts(vm);
        LoadShortcuts();
    }

    private void LoadShortcuts()
    {
        Nodes.Clear();
        AddShorcuts(_allShortcuts.Where(p => p.Category == ShortcutCategory.General).ToList(), "General");
        AddShorcuts(_allShortcuts.Where(p => p.Category == ShortcutCategory.SubtitleGridAndTextBox).ToList(), "SubtitleGridAndTextBox");
        AddShorcuts(_allShortcuts.Where(p => p.Category == ShortcutCategory.SubtitleGrid).ToList(), "SubtitleGrid");
        AddShorcuts(_allShortcuts.Where(p => p.Category == ShortcutCategory.Waveform).ToList(), "Waveform");
        ExpandAll();
    }

    private void AddShorcuts(List<ShortCut> shortcuts, string categoryName)
    {
        var children = new ObservableCollection<ShortcutTreeNode>(
            shortcuts.Select(x => new ShortcutTreeNode(x.Name + " [" + string.Join("+", x.Keys) + "]", x))
        );

        if (children.Count > 0)
        {
            var node = new ShortcutTreeNode(categoryName, children);
            Nodes.Add(node);
        }
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

    internal void UpdateVisibleShortcuts(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            LoadShortcuts();
            return;
        }

        Nodes.Clear();
        AddShorcuts(_allShortcuts.Where(p => p.Category == ShortcutCategory.General && p.Name.Contains(searchText, System.StringComparison.InvariantCultureIgnoreCase)).ToList(), "General");
        AddShorcuts(_allShortcuts.Where(p => p.Category == ShortcutCategory.SubtitleGridAndTextBox && p.Name.Contains(searchText, System.StringComparison.InvariantCultureIgnoreCase)).ToList(), "SubtitleGridAndTextBox");
        AddShorcuts(_allShortcuts.Where(p => p.Category == ShortcutCategory.SubtitleGrid && p.Name.Contains(searchText, System.StringComparison.InvariantCultureIgnoreCase)).ToList(), "SubtitleGrid");
        AddShorcuts(_allShortcuts.Where(p => p.Category == ShortcutCategory.Waveform && p.Name.Contains(searchText, System.StringComparison.InvariantCultureIgnoreCase)).ToList(), "Waveform");

        ExpandAll();
    }

   
    internal void ShortcutsTreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems == null || e.AddedItems.Count == 0 || e.AddedItems[0] is not ShortcutTreeNode node || node.ShortCut == null)
        {
            IsControlsEnabled = false;
            return;
        }

        IsControlsEnabled = true;
        CtrlIsSelected = node.ShortCut!.Keys.Contains("Ctrl");
        AltIsSelected = node.ShortCut!.Keys.Contains("Alt");
        ShiftIsSelected = node.ShortCut!.Keys.Contains("Shift");
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