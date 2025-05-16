using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

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


        var subtitleContextMenu = new MenuFlyout
        {
            Items =
            {
                new MenuItem
                {
                    Header = "Delete",
                    Command = vm.DeleteSelectedLinesCommand,
                },
                new MenuItem
                {
                    Header = "Insert after",
                    Command = vm.InsertLineAfterCommand,
                },
                new MenuItem
                {
                    Header = "Toggle italic",
                    Command = vm.ToggleLinesItalicCommand,
                },
            },
        };

        vm.SubtitleGrid = new DataGrid
        {
            Height = double.NaN, // auto size inside scroll viewer
            Margin = new Thickness(2),
            ItemsSource = vm.Subtitles, // Use ItemsSource instead of Items
            CanUserSortColumns = false,
            //   ContextFlyout = subtitleContextMenu, // Create new ContextMenu
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Extended,
        };


        // cellTheme.Setters.Add(new Setter(DataGridCell.HorizontalAlignmentProperty, VerticalAlignment.Stretch));
        //cellTheme.Setters.Add(new Setter(Control.MarginProperty, new Thickness(2)));
        //vm.SubtitleGrid.CellTheme = cellTheme;

        // Create a theme for DataGridCell + Apply the cell theme to the DataGrid (hide cell selection rectangle)
        //var cellTheme = new ControlTheme(typeof(DataGridCell));
        //cellTheme.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(0)));
        //      cellTheme.Setters.Add(new Setter(Border.BorderBrushProperty, Brushes.Transparent));
        //cellTheme.Setters.Add(new Setter(InputElement.FocusableProperty, false));
//        vm.SubtitleGrid.CellTheme = cellTheme;


        //     var cellTheme = vm.SubtitleGrid.CellTheme; // new ControlTheme(typeof(DataGridCell));
        //      cellTheme.Setters.Add(new Setter(Border.BorderBrushProperty, Brushes.Tomato));
        //cellTheme.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(0)));
        // cellTheme.Setters.Add(new Setter(InputElement.FocusableProperty, false));
        //        vm.SubtitleGrid.CellTheme = cellTheme;


        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

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
            Binding = new Binding("StartTime") { Converter = fullTimeConverter },
            Width = new DataGridLength(120),
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "End Time",
            Binding = new Binding("EndTime") { Converter = fullTimeConverter },
            Width = new DataGridLength(120)
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Duration",
            Binding = new Binding("Duration") { Converter = shortTimeConverter },
            Width = new DataGridLength(120),
            IsReadOnly = true
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Text",
            Binding = new Binding("Text"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star) // Stretch text column
        });


        vm.SubtitleGrid.DataContext = vm.Subtitles;
        vm.SubtitleGrid.SelectionChanged += vm.SubtitleGrid_SelectionChanged;


        // Set up two-way binding for SelectedItem
        vm.SubtitleGrid[!DataGrid.SelectedItemProperty] = new Binding("SelectedSubtitle")
        {
            Mode = BindingMode.TwoWay,
            Source = vm
        };

// Set up two-way binding for SelectedIndex
        vm.SubtitleGrid[!DataGrid.SelectedIndexProperty] = new Binding("SelectedSubtitleIndex")
        {
            Mode = BindingMode.TwoWay,
            Source = vm
        };


        //if (vm.SubtitlesSource is FlatTreeDataGridSource<SubtitleLineViewModel> source)
        //{
        //    source.RowSelection!.SelectionChanged += (sender, e) =>
        //    {
        //        vm.SubtitleGrid_SelectionChanged(source.RowSelection.SelectedItems);
        //    };
        //}

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
        vm.SubtitleGrid.AddHandler(InputElement.PointerPressedEvent, vm.SubtitleGrid_PointerPressed,
            RoutingStrategies.Tunnel);
        vm.SubtitleGrid.AddHandler(InputElement.PointerReleasedEvent, vm.SubtitleGrid_PointerReleased,
            RoutingStrategies.Tunnel);

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
            Width = 180,
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
            Text = "Show",
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
            Text = "Duration",
            FontWeight = FontWeight.Bold
        };
        durationPanel.Children.Add(durationLabel);

        var durationUpDown = new NumericUpDown
        {
            DataContext = vm,
            [!NumericUpDown.ValueProperty] = new Binding("SelectedSubtitle.Duration")
            {
                Mode = BindingMode.TwoWay,
                Converter = TimeSpanToSecondsConverter.Instance,
            },
            // Add a binding for the background property
            [!NumericUpDown.BackgroundProperty] = new Binding("SelectedSubtitle.Duration")
            {
                Converter = DurationToBackgroundConverter.Instance,
            },
        };

        durationPanel.Children.Add(durationUpDown);
        timeControlsPanel.Children.Add(durationPanel);

        Grid.SetColumn(timeControlsPanel, 0);
        editGrid.Children.Add(timeControlsPanel);

        // Right panel for text editing
        var textEditGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            RowDefinitions = new RowDefinitions("Auto,*,Auto")
        };

        //var textEditPanel = new StackPanel
        //{
        //    Spacing = 4,
        //    Orientation = Orientation.Vertical
        //};

        var textLabel = new TextBlock
        {
            Text = "Text",
            FontWeight = FontWeight.Bold,
        };
        textEditGrid.Children.Add(textLabel);

        var textCharsSecLabel = new TextBlock
        {
            Text = "Chars/sec: -",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            FontSize = 12,
            Padding = new Thickness(0, 0, 3, 0),
        };
        textCharsSecLabel.Bind(TextBlock.TextProperty,
            new Binding(nameof(vm.EditTextCharactersPerSecond))
            {
                Mode = BindingMode.OneWay
            });
        textEditGrid.Children.Add(textCharsSecLabel);

        var textBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 92,
            Height = 92,
            [!TextBox.TextProperty] = new Binding("SelectedSubtitle.Text")
            {
                Mode = BindingMode.TwoWay
            },
            FontSize = Se.Settings.Appearance.SubtitleTextBoxFontSize,
            FontWeight = Se.Settings.Appearance.SubtitleTextBoxFontBold ?  FontWeight.Bold : FontWeight.Normal,
        };
        textEditGrid.Children.Add(textBox);
        textBox.TextChanged += vm.SubtitleTextChanged;
        Grid.SetRow(textBox, 1);


        var textTotalLengthLabel = new TextBlock
        {
            Text = "Total length: -",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            FontSize = 12,
            Padding = new Thickness(0, 2, 3, 0),
        };
        textTotalLengthLabel.Bind(TextBlock.TextProperty,
            new Binding(nameof(vm.EditTextTotalLength))
            {
                Mode = BindingMode.OneWay
            });
        textEditGrid.Children.Add(textTotalLengthLabel);
        Grid.SetRow(textTotalLengthLabel, 2);


        var singleLineLengthLabel = new TextBlock
        {
            Text = "Line length: -",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            FontSize = 12,
            Padding = new Thickness(3, 2, 0, 0),
        };
        singleLineLengthLabel.Bind(TextBlock.TextProperty,
            new Binding(nameof(vm.EditTextLineLengths))
            {
                Mode = BindingMode.OneWay
            });
        textEditGrid.Children.Add(singleLineLengthLabel);
        Grid.SetRow(singleLineLengthLabel, 2);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 3,
            Margin = new Thickness(3)
        };

        // Auto Break button
        var autoBreakButton = new Button();
        Projektanker.Icons.Avalonia.Attached.SetIcon(autoBreakButton, "fa-solid fa-bolt"); // Example icon
        ToolTip.SetTip(autoBreakButton, "Auto-break");
        buttonPanel.Children.Add(autoBreakButton);
        autoBreakButton.Command = vm.AutoBreakCommand;

        // Unbreak button
        var unbreakButton = new Button();
        Projektanker.Icons.Avalonia.Attached.SetIcon(unbreakButton, "fa-solid fa-link-slash"); // Example icon
        ToolTip.SetTip(unbreakButton, "Unbreak");
        buttonPanel.Children.Add(unbreakButton);
        unbreakButton.Command = vm.UnbreakCommand;

        textEditGrid.Children.Add(buttonPanel);
        Grid.SetRow(buttonPanel, 1);
        Grid.SetColumn(buttonPanel, 1);

        Grid.SetColumn(textEditGrid, 1);
        editGrid.Children.Add(textEditGrid);

        Grid.SetRow(editGrid, 1);
        mainGrid.Children.Add(editGrid);

        return mainGrid;
    }
}