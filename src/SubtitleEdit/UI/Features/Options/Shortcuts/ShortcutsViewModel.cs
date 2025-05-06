using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        LoadShortCuts();
        
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

    private void LoadShortCuts()
    {
        var general = new ShortcutItem
        {
            Name = string.Empty,
            Keys = string.Empty,
            Age = 4,
            Category = ShortcutCategory.General,
            CategoryText = "General",
        };
        Shortcuts.Add(general);

        var gridAndTextBox = new ShortcutItem
        {
            Name = string.Empty,
            Keys = string.Empty,
            Age = 4,
            Category = ShortcutCategory.SubtitleGridAndTextBox,
            CategoryText = "Subtitle grid and text box",
        };
        Shortcuts.Add(gridAndTextBox);

        var grid = new ShortcutItem
        {
            Name = string.Empty,
            Keys = string.Empty,
            Age = 4,
            Category = ShortcutCategory.SubtitleGrid,
            CategoryText = "Subtitle grid",
        };
        Shortcuts.Add(grid);

        foreach (var item in Se.Settings.Shortcuts)
        {
            if (item.ActionName != "some-cat")
            {
                var sc = new ShortcutItem
                {
                    Category = ShortcutCategory.General,
                    CategoryText = "General",
                    Keys = string.Join('+', item.Keys),
                    Name = item.ActionName,
                };
                general.Children.Add(sc);
            }

            if (item.ActionName != "some-cat")
            {
                var sc = new ShortcutItem
                {
                    Category = ShortcutCategory.SubtitleGridAndTextBox,
                    CategoryText = "Subtitle grid and text box",
                    Keys = string.Join('+', item.Keys),
                    Name = item.ActionName,
                };
                gridAndTextBox.Children.Add(sc);
            }

            if (item.ActionName != "some-cat")
            {
                var sc = new ShortcutItem
                {
                    Category = ShortcutCategory.SubtitleGrid,
                    CategoryText = "Subtitle grid",
                    Keys = string.Join('+', item.Keys),
                    Name = item.ActionName,
                };
                grid.Children.Add(sc);
            }


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