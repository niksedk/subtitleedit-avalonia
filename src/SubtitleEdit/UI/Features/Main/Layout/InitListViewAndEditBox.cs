using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitListViewAndEditBox
{
    private static bool _isLeftMouseDown;
    
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
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Extended,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(0),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };

        vm.SubtitleGrid2 = new TreeDataGrid
        {
            Height = double.NaN, // auto size inside scroll viewer
            Margin = new Thickness(2),
            Source = vm.TranslateRowSource,
            CanUserSortColumns = false,
            ContextFlyout = contextMenu,
        };

        // Columns
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "#",
            Binding = new Binding("Number"),
            Width = new DataGridLength(50),
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Show",
            Binding = new Binding("StartTime"),
            Width = new DataGridLength(120),
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Hide",
            Binding = new Binding("EndTime"),
            Width = new DataGridLength(120),
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Duration",
            Binding = new Binding("Duration"),
            Width = new DataGridLength(120),
            IsReadOnly = true,
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Text",
            Binding = new Binding("Text"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star), // Stretch text column
        });

        // Bind data
        vm.SubtitleGrid.ItemsSource = vm.Subtitles;
        vm.SubtitleGrid.SelectionChanged += vm.SubtitleGrid_SelectionChanged;

        Grid.SetRow(vm.SubtitleGrid, 0);
        mainGrid.Children.Add(vm.SubtitleGrid);
        

        // Create a Flyout for the DataGrid
        var flyout = new MenuFlyout();
        
        flyout.Opening += vm.SubtitleContextOpening;
        
        // Add menu items with commands
        var deleteMenuItem = new MenuItem { Header = "Delete" };
        deleteMenuItem.Click += (s, e) => vm.DeleteSelectedItems();
        
        var insertAfterMenuItem = new MenuItem { Header = "Insert after" };
        insertAfterMenuItem.Click += (s, e) => vm.InsertAfterSelectedItem();
        
        var italicMenuItem = new MenuItem { Header = "Italic" };
        italicMenuItem.Click += (s, e) => vm.ToggleItalic();
        
        // Add items to flyout menu
        flyout.Items.Add(deleteMenuItem);
        flyout.Items.Add(insertAfterMenuItem);
        flyout.Items.Add(italicMenuItem);
        
        // Set the ContextFlyout property
        vm.SubtitleGrid.ContextFlyout = flyout;
        vm.SubtitleGrid.AddHandler(InputElement.PointerPressedEvent, vm.SubtitleGrid_PointerPressed, RoutingStrategies.Tunnel);
        vm.SubtitleGrid.AddHandler(InputElement.PointerReleasedEvent, vm.SubtitleGrid_PointerReleased, RoutingStrategies.Tunnel);

        // Edit area - restructured with time controls on left, multiline text on right
        var editGrid = new Grid
        {
            Margin = new Thickness(10),
            ColumnDefinitions = new ColumnDefinitions("Auto, *"), // Two columns: left for time controls, right for text
            RowDefinitions = new RowDefinitions("Auto")
        };

        // Left panel for time controls
        var timeControlsPanel = new StackPanel
        {
            Spacing = 8,
            Width = 200,
            Margin = new Thickness(0, 0, 10, 0)
        };

        // Start Time controls
        var startTimePanel = new StackPanel
        {
            Spacing = 4,
            Orientation = Orientation.Vertical
        };

        var startTimeLabel = new TextBlock
        {
            Text = "Show:",
            FontWeight = FontWeight.Bold
        };
        startTimePanel.Children.Add(startTimeLabel);

        //var startTimeBox = new TextBox
        //{
        //    Watermark = "hh:mm:ss.fff",
        //    Height = 32,
        //    [!TextBox.TextProperty] = new Binding("SelectedSubtitle.StartTime")
        //    {
        //        Mode = BindingMode.TwoWay,
        //        StringFormat = "c" // "c" = constant ("00:00:00.000")
        //    }
        //};
        //startTimePanel.Children.Add(startTimeBox);

        var timeCodeUpDown = new TimeCodeUpDown
        {
            DataContext = vm,
            [!TimeCodeUpDown.ValueProperty] = new Binding("SelectedSubtitle.StartTime")
            {
                Mode = BindingMode.TwoWay,
                //StringFormat = "c" // "c" = constant ("00:00:00.000")
            }
        };
        startTimePanel.Children.Add(timeCodeUpDown);


        timeControlsPanel.Children.Add(startTimePanel);

        //// End Time controls
        //var endTimePanel = new StackPanel
        //{
        //    Spacing = 4,
        //    Orientation = Orientation.Vertical
        //};

        //var endTimeLabel = new TextBlock
        //{
        //    Text = "End Time:",
        //    FontWeight = FontWeight.Bold
        //};
        //endTimePanel.Children.Add(endTimeLabel);

        //var endTimeBox = new TextBox
        //{
        //    Watermark = "hh:mm:ss.fff",
        //    Height = 32,
        //    [!TextBox.TextProperty] = new Binding("SelectedSubtitle.EndTime")
        //    {
        //        Mode = BindingMode.TwoWay,
        //        StringFormat = "c" // "c" = constant ("00:00:00.000")
        //    }
        //};
        //endTimePanel.Children.Add(endTimeBox);
        //timeControlsPanel.Children.Add(endTimePanel);

        // Duration display
        var durationPanel = new StackPanel
        {
            Spacing = 4,
            Orientation = Orientation.Vertical
        };

        var durationLabel = new TextBlock
        {
            Text = "Duration:",
            FontWeight = FontWeight.Bold
        };
        durationPanel.Children.Add(durationLabel);

        var durationBox = new TextBox
        {
            IsReadOnly = true,
            Height = 32,
            Background = new SolidColorBrush(Colors.LightGray),
            [!TextBox.TextProperty] = new Binding("SelectedSubtitle.Duration")
            {
                Mode = BindingMode.TwoWay,
                StringFormat = "c"
            }
        };
        durationPanel.Children.Add(durationBox);
        timeControlsPanel.Children.Add(durationPanel);

        Grid.SetColumn(timeControlsPanel, 0);
        editGrid.Children.Add(timeControlsPanel);

        // Right panel for text editing
        var textEditPanel = new StackPanel
        {
            Spacing = 4,
            Orientation = Orientation.Vertical
        };

        var textLabel = new TextBlock
        {
            Text = "Text:",
            FontWeight = FontWeight.Bold
        };
        textEditPanel.Children.Add(textLabel);

        var textBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 100,
            [!TextBox.TextProperty] = new Binding("SelectedSubtitle.Text")
            {
                Mode = BindingMode.TwoWay
            }
        };
        textEditPanel.Children.Add(textBox);

        Grid.SetColumn(textEditPanel, 1);
        editGrid.Children.Add(textEditPanel);

        Grid.SetRow(editGrid, 1);
        mainGrid.Children.Add(editGrid);

        return mainGrid;
    }
}