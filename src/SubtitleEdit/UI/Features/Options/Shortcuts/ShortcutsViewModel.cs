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
        var sc1 = new ShortcutItem
        {
            Keys = "Ctrl+C",
            Age = 4,
            Category = ShortcutCategory.General,
        };
        Shortcuts.Add(sc1);
        
        ShortcutsSource = new HierarchicalTreeDataGridSource<ShortcutItem>(_shortcuts)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<ShortcutItem>(
                    new TextColumn<ShortcutItem, string>("First Name", x => x.Keys),
                    x => x.Children),
                new TextColumn<ShortcutItem, string>("Last Name", x => x.Keys),
                new TextColumn<ShortcutItem, int>("Age", x => x.Age),
            },
        };
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