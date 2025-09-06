using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.SplitSubtitle;

public class SplitSubtitleWindow : Window
{
    private readonly SplitSubtitleViewModel _vm;

    public SplitSubtitleWindow(SplitSubtitleViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = Se.Language.Tools.SplitSubtitle.Title;
        CanResize = true;
        Width = 900;
        Height = 700;
        MinWidth = 600;
        MinHeight = 400;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButton(Se.Language.Tools.SplitSubtitle.Split, vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // input/output info
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // split options
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // split items list
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
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

        grid.Add(MakeInputOutputView(vm), 0);
        grid.Add(MakeOptionsView(vm), 1);
        grid.Add(MakeListView(vm), 2);
        grid.Add(panelButtons, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private Control MakeInputOutputView(SplitSubtitleViewModel vm)
    {
        var labelSubtitleInfo = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.SubtitleInfo));

        var labelOutputFolder = UiUtil.MakeLabel(Se.Language.General.OutputFolder).WithMinWidth(100);
        var comboOutputFoler = UiUtil.MakeTextBox(200, vm, nameof(vm.OutputFolder));
        var buttonBrowse = UiUtil.MakeBrowseButton(vm.BrowseCommand);
        var buttonOpen = UiUtil.MakeButton(vm.OpenFolderCommand, IconNames.FolderOpen, Se.Language.General.OpenOutputFolder);
        var panelOutputFolder = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                labelOutputFolder,
                comboOutputFoler,
                buttonBrowse,
                buttonOpen,
            }
        };

        var labelFormat = UiUtil.MakeLabel(Se.Language.General.Format).WithMinWidth(100);
        var comboFormat = UiUtil.MakeComboBox(vm.Formats, vm, nameof(vm.Formats), nameof(vm.SelectedSubtitleFormat));
        var panelFormat = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                labelFormat,
                comboFormat,
            }
        };

        var labelEncoding = UiUtil.MakeLabel(Se.Language.General.Encoding).WithMinWidth(100);
        var comboEncoding = UiUtil.MakeComboBox(vm.Encodings, vm, nameof(vm.Encodings), nameof(vm.SelectedEncoding));
        var panelEncoding = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                labelEncoding,
                comboEncoding,
            }
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                labelSubtitleInfo,
                panelOutputFolder,
                panelFormat,
                panelEncoding,
            },
            Spacing = 5,
        };

        return UiUtil.MakeBorderForControl(panel);
    }

    private static Border MakeOptionsView(SplitSubtitleViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var radioLines = UiUtil.MakeRadioButton(Se.Language.General.Lines, vm, nameof(vm.SplitByLines));
        var radioCharacters = UiUtil.MakeRadioButton(Se.Language.General.Characters, vm, nameof(vm.SplitByCharacters));
        var radioTime = UiUtil.MakeRadioButton(Se.Language.General.Time, vm, nameof(vm.SplitByTime));
        var panelSplitBy = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            Children = { radioLines, radioCharacters, radioTime },
            Margin = new Avalonia.Thickness(0, 0, 20, 0),
        };

        var labelParts = UiUtil.MakeLabel(Se.Language.Tools.SplitSubtitle.NumberOfEqualParts);
        var numberUpDownParts = UiUtil.MakeNumericUpDownInt(1, vm.PartsMax, 110, nameof(vm.NumberOfEqualParts));
        var panelParts = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            Children = { labelParts, numberUpDownParts },
        };

        grid.Add(panelSplitBy, 0);
        grid.Add(panelParts, 0, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

    private Control MakeListView(SplitSubtitleViewModel vm)
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
            ItemsSource = vm.SplitItems,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NoSymbolLines,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SplitDisplayItem.Lines)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Characters,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SplitDisplayItem.Characters)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FileName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SplitDisplayItem.FileName)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedSpiltItem)) { Source = vm });

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
