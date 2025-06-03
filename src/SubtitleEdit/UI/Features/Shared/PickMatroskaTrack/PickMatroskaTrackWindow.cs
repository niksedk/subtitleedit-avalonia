using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;

public class PickMatroskaTrackWindow : Window
{
    private readonly PickMatroskaTrackViewModel _vm;

    public PickMatroskaTrackWindow(PickMatroskaTrackViewModel vm)
    {
        _vm = vm;
        vm.Window = this;
        Icon = UiUtil.GetSeIcon();
        Title = "Pick Matroka track";
        Width = 1000;
        Height = 600;
        MinWidth = 800;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var tracksView = MakeTracksView(vm);
        var subtitleView = MakeSubtitleView(vm);

        var buttonExport = UiUtil.MakeButton("Export...", vm.ExportCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonExport, buttonOk, buttonCancel);

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(tracksView, 0, 0);
        grid.Add(subtitleView, 0, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);


        Content = grid;

        Activated += delegate
        {
            buttonOk.Focus(); // hack to make OnKeyDown work
        };
    }

    private Border MakeTracksView(PickMatroskaTrackViewModel vm)
    {
        var dataGridTracks = new DataGrid
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
            ItemsSource = _vm.Tracks,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = "#",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MatroskaTrackInfoDisplay.TrackNumber)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Name",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MatroskaTrackInfoDisplay.Name)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Language",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MatroskaTrackInfoDisplay.Language)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Codec",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MatroskaTrackInfoDisplay.Codec)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Default",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MatroskaTrackInfoDisplay.IsDefault)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Forced",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MatroskaTrackInfoDisplay.IsForced)),
                    IsReadOnly = true,
                },
            },
        };
        dataGridTracks.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(_vm.SelectedTrack)));
        dataGridTracks.SelectionChanged += vm.DataGridTracksSelectionChanged;

        var border = new Border
        {
            Child = dataGridTracks,
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            Padding = new Thickness(10, 0, 10, 0),
            CornerRadius = new CornerRadius(5),
        };

        return border;
    }

    private Border MakeSubtitleView(PickMatroskaTrackViewModel vm)
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
            DataContext = _vm,
            ItemsSource = _vm.Rows,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = "#",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MatroskaSubtitleCueDisplay.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Show",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MatroskaSubtitleCueDisplay.Show)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Duration",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MatroskaSubtitleCueDisplay.Duration)) { Converter = shortTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Text",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MatroskaSubtitleCueDisplay.Text)),
                    IsReadOnly = true,
                },
            },
        };

        var border = new Border
        {
            Child = dataGridSubtitle,
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            Padding = new Thickness(10, 0, 10, 0),
            CornerRadius = new CornerRadius(5),
        };

        return border;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}