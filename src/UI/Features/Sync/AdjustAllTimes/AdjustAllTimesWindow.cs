using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

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

        var splitButtonShowEarlier = new SplitButton
        {
            Content = Se.Language.Sync.ShowEarlier,
            VerticalAlignment = VerticalAlignment.Center,
            Command = vm.ShowEarlierCommand,
            Margin = new Thickness(10, 0, 0, 0),
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = Se.Language.General.TenMilliseconds,
                        Command = vm.ShowEarlierTimeSpanCommand,
                        CommandParameter = TimeSpan.FromMilliseconds(10),
                    },
                    new MenuItem
                    {
                        Header = Se.Language.General.OneHundredMilliseconds,
                        Command = vm.ShowEarlierTimeSpanCommand,
                        CommandParameter = TimeSpan.FromMilliseconds(100),
                    },
                    new MenuItem
                    {
                        Header = Se.Language.General.FiveHundredMilliseconds,
                        Command = vm.ShowEarlierTimeSpanCommand,
                        CommandParameter = TimeSpan.FromMilliseconds(500),
                    },
                    new MenuItem
                    {
                        Header = Se.Language.General.OneSecond,
                        Command = vm.ShowEarlierTimeSpanCommand,
                        CommandParameter = TimeSpan.FromSeconds(1),
                    },
                    new MenuItem
                    {
                        Header = Se.Language.General.FiveSeconds,
                        Command = vm.ShowEarlierTimeSpanCommand,
                        CommandParameter = TimeSpan.FromSeconds(5),
                    },
                }
            }
        };

        var splitButtonShowLater = new SplitButton
        {
            Content = Se.Language.Sync.ShowLater,
            VerticalAlignment = VerticalAlignment.Center,
            Command = vm.ShowLaterCommand,
            Margin = new Thickness(5, 0, 0, 0),
            Flyout = new MenuFlyout
            {
                Items =
                {
                   new MenuItem
                    {
                        Header = Se.Language.General.TenMilliseconds,
                        Command = vm.ShowLaterTimeSpanCommand,
                        CommandParameter = TimeSpan.FromMilliseconds(10),
                    },
                    new MenuItem
                    {
                        Header = Se.Language.General.OneHundredMilliseconds,
                        Command = vm.ShowLaterTimeSpanCommand,
                        CommandParameter = TimeSpan.FromMilliseconds(100),
                    },
                    new MenuItem
                    {
                        Header = Se.Language.General.FiveHundredMilliseconds,
                        Command = vm.ShowLaterTimeSpanCommand,
                        CommandParameter = TimeSpan.FromMilliseconds(500),
                    },
                    new MenuItem
                    {
                        Header = Se.Language.General.OneSecond,
                        Command = vm.ShowLaterTimeSpanCommand,
                        CommandParameter = TimeSpan.FromSeconds(1),
                    },
                    new MenuItem
                    {
                        Header = Se.Language.General.FiveSeconds,
                        Command = vm.ShowLaterTimeSpanCommand,
                        CommandParameter = TimeSpan.FromSeconds(5),
                    },
                }
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
                splitButtonShowEarlier,
                splitButtonShowLater,
            },
        };

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

        grid.Add(panelAdjustment, 0);
        grid.Add(panelRadioButtons, 1);
        grid.Add(buttonPanel, 2);

        Content = grid;

        Loaded += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}