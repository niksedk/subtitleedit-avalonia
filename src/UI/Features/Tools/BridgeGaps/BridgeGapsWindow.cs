using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.BridgeGaps;

public class BridgeGapsWindow : Window
{
    private readonly BridgeGapsViewModel _vm;

    public BridgeGapsWindow(BridgeGapsViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = Se.Language.Tools.AdjustDurations.Title;
        CanResize = true;
        Width = 1000;
        Height = 800;
        MinWidth = 900;
        MinHeight = 500;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelBridgeGapSmallerThan = UiUtil.MakeLabel(Se.Language.Tools.BridgeGaps.BridgeGapsSmallerThan);
        var numericUpDownBridgeGapSmallerThan = UiUtil.MakeNumericUpDownInt(1, 10000, 130, vm, nameof(vm.BridgeGapsSmallerThanMs));
        numericUpDownBridgeGapSmallerThan.ValueChanged += vm.ValueChanged;

        var labelMinGap = UiUtil.MakeLabel(Se.Language.Tools.BridgeGaps.MinGap);
        var numericUpDownMinGap = UiUtil.MakeNumericUpDownInt(0, 1000, 130, vm, nameof(vm.MinGapMs));
        numericUpDownMinGap.ValueChanged += vm.ValueChanged;

        var labelPercentForLeft = UiUtil.MakeLabel(Se.Language.Tools.BridgeGaps.PercentForLeft);
        var numericUpDownPercentForLeft = UiUtil.MakeNumericUpDownInt(0, 100, 130, vm, nameof(vm.PercentForLeft));
        numericUpDownPercentForLeft.ValueChanged += vm.ValueChanged;

        var panelControls = UiUtil.MakeHorizontalPanel(
            labelBridgeGapSmallerThan,
            numericUpDownBridgeGapSmallerThan,
            labelMinGap,
            numericUpDownMinGap,
            labelPercentForLeft,
            numericUpDownPercentForLeft);

        var subtitleView = MakeSubtitleView(vm);

        var labelStatus = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.StatusText)).WithAlignmentTop();

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Bridge gap smaller than
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Subtitle view
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
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

        grid.Add(panelControls, 0);
        grid.Add(subtitleView, 1);
        grid.Add(labelStatus, 2);
        grid.Add(panelButtons, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private Border MakeSubtitleView(BridgeGapsViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var dataGridSubtitle = new DataGrid
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
            ItemsSource = vm.Subtitles,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BridgeGapDisplayItem.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BridgeGapDisplayItem.StartTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Duration,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BridgeGapDisplayItem.Duration)) { Converter = shortTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BridgeGapDisplayItem.Text)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Status,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BridgeGapDisplayItem.InfoText)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
            },
        };
        //dataGridSubtitle.Bind(DataGrid.SelectedItemProperty,
        //    new Binding(nameof(vm.SelectedOcrSubtitleItem)) { Source = vm });
        //vm.SubtitleGrid = dataGridSubtitle;

        return UiUtil.MakeBorderForControl(dataGridSubtitle);
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
