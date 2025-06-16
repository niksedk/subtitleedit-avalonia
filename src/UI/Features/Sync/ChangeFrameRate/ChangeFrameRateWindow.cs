using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;

public class ChangeFrameRateWindow : Window
{
    private ChangeFrameRateViewModel _vm;
    
    public ChangeFrameRateWindow(ChangeFrameRateViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Change frame rate";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelFromFrameRate = new Label
        {
            Content = "From frame rate",
            VerticalAlignment = VerticalAlignment.Center,
        };

        var comboFromFrameRate = new ComboBox
        {
            ItemsSource = vm.FromFrameRates,
            SelectedValue = vm.SelectedFromFrameRate,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 90,
        }.WithBindSelected(nameof(vm.SelectedFromFrameRate));

        var buttonFromFrameRate = UiUtil.MakeButtonBrowse(vm.BrowseFromFrameRateCommand);

        var buttonSwitch = UiUtil.MakeButton(vm.SwitchFrameRatesCommand, IconNames.MdiSwapVertical);

        var labelToFrameRate = new Label
        {
            Content = "To frame rate",
            VerticalAlignment = VerticalAlignment.Center,
        };

        var comboToFrameRate = new ComboBox
        {
            ItemsSource = vm.ToFrameRates,
            SelectedValue = vm.SelectedToFrameRate,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 90,
        }.WithBindSelected(nameof(vm.SelectedToFrameRate));

        var buttonToFrameRate = UiUtil.MakeButtonBrowse(vm.BrowseToFrameRateCommand);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var row = 0;
        grid.Add(labelFromFrameRate, row, 0);
        grid.Add(comboFromFrameRate, row, 1);
        grid.Add(buttonFromFrameRate, row, 2);
        grid.Add(buttonSwitch, row, 3, 2);
        row++;

        grid.Add(labelToFrameRate, row, 0);
        grid.Add(comboToFrameRate, row, 1);
        grid.Add(buttonToFrameRate, row, 2);
        row++;

        grid.Add(buttonPanel, row, 0, 1, 4);

        Content = grid;
        
        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
