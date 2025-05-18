using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class ShortcutsViewModel : ObservableObject
{

    [ObservableProperty] private ObservableCollection<ShortcutItem> _shortcuts;
    [ObservableProperty] private ShortcutItem? _selectedShortcut;
    [ObservableProperty] private bool _controlsEnabled;
    [ObservableProperty] private bool _ctrlIsSelected;
    [ObservableProperty] private bool _altIsSelected;
    [ObservableProperty] private bool _shiftIsSelected;
    private List<ShortcutItem> _allShortcuts;

    public HierarchicalTreeDataGridSource<ShortcutItem> ShortcutsSource { get; set; }

    public bool OkPressed { get; set; }
    public ShortcutsWindow? Window { get; set; }
    public TreeDataGrid ShortcutsGrid { get; internal set; }

    public ShortcutsViewModel()
    {
        Shortcuts = new ObservableCollection<ShortcutItem>();
        _allShortcuts = new List<ShortcutItem>();

        ShortcutsSource = new HierarchicalTreeDataGridSource<ShortcutItem>(_shortcuts)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<ShortcutItem>(
                    new TextColumn<ShortcutItem, string>("Category",
                    x => x.CategoryText),
                    x => x.Children),
                new TextColumn<ShortcutItem, string>("Function", x => x.Name),
                new TextColumn<ShortcutItem, string>("Keys", x => x.Keys),
            },
        };
    }

    public void LoadShortCuts(MainViewModel vm)
    {
        var categories = new List<ShortcutItem>();
        foreach (var shortcut in ShortcutsMain.GetAllShortcuts(vm))
        {
            var categoryEnum = shortcut.Category;
            var category = categories.FirstOrDefault(x => x.Category == categoryEnum);
            if (category == null)
            {
                category = new ShortcutItem
                {
                    Name = string.Empty,
                    Keys = string.Empty,
                    Category = categoryEnum,
                    CategoryText = Localize(categoryEnum),
                };
                categories.Add(category);
                Shortcuts.Add(category);
            }

            var item = new ShortcutItem
            {
                Category = categoryEnum,
                CategoryText = string.Empty,
                Keys = string.Join('+', shortcut.Keys),
                Name = shortcut.Name,
            };

            category.Children.Add(item);
            _allShortcuts.Add(item);
        }
        
        ShortcutsSource.ExpandAll();
    }

    private static string Localize(ShortcutCategory categoryEnum)
    {
        return categoryEnum.ToString();  //TODO: localize
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
        var categories = new List<ShortcutItem>();
        Shortcuts.Clear();
        foreach (var shortcut in _allShortcuts)
        {
            if (shortcut.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                shortcut.Keys.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                var categoryEnum = shortcut.Category;
                var category = categories.FirstOrDefault(x => x.Category == categoryEnum);
                if (category == null)
                {
                    category = new ShortcutItem
                    {
                        Name = string.Empty,
                        Keys = string.Empty,
                        Category = categoryEnum,
                        CategoryText = Localize(categoryEnum),
                    };
                    categories.Add(category);
                    Shortcuts.Add(category);
                }

                category.Children.Add(shortcut);
            }
        }
        
        ShortcutsSource.ExpandAll();
    }

    public void ShortcutGrid_SelectionChanged(IReadOnlyList<ShortcutItem?> rowSelectionSelectedItems)
    {
        var shortcut = rowSelectionSelectedItems.FirstOrDefault();
        if (shortcut?.Children.Count > 0)
        {
            shortcut = null;
        }

        SelectedShortcut = shortcut;
        ControlsEnabled = shortcut != null;
        if (shortcut != null)
        {
            CtrlIsSelected = shortcut.Keys.Contains("Ctrl");
            AltIsSelected = shortcut.Keys.Contains("Alt");
            ShiftIsSelected = shortcut.Keys.Contains("Shift");
        }
    }
}