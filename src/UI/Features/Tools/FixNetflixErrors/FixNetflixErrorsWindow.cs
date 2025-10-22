using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.FixNetflixErrors;

public class FixNetflixErrorsWindow : Window
{
    private readonly FixNetflixErrorsViewModel _vm;

    public FixNetflixErrorsWindow(FixNetflixErrorsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.NetflixCheckAndFix.Title;
        Width = 910;
        Height = 640;
        MinWidth = 800;
        MinHeight = 620;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var settingsView = MakeSettingsView(vm);
        var fixesView = MakeFixesView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonOk,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );

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
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(settingsView, 0, 0);
        grid.Add(fixesView, 0, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private Control MakeSettingsView(FixNetflixErrorsViewModel vm)
    {
        // Top: language selection
        var panelTop = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                UiUtil.MakeTextBlock(Se.Language.General.Language).WithMarginRight(5),
                UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage))
            }
        };

        // Grid with list of checks
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            [!DataGrid.ItemsSourceProperty] = new Binding(nameof(vm.Checks))
        };

        dataGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Enabled,
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
            CellTemplate = new FuncDataTemplate<NetflixCheckDisplayItem>((item, _) =>
            {
                var cb = new CheckBox
                {
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(NetflixCheckDisplayItem.IsSelected)) { Mode = BindingMode.TwoWay },
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                cb.IsCheckedChanged += (_, __) => vm.SetDirty();
                return new Border
                {
                    Background = Brushes.Transparent,
                    Padding = new Thickness(4),
                    Child = cb
                };
            }),
            Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
            Binding = new Binding(nameof(NetflixCheckDisplayItem.Name)),
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.General.SelectAll, vm.ChecksSelectAllCommand),
            UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.ChecksInverseSelectionCommand)
        );

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions = { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) } },
        };

        grid.Add(panelTop, 0, 0);
        grid.Add(dataGrid, 1, 0);
        grid.Add(buttonPanel, 2, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private Border MakeFixesView(FixNetflixErrorsViewModel vm)
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
            ItemsSource = _vm.Fixes,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<FixNetflixErrorsItem>((item, _) =>
                        new Border
                        {
                            Background = Brushes.Transparent, // Prevents highlighting
                            Padding = new Thickness(4),
                            Child = new CheckBox
                            {
                                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixNetflixErrorsItem.Apply)),
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                        }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FixNetflixErrorsItem.IndexDisplay)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Before,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FixNetflixErrorsItem.Before)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.After,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FixNetflixErrorsItem.After)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(_vm.SelectedFix)));
        //dataGridTracks.SelectionChanged += vm.DataGridTracksSelectionChanged;

        return UiUtil.MakeBorderForControl(dataGrid);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded(e);
    }
}