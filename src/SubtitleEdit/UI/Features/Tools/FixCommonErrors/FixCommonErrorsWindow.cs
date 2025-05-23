using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main;
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

        var labelStep1 = new Label
        {
            Content = "Fix common errors, step 1",
            VerticalAlignment = VerticalAlignment.Center,
        };
        labelStep1.Bind(IsVisibleProperty, new Binding(nameof(vm.Step1IsVisible)));

        var labelStep2 = new Label
        {
            Content = "Fix common errors, step 2",
            VerticalAlignment = VerticalAlignment.Center,
        };
        labelStep2.Bind(IsVisibleProperty, new Binding(nameof(vm.Step2IsVisible)));

        var textBoxSearch = UiUtil.MakeTextBox(250, vm, nameof(vm.SearchText)).WithMarginRight(25);
        textBoxSearch.Watermark = "Search rules...";
        textBoxSearch.TextChanged += vm.TextBoxSearch_TextChanged;
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

        var rulesGrid = new DataGrid
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
                    Binding = new Binding(nameof(FixRuleDisplayItem.IsSelected)),
                },
                new DataGridTextColumn
                {
                    Header = "Name",
                    Binding = new Binding(nameof(FixRuleDisplayItem.Name)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Example",
                    Binding = new Binding(nameof(FixRuleDisplayItem.Example)),
                    IsReadOnly = true,
                },
            },
        };
        rulesGrid.Bind(IsVisibleProperty, new Binding(nameof(vm.Step1IsVisible)));

        var step2Grid = MakeStep2Grid();
        step2Grid.Bind(IsVisibleProperty, new Binding(nameof(_vm.Step2IsVisible)));

        var buttonPanelRules = UiUtil.MakeButtonBar(
            UiUtil.MakeButton("Select all", vm.RulesSelectAllCommand),
            UiUtil.MakeButton("Inverse selection", vm.RulesInverseSelectedCommand),
            UiUtil.MakeTextBlock("Profile").WithMarginLeft(25).WithMarginRight(10),
            UiUtil.MakeComboBox(vm.Profiles, vm, nameof(vm.SelectedProfile)),
            UiUtil.MakeButton("...", vm.ShowProfileCommand).Compact()
        );
        buttonPanelRules.Bind(IsVisibleProperty, new Binding(nameof(vm.Step1IsVisible)));


        var buttonToApplyFixes = UiUtil.MakeButton("To apply fixes", vm.ToApplyFixesCommand)
            .WithIconRight("fa-solid fa-arrow-right")
            .BindVisible(vm, nameof(vm.Step1IsVisible));

        var buttonBackToFixList = UiUtil.MakeButton("Back to fix list", vm.BackToFixListCommand)
            .WithIconLeft("fa-solid fa-arrow-left")
            .BindVisible(vm, nameof(vm.Step2IsVisible));

        var buttonApplyFixes = UiUtil.MakeButton("Apply fixes", vm.DoApplyFixesCommand)
            .BindVisible(vm, nameof(vm.Step2IsVisible));


        var buttonPanelRight = UiUtil.MakeButtonBar(
            buttonBackToFixList,
            buttonToApplyFixes,
            buttonApplyFixes,
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

        grid.Children.Add(labelStep1);
        Grid.SetRow(labelStep1, 0);
        Grid.SetColumn(labelStep1, 0);
        grid.Children.Add(labelStep2);
        Grid.SetRow(labelStep2, 0);
        Grid.SetColumn(labelStep2, 0);

        grid.Children.Add(panelTopRight);
        Grid.SetRow(panelTopRight, 0);
        Grid.SetColumn(panelTopRight, 0);
        Grid.SetColumnSpan(panelTopRight, 2);

        grid.Children.Add(rulesGrid);
        Grid.SetRow(rulesGrid, 1);
        Grid.SetColumn(rulesGrid, 0);
        Grid.SetColumnSpan(rulesGrid, 2);

        grid.Children.Add(step2Grid);
        Grid.SetRow(step2Grid, 1);
        Grid.SetColumn(step2Grid, 0);
        Grid.SetColumnSpan(step2Grid, 2);

        grid.Children.Add(buttonPanelRules);
        Grid.SetRow(buttonPanelRules, 2);
        Grid.SetColumn(buttonPanelRules, 0);


        grid.Children.Add(buttonPanelRight);
        Grid.SetRow(buttonPanelRight, 2);
        Grid.SetColumn(buttonPanelRight, 1);


        Content = grid;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    private Grid MakeStep2Grid()
    {
        // top
        var gridFixes = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        var dataGridFixes = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = _vm,
            ItemsSource = _vm.Fixes,
            Columns =
            {
                new DataGridCheckBoxColumn
                {
                    Header = "Apply",
                    Binding = new Binding(nameof(FixDisplayItem.IsSelected)),
                },
                new DataGridTextColumn
                {
                    Header = "#",
                    Binding = new Binding(nameof(FixDisplayItem.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Action",
                    Binding = new Binding(nameof(FixDisplayItem.Action)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Before",
                    Binding = new Binding(nameof(FixDisplayItem.Before)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "After",
                    Binding = new Binding(nameof(FixDisplayItem.After)),
                    IsReadOnly = true,
                },
            },
        };
        dataGridFixes.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(_vm.SelectedFix)));
        dataGridFixes.SelectionChanged += DataGridFixes_SelectionChanged;

        var buttonBarFixes = UiUtil.MakeButtonBar(
            UiUtil.MakeButton("Select all", _vm.FixesSelectAllCommand),
            UiUtil.MakeButton("Inverse selection", _vm.FixesInverseSelectedCommand),
            UiUtil.MakeButton("Refresh fixes", _vm.FixesInverseSelectedCommand),
            UiUtil.MakeButton("Apply selected fixes", _vm.FixesInverseSelectedCommand)
        );

        gridFixes.Children.Add(dataGridFixes);
        Grid.SetRow(dataGridFixes, 0);
        Grid.SetColumn(dataGridFixes, 0);

        gridFixes.Children.Add(buttonBarFixes);
        Grid.SetRow(buttonBarFixes, 1);
        Grid.SetColumn(buttonBarFixes, 0);

        var borderFixes = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
            Child = gridFixes,
        };

        // bottom
        var gridSubtitles = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var dataGridSubtitles = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = _vm,
            ItemsSource = _vm.Paragraphs,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = "#",
                    Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Show",
                    Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Hide",
                    Binding = new Binding(nameof(SubtitleLineViewModel.EndTime)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Duration",
                    Binding = new Binding(nameof(SubtitleLineViewModel.Duration)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Text",
                    Binding = new Binding(nameof(SubtitleLineViewModel.Text)),
                    IsReadOnly = true,
                },
            },
        };
        dataGridSubtitles.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(_vm.SelectedParagraph)));

        var gridCurrentSubtbtitle = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };



        gridSubtitles.Children.Add(dataGridSubtitles);
        Grid.SetRow(dataGridSubtitles, 0);
        Grid.SetColumn(dataGridSubtitles, 0);

        gridSubtitles.Children.Add(gridCurrentSubtbtitle);
        Grid.SetRow(gridCurrentSubtbtitle, 1);
        Grid.SetColumn(gridCurrentSubtbtitle, 0);

        var borderSubtitles = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
            Child = gridSubtitles,
        };  

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        grid.Children.Add(borderFixes);
        Grid.SetRow(borderFixes, 0);
        Grid.SetColumn(borderFixes, 0);

        grid.Children.Add(borderSubtitles);
        Grid.SetRow(borderSubtitles, 1);
        Grid.SetColumn(borderSubtitles, 0);

        return grid;
    }

    private void DataGridFixes_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
