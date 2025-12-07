using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitFooter
{
    public static Grid Make(MainViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            RowDefinitions = new RowDefinitions("Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(5, 0, 5, 0),
        };

        vm.StatusTextLeftLabel = new TextBlock
        {
            Text = string.Empty,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
        };
        grid.Add(vm.StatusTextLeftLabel, 0);
        vm.StatusTextLeftLabel.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusTextLeft)));

        var right = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 4),
            DataContext = vm,
        };
        right.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusTextRight)));

        var panelRight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Icon
                {
                    Value = IconNames.LockClock,
                    FontSize = 20,
                    [ToolTip.TipProperty] = Se.Language.General.LockTimeCodes,
                    [!Visual.IsVisibleProperty] = new Binding(nameof(vm.LockTimeCodes)),
                },
                new Button
                {
                    Content = new Icon
                    {
                        Value = IconNames.Filter,
                        FontSize = 20,
                        [ToolTip.TipProperty] = Se.Language.General.LayerFilterOn,
                    },
                    [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowLayerFilterIcon)),
                    [!Button.CommandProperty] = new Binding(nameof(vm.ShowPickLayerFilterCommand)),

                    // make it look like just an icon
                    Background = null,
                    BorderBrush = null,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0,0,15,0),
                },

                new Button
                {
                    Content = new Icon
                    {
                        Value = IconNames.TimerSettings,
                        FontSize = 20,
                        [ToolTip.TipProperty] = Se.Language.General.OffsetTimeCodes,
                    },
                    [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsVideoOffsetVisible)),
                    [!Button.CommandProperty] = new Binding(nameof(vm.ShowVideoSetOffsetCommand)),

                    // make it look like just an icon
                    Background = null,
                    BorderBrush = null,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                },
                UiUtil.MakeLabel()
                    .WithBindText(vm, nameof(vm.VideoOffsetText))
                    .WithBindVisible(vm, nameof(vm.IsVideoOffsetVisible)).WithMarginRight(15),

                right,
            },
        };

        grid.Add(panelRight, 0, 1);

        return grid;
    }
}