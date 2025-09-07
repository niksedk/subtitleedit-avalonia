using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;

public class ChangeSpeedWindow : Window
{
    private readonly ChangeSpeedViewModel _vm;
    
    public ChangeSpeedWindow(ChangeSpeedViewModel vm)
    {
        UiUtil.InitializeWindow(this);
        Title = "Change speed";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = "Speed in %",
            VerticalAlignment = VerticalAlignment.Center,
        };

        var numericUpDownSpeed = new NumericUpDown
        {
            Width = 150,
            Margin = new Thickness(0, 0, 10, 0),
            Minimum = 0,
            Maximum = 1000,
            Increment = 0.1m,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(ChangeSpeedViewModel.SpeedPercent)) { Mode = BindingMode.TwoWay },
        };

        var buttonFromDropFrame = UiUtil.MakeButton("From drop frame value", vm.SetFromDropFrameValueCommand);
        var buttonToDropFrame = UiUtil.MakeButton("To drop frame value", vm.SetToDropFrameValueCommand);

        var panelSpeed = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 10, 0),
            Children =
            {
                label,
                numericUpDownSpeed,
                buttonFromDropFrame,
                buttonToDropFrame
            }
        };

        var panelRadioButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(50, 10, 0, 0),
            Children =
            {
                new RadioButton
                {
                    Content = "Adjust all",
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustAll))
                },
                new RadioButton
                {
                    Content = "Adjust selected lines",
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustSelectedLines))
                },
                new RadioButton
                {
                    Content = "Adjust selected lines and forward",
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustSelectedLinesAndForward))
                }
            },
        };

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
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelSpeed, 0);
        grid.Add(panelRadioButtons, 1);
        grid.Add(buttonPanel, 2);

        Content = grid;
        
        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
