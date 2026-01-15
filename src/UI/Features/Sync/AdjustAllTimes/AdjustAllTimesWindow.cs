using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Sync.AdjustAllTimes;

public class AdjustAllTimesWindow : Window
{
    public AdjustAllTimesWindow(AdjustAllTimesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Sync.AdjustAllTimes;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = Se.Language.General.Adjustment,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 5, 0, 5),
        };

        var timeCodeUpDown = new TimeCodeUpDown
        {
            DataContext = vm,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.Adjustment))
            {
                Mode = BindingMode.TwoWay,
            },
            VerticalAlignment = VerticalAlignment.Top,
        };

        var gridAdjustment = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing  = 5,
        };

        gridAdjustment.Add(label, 0, 0);
        gridAdjustment.Add(timeCodeUpDown, 0, 1);
        gridAdjustment.Add(UiUtil.MakeButton(Se.Language.Sync.ShowEarlier, vm.ShowEarlierCommand).WithMarginBottom(25).WithMarginTop(0).WithMinWidth(150), 0, 2);
        gridAdjustment.Add(UiUtil.MakeButton(Se.Language.Sync.ShowLater, vm.ShowLaterCommand).WithMarginBottom(25).WithMarginTop(0).WithMinWidth(150), 0, 3);


        gridAdjustment.Add(UiUtil.MakeButton(Se.Language.Sync.ShowEarlier + ": 10 ms", vm.ShowEarlierCommand).WithMinWidth(150), 1, 2);
        gridAdjustment.Add(UiUtil.MakeButton(Se.Language.Sync.ShowLater + ": 10 ms", vm.ShowLaterCommand).WithMinWidth(150), 1, 3);

        gridAdjustment.Add(UiUtil.MakeButton(Se.Language.Sync.ShowEarlier + ": 100 ms", vm.ShowEarlierCommand).WithMinWidth(150), 2, 2);
        gridAdjustment.Add(UiUtil.MakeButton(Se.Language.Sync.ShowLater + ": 100 ms", vm.ShowLaterCommand).WithMinWidth(150), 2, 3);

        gridAdjustment.Add(UiUtil.MakeButton(Se.Language.Sync.ShowEarlier + ": ½ sec", vm.ShowEarlierCommand).WithMinWidth(150), 3, 2);
        gridAdjustment.Add(UiUtil.MakeButton(Se.Language.Sync.ShowLater + ": ½ sec", vm.ShowLaterCommand).WithMinWidth(150), 3, 3);

        gridAdjustment.Add(UiUtil.MakeButton(Se.Language.Sync.ShowEarlier + ": 1 sec", vm.ShowEarlierCommand).WithMinWidth(150), 4, 2);
        gridAdjustment.Add(UiUtil.MakeButton(Se.Language.Sync.ShowLater + ": 1 sec", vm.ShowLaterCommand).WithMinWidth(150), 4, 4);

        gridAdjustment.Add(UiUtil.MakeButton(Se.Language.Sync.ShowEarlier + ": 5 sec", vm.ShowEarlierCommand).WithMinWidth(150), 5, 2);
        gridAdjustment.Add(UiUtil.MakeButton(Se.Language.Sync.ShowLater + ": 5 sec", vm.ShowLaterCommand).WithMinWidth(150), 5, 3);


        var panelRadioButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(50, 10, 0, 0),
            Children =
            {
                new RadioButton
                {
                    Content = Se.Language.Sync.AdjustAll,
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustAll))
                },
                new RadioButton
                {
                    Content = Se.Language.Sync.AdjustSelectedLines,
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustSelectedLines))
                },
                new RadioButton
                {
                    Content = Se.Language.Sync.AdjustSelectedLinesAndForward,
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustSelectedLinesAndForward))
                }
            },
        };

        var buttonOk = UiUtil.MakeButtonDone(vm.OkCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk);
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

        grid.Add(gridAdjustment, 0);
        grid.Add(panelRadioButtons, 1);
        grid.Add(buttonPanel, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}