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
        Width = 510;
        Height = 240;
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
            MinWidth = 80,
        };

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
            MinWidth = 80,
        };

        var panelFromFrameRate = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                labelFromFrameRate,
                comboFromFrameRate,
                labelToFrameRate,
                comboToFrameRate
            }
        };

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton("OK", vm.OkCommand),
            UiUtil.MakeButton("Cancel", vm.CancelCommand)
        );
        
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Children.Add(panelFromFrameRate);
        Grid.SetRow(panelFromFrameRate, 0);
        Grid.SetColumn(panelFromFrameRate, 0);

        grid.Children.Add(buttonPanel);
        Grid.SetRow(buttonPanel, 1);
        Grid.SetColumn(buttonPanel, 0);

        grid.Children.Add(buttonPanel);
        Grid.SetRow(buttonPanel, 2);
        Grid.SetColumn(buttonPanel, 0);

        Content = grid;
        
        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
