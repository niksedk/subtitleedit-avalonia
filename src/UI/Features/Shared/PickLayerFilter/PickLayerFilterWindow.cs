using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickLayerFilter;

public class PickLayerFilterWindow : Window
{
    public PickLayerFilterWindow(PickLayerFilterViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.FilterLayersTitle;
        CanResize = true;
        Width = 800;
        Height = 700;
        MinWidth = 500;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var fontsView = MakeLayersView(vm);

        var buttonRemoveFilter = UiUtil.MakeButton(Se.Language.General.RemoveFilter, vm.RemoveFilterCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonRemoveFilter, buttonOk, buttonCancel);

        var buttonSelectAll = UiUtil.MakeButton(Se.Language.General.SelectAll, vm.SelectAllCommand);
        var buttonInverseSelection = UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.InvertSelectionCommand);
        var panelButtonTools = UiUtil.MakeHorizontalPanel(buttonSelectAll, buttonInverseSelection);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(fontsView, 0);
        grid.Add(panelButtonTools, 1);
        grid.Add(buttonPanel, 1);

        Content = grid;

        Loaded += delegate 
        { 
            buttonOk.Focus();
            vm.Loaded();
        }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private Border MakeLayersView(PickLayerFilterViewModel vm)
    {
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
            ItemsSource = vm.Layers,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Enabled,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<LayerItem>((item, _) =>
                    new Border
                    {
                        Background = Brushes.Transparent, // Prevents highlighting
                        Padding = new Thickness(4),
                        Child = new CheckBox
                        {
                            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(LayerItem.IsSelected)),
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Layer,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(LayerItem.Layer)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Usages,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(LayerItem.UsageCount)),
                    IsReadOnly = true,
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedLayer)));
        vm.LayerGrid = dataGrid;
        dataGrid.KeyDown += (s, e) => vm.LayerGridKeyDown(e);

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
