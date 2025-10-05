using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class ProfilesWindow : Window
{
    public ProfilesWindow(ProfilesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Profiles;
        CanResize = true;
        Width = 1000;
        Height = 800;
        MinWidth = 800;
        MinHeight = 700;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = Se.Language.Tools.AdjustDurations.AdjustVia,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeDataGrid(vm), 0, 0);
        grid.Add(MakeControlsGrid(vm), 0, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Border MakeDataGrid(ProfilesViewModel vm)
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
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Name,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(ProfileDisplay.Name)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.Options.Settings.SingleLineMaxLength,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(ProfileDisplay.SingleLineMaxLength)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.MaxCharactersPerSecond,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(ProfileDisplay.MaxCharsPerSec)),
                    IsReadOnly = true,
                },
            },
        };
        dataGrid.Bind(DataGrid.ItemsSourceProperty, new Binding(nameof(vm.Profiles)) { Source = vm });
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedProfile)) { Source = vm });

        return UiUtil.MakeBorderForControl(dataGrid);
    }

    private static Grid MakeControlsGrid(ProfilesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(0),
            ColumnSpacing = 0,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var labelName = UiUtil.MakeLabel(Se.Language.General.Name);
        var textBoxName = UiUtil.MakeTextBox(250, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.Name));

        grid.Add(labelName, 0, 0);
        grid.Add(textBoxName, 0, 1);

        return grid;
    }
}
