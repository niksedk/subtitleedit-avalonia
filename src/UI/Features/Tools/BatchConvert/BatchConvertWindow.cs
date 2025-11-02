using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class BatchConvertWindow : Window
{
    private readonly BatchConvertViewModel _vm;

    public BatchConvertWindow(BatchConvertViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.BatchConvert.Title;
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

        var labelFunctionsSelected = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.ActionsSelected))
            .WithAlignmentTop();

        var buttonConvert = UiUtil.MakeButton(Se.Language.General.Convert, vm.ConvertCommand);
        var buttonStatistics = UiUtil.MakeButton(Se.Language.File.Statistics.Title, vm.StatisticsCommand);
        var buttonDone = UiUtil.MakeButtonDone(vm.DoneCommand);
        var buttonPanel = UiUtil.MakeButtonBar(
            buttonConvert,
            buttonStatistics,
            buttonDone
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
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(fileView, 0, 0, 1, 2);
        grid.Add(functionsListView, 1, 0);
        grid.Add(functionView, 1, 1);
        grid.Add(labelFunctionsSelected, 2, 0);
        grid.Add(buttonPanel, 2, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonDone.Focus(); }; // hack to make OnKeyDown work
    }

    private static Border MakeFileView(BatchConvertViewModel vm)
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
                    Binding = new Binding(nameof(BatchConvertItem.Size)) { Converter = new FileSizeConverter(), Mode = BindingMode.OneWay },
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
                UiUtil.MakeButton(vm.AddFilesCommand, IconNames.Plus, Se.Language.General.Add).WithMarginLeft(10),
                UiUtil.MakeButton(vm.RemoveSelectedFilesCommand, IconNames.Trash, Se.Language.General.Remove).WithMarginLeft(5),
                UiUtil.MakeButton(vm.ClearAllFilesCommand, IconNames.Close, Se.Language.General.Clear).WithMarginLeft(5),
                UiUtil.MakeLabel(Se.Language.General.TargetFormat).WithMarginLeft(15),
                UiUtil.MakeComboBox(vm.TargetFormats, vm, nameof(vm.SelectedTargetFormat)),
                UiUtil.MakeButton(Se.Language.General.OutputProperties, vm.ShowOutputPropertiesCommand).WithMarginLeft(18),
                MakeOutputPropertiesGrid(vm),
            }
        };

        var labelBatchItemsInfo = UiUtil.MakeLabel()
            .WithBindText(vm, nameof(vm.BatchItemsInfo))
            .WithMarginTop(5)
            .WithMarginRight(5)
            .WithAlignmentTop()
            .WithAlignmentRight();

        // hack to make drag and drop work on the DataGrid - also on empty rows
        var dropHost = new Border
        {
            Background = Brushes.Transparent,
            Child = dataGrid,
        };
        DragDrop.SetAllowDrop(dropHost, true);
        dropHost.AddHandler(DragDrop.DragOverEvent, vm.FileGridOnDragOver, RoutingStrategies.Bubble);
        dropHost.AddHandler(DragDrop.DropEvent, vm.FileGridOnDrop, RoutingStrategies.Bubble);

        grid.Add(dropHost, 0, 0);
        grid.Add(labelBatchItemsInfo, 0);
        grid.Add(panelFileControls, 1, 0);

        var border = UiUtil.MakeBorderForControlNoPadding(grid);
        return border;
    }

    private static Grid MakeOutputPropertiesGrid(BatchConvertViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 0,
            RowSpacing = 0,
        };

        var labelOutputSourceFolder = UiUtil.MakeLabel(new Binding(nameof(vm.OutputFolderLabel)));
        var linkLabelOutputFolder = UiUtil.MakeLink(string.Empty, vm.OpenOutputFolderCommand, vm, nameof(vm.OutputFolderLinkLabel))
                            .WithAlignmentLeft();
        var labelOutputEncoding = UiUtil.MakeLabel(new Binding(nameof(vm.OutputEncodingLabel))).WithAlignmentTop();

        grid.Add(labelOutputSourceFolder, 0);
        grid.Add(linkLabelOutputFolder, 0);
        grid.Add(labelOutputEncoding, 1);

        return grid;
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
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Enabled,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<BatchConvertFunction>((item, _) =>
                    new Border
                    {
                        Background = Brushes.Transparent, // Prevents highlighting
                        Padding = new Thickness(0),
                        Child = MakeSelectedCheckBox(vm)
                    }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                    Binding = new Binding(nameof(BatchConvertFunction.Name)),
                    IsReadOnly = true,
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedBatchFunction)) { Source = vm });
        dataGrid.SelectionChanged += (_, _) => vm.SelectedFunctionChanged();

        return UiUtil.MakeBorderForControl(dataGrid);
    }

    private static CheckBox MakeSelectedCheckBox(BatchConvertViewModel vm)
    {
        var checkBox = new CheckBox
        {
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(BatchConvertFunction.IsSelected)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5, 0, 0, 0),
        };

        checkBox.IsCheckedChanged += (_, _) => vm.SelectedFunctionChanged();

        return checkBox;
    }

    private static Border MakeFunctionView(BatchConvertViewModel vm)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Padding = new Thickness(10, 15, 10, 10),
            Width = double.NaN,
            Height = 300,
        };

        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderBrush(),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
        };
        vm.FunctionContainer = scrollViewer;

        return UiUtil.MakeBorderForControl(scrollViewer);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
