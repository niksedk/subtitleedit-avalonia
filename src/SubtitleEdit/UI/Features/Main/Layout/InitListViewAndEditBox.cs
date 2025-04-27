using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitListViewAndEditBox
{
    public static Grid MakeLayoutListViewAndEditBox(MainView mainPage, MainViewModel vm)
    {
        mainPage.DataContext = vm;

        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto") // First row fills, second is auto-sized
        };

        // DataGrid for subtitles
        vm.SubtitleGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserResizeColumns = true,
            IsReadOnly = false,
            SelectionMode = DataGridSelectionMode.Extended,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(0),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };

        // Columns
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "#",
            Binding = new Binding("Number"),
            Width = new DataGridLength(50)
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Start Time",
            Binding = new Binding("StartTime"),
            Width = new DataGridLength(120)
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "End Time",
            Binding = new Binding("EndTime"),
            Width = new DataGridLength(120)
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Duration",
            Binding = new Binding("Duration"),
            Width = new DataGridLength(120),
            IsReadOnly = true
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Text",
            Binding = new Binding("Text"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star) // Stretch text column
        });

        // Bind data
        vm.SubtitleGrid.ItemsSource = vm.Subtitles;
        vm.SubtitleGrid.SelectionChanged += vm.SubtitleGrid_SelectionChanged;

        Grid.SetRow(vm.SubtitleGrid, 0);
        mainGrid.Children.Add(vm.SubtitleGrid);

        // Edit area - small bottom grid
       var editGrid = new Grid
        {
            Margin = new Thickness(10),
            ColumnDefinitions = new ColumnDefinitions("Auto, *, Auto, *, Auto, *"),
            RowDefinitions = new RowDefinitions("Auto"),
            //Height = 200,

        };

        // Start Time label
        var startTimeLabel = new TextBlock
        {
            Text = "Start Time:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(startTimeLabel, 0);
        editGrid.Children.Add(startTimeLabel);

        // Start Time edit
        var startTimeBox = new TextBox
        {
            Watermark = "hh:mm:ss.fff",
            [!TextBox.TextProperty] = new Binding("SelectedSubtitle.StartTime")
            {
                Mode = BindingMode.TwoWay,
                StringFormat = "c" // "c" = constant ("00:00:00.000")
            }
        };
        Grid.SetColumn(startTimeBox, 1);
        editGrid.Children.Add(startTimeBox);

        // Duration label
        var durationLabel = new TextBlock
        {
            Text = "Duration:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(durationLabel, 2);
        editGrid.Children.Add(durationLabel);

        // Duration edit (readonly for now)
        var durationBox = new TextBox
        {
            IsReadOnly = true,
            [!TextBox.TextProperty] = new Binding("SelectedSubtitle.Duration")
            {
                Mode = BindingMode.OneWay,
                StringFormat = "c"
            }
        };
        Grid.SetColumn(durationBox, 3);
        editGrid.Children.Add(durationBox);

        // Text label
        var textLabel = new TextBlock
        {
            Text = "Text:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(textLabel, 4);
        editGrid.Children.Add(textLabel);

        // Text edit
        var textBox = new TextBox
        {
            AcceptsReturn = true,
            MinWidth = 200,
            [!TextBox.TextProperty] = new Binding("SelectedSubtitle.Text")
            {
                Mode = BindingMode.TwoWay
            }
        };
        Grid.SetColumn(textBox, 5);
        editGrid.Children.Add(textBox);

        Grid.SetRow(editGrid, 1);
        mainGrid.Children.Add(editGrid);

        return mainGrid;
    }
}
