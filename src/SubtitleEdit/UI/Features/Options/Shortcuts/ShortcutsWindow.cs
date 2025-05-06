using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Nikse.SubtitleEdit.Logic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public class ShortcutsWindow : Window
{
    private TextBox _searchBox;
    private ShortcutsViewModel _vm;
    
    public ShortcutsWindow(ShortcutsViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Shortcuts";
        Width = 650;
        Height = 650;
        CanResize = true;
        
        _vm = vm;
        vm.Window = this;
        DataContext = vm;
        
        _searchBox = new TextBox
        {
            Watermark = "Search shortcuts...",
            Margin = new Thickness(10),
        };

        var contentPanel = new TreeDataGrid
        {
            Height = double.NaN, // auto size inside scroll viewer
            Margin = new Thickness(2),
            Source = vm.ShortcutsSource,
            DataContext = _vm,
            CanUserSortColumns = false,
        };

        var scrollViewer = new ScrollViewer
        {
            Content = contentPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var buttonOk = UiUtil.MakeButton("OK", vm.CommandOkCommand);
        var buttonCancel = UiUtil.MakeButton("Cancel", vm.CommandCancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*")
        };
        grid.Children.Add(_searchBox);
        Grid.SetRow(_searchBox, 0);
        Grid.SetColumn(_searchBox, 0);
        
        grid.Children.Add(scrollViewer);
        Grid.SetRow(scrollViewer, 1);
        Grid.SetColumn(scrollViewer, 0);

        grid.Children.Add(buttonPanel);
        Grid.SetRow(buttonPanel, 2);
        Grid.SetColumn(buttonPanel, 0);

        Content = grid;
    }
}

public class ShortcutItem
{
    public ShortcutCategory Category { get; set; }
    public string CategoryText { get; set; }
    public string Name { get; set; }
    public string Keys { get; set; }
    public ShortCut Shortcut { get; set; }
    public ObservableCollection<ShortcutItem> Children { get; } = new();
}

public enum ShortcutCategory
{
    General,
    SubtitleGridAndTextBox,
    Waveform,
    SubtitleGrid,
}