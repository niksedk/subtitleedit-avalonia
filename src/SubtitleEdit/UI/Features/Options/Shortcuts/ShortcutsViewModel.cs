using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class ShortcutsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ShortcutItem> _shortcuts;

    public HierarchicalTreeDataGridSource<ShortcutItem> ShortcutsSource { get; set; }

    public bool OkPressed { get; set; }
    public ShortcutsWindow Window { get; set; }

    public ShortcutsViewModel()
    {
        Shortcuts = new ObservableCollection<ShortcutItem>( );
        
        ShortcutsSource = new HierarchicalTreeDataGridSource<ShortcutItem>(_shortcuts)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<ShortcutItem>(
                    new TextColumn<ShortcutItem, string>("Category", x => x.CategoryText),
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
            var categoryName = shortcut.Control ?? "General";
            var category = categories.FirstOrDefault(x => x.Name == categoryName);
            if (category == null)
            {
                category = new ShortcutItem
                {
                    Name = string.Empty,
                    Keys = string.Empty,
                    Category = ShortcutCategory.General,
                    CategoryText = "General",
                };
                categories.Add(category);
                Shortcuts.Add(category);
            }
            
            category.Children.Add(new ShortcutItem
            {
                Category = ShortcutCategory.General,
                CategoryText = category.Name,
                Keys = string.Join('+', shortcut.Keys),
                Name = shortcut.Name,
            });
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
}