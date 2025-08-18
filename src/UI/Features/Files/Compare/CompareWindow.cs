using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;

public class CompareWindow : Window
{
    public CompareWindow(CompareViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = Se.Language.File.Compare;
        Width = 1200;
        Height = 600;
        CanResize = true;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star), // subtitle views
                new RowDefinition(GridLength.Auto), // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10
        };

        // left (original)
        var leftView = MakeSubtitlesView(vm.LeftSubtitles, nameof(vm.SelectedLeft));
        grid.Children.Add(leftView);
        Grid.SetRow(leftView, 0);
        Grid.SetColumn(leftView, 0);

        // right (modified)
        var rightView = MakeSubtitlesView(vm.RightSubtitles, nameof(vm.SelectedRight));
        grid.Children.Add(rightView);
        Grid.SetRow(rightView, 0);
        Grid.SetColumn(rightView, 1);

        // Buttons
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        grid.Children.Add(panelButtons);
        Grid.SetRow(panelButtons, 1);
        Grid.SetColumnSpan(panelButtons, 2);

        Content = grid;
    }

    private Border MakeSubtitlesView(ObservableCollection<SubtitleLineViewModel> items, string selectedBinding)
    {
        var dg = new DataGrid
        {
            ItemsSource = items,
            CanUserSortColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Single,
            Height = double.NaN,
            Margin = new Thickness(2)
        };

        dg.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
            Width = new DataGridLength(50),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });

        dg.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Show,
            Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)),
            Width = new DataGridLength(120),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });

        dg.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Hide,
            Binding = new Binding(nameof(SubtitleLineViewModel.EndTime)),
            Width = new DataGridLength(120),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });

        dg.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(SubtitleLineViewModel.Text)),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });

        dg.Bind(DataGrid.SelectedItemProperty, new Binding(selectedBinding)
        {
            Mode = BindingMode.TwoWay
        });

        return UiUtil.MakeBorderForControl(dg);
    }
}
