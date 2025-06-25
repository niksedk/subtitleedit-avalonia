using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewChangeSpeed
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = "Change speed",
            VerticalAlignment = VerticalAlignment.Center,
        };
        
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

    //    var buttonFromDropFrame = UiUtil.MakeButton("From drop frame value", vm.SetFromDropFrameValueCommand);
     //   var buttonToDropFrame = UiUtil.MakeButton("To drop frame value", vm.SetToDropFrameValueCommand);

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
          //      buttonFromDropFrame,
          //    buttonToDropFrame
            }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelHeader, 0);
        grid.Add(panelSpeed, 1);

        return grid;
    }
}
