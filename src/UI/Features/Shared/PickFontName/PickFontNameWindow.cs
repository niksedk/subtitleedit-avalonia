using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickFontName;

public class PickFontNameWindow : Window
{
    private readonly PickFontNameViewModel _vm;

    public PickFontNameWindow(PickFontNameViewModel vm)
    {
        UiUtil.InitializeWindow(this);
        Title = Se.Language.Tools.PickFontNameTitle;
        CanResize = true;
        Width = 800;
        Height = 700;
        MinWidth = 500;
        MinHeight = 400;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelSearch = UiUtil.MakeLabel(Se.Language.General.Search);
        var textBoxSearch = new TextBox
        {
            Width = 300,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 10),
        };
        var panelSearch = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelSearch,
                textBoxSearch,
            }
        };

        var fontsView = MakeFontsView(vm);

        var imagePreview = new Image();
        var borderPreview = UiUtil.MakeBorderForControl(imagePreview);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
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

        grid.Add(panelSearch, 0);
        grid.Add(fontsView, 1);
        grid.Add(borderPreview, 2);
        grid.Add(buttonPanel, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private Border MakeFontsView(PickFontNameViewModel vm)
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
            DataContext = _vm,
            ItemsSource = _vm.FontNames,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FontName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MatroskaTrackInfoDisplay.TrackNumber)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star), 
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(_vm.SelectedFontName)));
        dataGrid.SelectionChanged += vm.DataGridFontNameSelectionChanged;

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
