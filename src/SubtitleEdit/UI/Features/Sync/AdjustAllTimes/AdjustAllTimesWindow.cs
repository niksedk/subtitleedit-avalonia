using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Sync.AdjustAllTimes;

public class AdjustAllTimesWindow : Window
{
    private AdjustAllTimesViewModel _vm;

    public AdjustAllTimesWindow(AdjustAllTimesViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Adjust all times (show earlier/later)";
        Width = 510;
        Height = 275;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = "Adjustment",
            VerticalAlignment = VerticalAlignment.Center,
        };

        var timeCodeUpDown = new TimeCodeUpDown
        {
            DataContext = vm,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.Adjustment))
            {
                Mode = BindingMode.TwoWay,
            }
        };

        var panelAdjustment = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                label,
                timeCodeUpDown,
                UiUtil.MakeButton("Show earlier", vm.ShowEarlierCommand).WithMarginLeft(15),
                UiUtil.MakeButton("Show later", vm.ShowLaterCommand),
            },
        };

        var labelInfo = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
        };
        labelInfo.Bind(TextBlock.TextProperty, new Binding(nameof(vm.TotalAdjustmentInfo)));

        var panelRadioButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                new RadioButton
                {
                    Content = "Adjust All",
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustAll))
                },
                new RadioButton
                {
                    Content = "Adjust Selected Lines",
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustSelectedLines))
                },
                new RadioButton
                {
                    Content = "Adjust Selected Lines And Forward",
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustSelectedLinesAndForward))
                }
            }
        };
        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton("Apply", vm.ApplyCommand),
            UiUtil.MakeButton("OK", vm.OkCommand),
            UiUtil.MakeButton("Cancel", vm.CancelCommand)
        );
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition
                {
                    Height = new GridLength(1, GridUnitType.Auto)
                },

                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Children.Add(panelAdjustment);
        Grid.SetRow(panelAdjustment, 0);

        // grid.Children.Add(panelShowEarlierOrLater);
        // Grid.SetRow(panelShowEarlierOrLater, 1);

        grid.Children.Add(panelRadioButtons);
        Grid.SetRow(panelRadioButtons, 2);

        grid.Children.Add(buttonPanel);
        Grid.SetRow(buttonPanel, 3);
        
        Content = grid;
        
        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}