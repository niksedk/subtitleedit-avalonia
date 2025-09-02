using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa;

public class AssaStylePickerWindow : Window
{
    public AssaStylePickerWindow(AssaStylePickerViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        CanResize = true;
        Width = 1200;
        Height = 850;
        MinWidth = 1100;
        MinHeight = 600;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var labelFontsAndImages = UiUtil.MakeLabel(Se.Language.Assa.FontsAndGraphics);

        var buttonImport = UiUtil.MakeButton(Se.Language.General.Import, vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonImport, buttonCancel);

        grid.Add(labelFontsAndImages, 0);
        grid.Add(MakeDataGrid(vm), 1);
        grid.Add(panelButtons, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonImport.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Border MakeDataGrid(AssaStylePickerViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
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
            DataContext = vm,
            ItemsSource = vm.Styles,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Name,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.Name)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FontName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.FontName)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FontSize,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.FontSize)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedStyle)) { Source = vm });
        dataGrid.SelectionChanged += vm.DataGridSelectionChanged;

        return UiUtil.MakeBorderForControl(grid);
    }
}
