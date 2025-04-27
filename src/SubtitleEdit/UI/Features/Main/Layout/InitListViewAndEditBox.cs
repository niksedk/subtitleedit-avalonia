using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static partial class InitLayout
{
    public class InitListViewAndEditBox
    {
        public static Grid MakeLayoutListViewAndEditBox(MainView mainPage, MainViewModel vm)
        {
            mainPage.DataContext = vm;

            // Create layout
            var mainGrid = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*,Auto")
            };

            // Header
            var headerText = new TextBlock
            {
                Text = "Subtitle Editor",
                FontSize = 24,
                FontWeight = FontWeight.Bold,
                Margin = new Thickness(10)
            };
            Grid.SetRow(headerText, 0);
            mainGrid.Children.Add(headerText);

            // DataGrid for subtitles
            vm.SubtitleGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserResizeColumns = true,
                IsReadOnly = false,
                SelectionMode = DataGridSelectionMode.Extended,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(10)
            };

            // Columns
            vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "#",
                Binding = new Binding("Number"),
                Width = new DataGridLength(50)
            });
            vm.SubtitleGrid.Columns.Add(new DataGridCheckBoxColumn
            {
                Header = "Visible",
                Binding = new Binding("IsVisible"),
                Width = new DataGridLength(70)
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
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });

            // Bind data
            vm.SubtitleGrid.ItemsSource = vm.Subtitles;
            vm.SubtitleGrid.SelectionChanged += vm.SubtitleGrid_SelectionChanged;

            Grid.SetRow(vm.SubtitleGrid, 1);
            mainGrid.Children.Add(vm.SubtitleGrid);

            // Text edit area
            var editPanel = new StackPanel
            {
                Margin = new Thickness(10),
                Spacing = 5
            };

            var editLabel = new TextBlock
            {
                Text = "Edit Selected Subtitle:",
                FontWeight = FontWeight.Bold
            };
            editPanel.Children.Add(editLabel);

            vm.EditTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = 80
            };
            vm.EditTextBox.Bind(TextBox.TextProperty, new Binding("EditText") { Mode = BindingMode.TwoWay });
            vm.EditTextBox.Bind(InputElement.IsEnabledProperty, new Binding("IsSubtitleSelected"));
            //TODO: vm.EditTextBox.KeyDown += EditTextBox_KeyDown;

            editPanel.Children.Add(vm.EditTextBox);

            // Add save button
            var saveButton = new Button
            {
                Content = "Save Changes",
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 5, 0, 0)
            };
            saveButton.Bind(Button.CommandProperty, new Binding("SaveChangesCommand"));
            saveButton.Bind(InputElement.IsEnabledProperty, new Binding("IsSubtitleSelected"));
            editPanel.Children.Add(saveButton);

            Grid.SetRow(editPanel, 2);
            mainGrid.Children.Add(editPanel);

            return mainGrid;
        }
    }
}