using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class BatchConvertWindow : Window
{
    private BatchConvertViewModel _vm;
    
    public BatchConvertWindow(BatchConvertViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Batch convert";
        Width = 1024;
        Height = 740;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var fileView = MakeFileView(vm);
        var functionsListView = MakeFunctionsListView(vm);
        var functionView = MakeFunctionView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonPanel = UiUtil.MakeButtonBar(
            buttonOk,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );
        
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(fileView, 0, 0, 1, 2);
        grid.Add(functionsListView, 1, 0);
        grid.Add(functionView, 1, 1);
        grid.Add(buttonPanel, 2, 0, 1, 2);

        Content = grid;
        
        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private Border MakeFileView(BatchConvertViewModel vm)
    {
        var grid = new Grid
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
            ItemsSource = vm.BatchItems,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FileName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BatchConvertItem.FileName)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Size,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BatchConvertItem.Size)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Format,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BatchConvertItem.Format)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Status,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BatchConvertItem.Status)),
                    IsReadOnly = true,
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedBatchItem)) { Source = vm });

        var panelFileControls = new StackPanel
            {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
            {
                UiUtil.MakeButton("Add", vm.AddFilesCommand),
                UiUtil.MakeButton("Remove", vm.RemoveSelectedFilesCommand),
                UiUtil.MakeButton("Clear", vm.ClearAllFilesCommand),
                UiUtil.MakeSeparatorForHorizontal(),
                UiUtil.MakeLabel("Target format"),
                UiUtil.MakeComboBox(vm.TargetFormats, vm, nameof(vm.SelectedTargetFormat)),
                UiUtil.MakeLabel("Target encoding").WithLarginLeft(5),
                UiUtil.MakeComboBox(vm.TargetEncodings, vm, nameof(vm.SelectedTargetEncoding)),
                UiUtil.MakeSeparatorForHorizontal(),
                UiUtil.MakeButton("Output properties", vm.ShowOutputPropertiesCommand),
                UiUtil.MakeLabel(new Binding(nameof(vm.OutputPropertiesText))),
            }
        };

        grid.Add(dataGrid, 0, 0);
        grid.Add(panelFileControls, 1, 0);

        var border = UiUtil.MakeBorderForControl(grid);
        return border;  
    }

    private Border MakeFunctionsListView(BatchConvertViewModel vm)
    {
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            HeadersVisibility = DataGridHeadersVisibility.None,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = 300,
            DataContext = vm,
            ItemsSource = vm.BatchFunctions,
            Columns =
            {
                new DataGridTextColumn
                {
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BatchConvertFunction.IsSelected)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BatchConvertFunction.Name)),
                    IsReadOnly = true,
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedBatchFunction)) { Source = vm });
        dataGrid.SelectionChanged += vm.SelectedFunctionChanged;

        return UiUtil.MakeBorderForControl(dataGrid);
    }

    private Border MakeFunctionView(BatchConvertViewModel vm)
    {
        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
            Child = UiUtil.MakeLabel("function options"),
        };
        vm.FunctionContainer = border;

        return border;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
