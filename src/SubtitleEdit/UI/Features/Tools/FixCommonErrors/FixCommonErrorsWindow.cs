using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public class FixCommonErrorsWindow : Window
{
    private FixCommonErrorsViewModel _vm;

    public FixCommonErrorsWindow(FixCommonErrorsViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Fix common errors";
        Width = 950;
        Height = 750;
        MinWidth = 800; 
        MinHeight = 600;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = "Fix common errors, step 1",
            VerticalAlignment = VerticalAlignment.Center,
        };

        var textBoxSearch = UiUtil.MakeTextBox(200, vm, nameof(vm.SearchText)).WithMarginRight(25);
        textBoxSearch.Watermark = "Search rules...";
        var panelTopRight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                textBoxSearch,
                UiUtil.MakeTextBlock("Language").WithMarginRight(5),
                UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage)),
            },
        };

        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            ItemsSource = vm.FixRules,
            Columns =
            {
                new DataGridCheckBoxColumn
                {
                    Header = "Enabled",
                    Binding = new Avalonia.Data.Binding(nameof(FixRuleDisplayItem.IsSelected)),
                },
                new DataGridTextColumn
                {
                    Header = "Name",
                    Binding = new Avalonia.Data.Binding(nameof(FixRuleDisplayItem.Name)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Example",
                    Binding = new Avalonia.Data.Binding(nameof(FixRuleDisplayItem.Example)),
                    IsReadOnly = true,
                },
            },
        };

        var buttonPanelLeft = UiUtil.MakeButtonBar(
            UiUtil.MakeButton("Select all", vm.OkCommand),
            UiUtil.MakeButton("Inverse selection", vm.CancelCommand),
            UiUtil.MakeTextBlock("Profile").WithMarginLeft(25).WithMarginRight(10),
            UiUtil.MakeComboBox(vm.Profiles, vm, nameof(vm.SelectedProfile)),
            UiUtil.MakeButton("...").Compact()
        );

        var buttonPanelRight = UiUtil.MakeButtonBar(
            UiUtil.MakeButton("Apply fixes", vm.OkCommand),
            UiUtil.MakeButton("Cancel", vm.CancelCommand)
        );

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Children.Add(label);
        Grid.SetRow(label, 0);
        Grid.SetColumn(label, 0);

        grid.Children.Add(panelTopRight);
        Grid.SetRow(panelTopRight, 0);
        Grid.SetColumn(panelTopRight, 1);

        grid.Children.Add(dataGrid);
        Grid.SetRow(dataGrid, 1);
        Grid.SetColumn(dataGrid, 0);
        Grid.SetColumnSpan(dataGrid, 2);

        grid.Children.Add(buttonPanelLeft);
        Grid.SetRow(buttonPanelLeft, 2);
        Grid.SetColumn(buttonPanelLeft, 0);


        grid.Children.Add(buttonPanelRight);
        Grid.SetRow(buttonPanelRight, 2);
        Grid.SetColumn(buttonPanelRight, 1);


        Content = grid;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
