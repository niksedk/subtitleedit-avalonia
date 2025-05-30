using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Edit.ShowHistory;

public class ShowHistoryWindow : Window
{
    private ShowHistoryViewModel _vm;

    public ShowHistoryWindow(ShowHistoryViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Show history";
        Width = 710;
        Height = 640;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Single,
            Margin = new Thickness(0, 10, 0, 0),
            [!DataGrid.ItemsSourceProperty] = new Binding(nameof(vm.HistoryItems)),
            [!DataGrid.SelectedItemProperty] = new Binding(nameof(vm.SelectedHistoryItem)),
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = "Time",
                    Binding = new Binding(nameof(ShowHistoryDisplayItem.Time)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = "Description",
                    Binding = new Binding(nameof(ShowHistoryDisplayItem.Description)),
                    Width = new DataGridLength(3, DataGridLengthUnitType.Auto)
                }
            },
        };
        dataGrid.SelectionChanged += (sender, args) =>
        {
            vm.IsRollbackEnabled = dataGrid.SelectedItem != null;
        };

        var buttonRollback = UiUtil.MakeButton("Restore selected", vm.RollbackToCommand).WithBindEnabled(nameof(vm.IsRollbackEnabled));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonRollback,
            buttonCancel
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
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(dataGrid, 0, 0);
        grid.Add(panelButtons, 1, 0);

        Content = grid;

        Activated += delegate { buttonCancel.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
