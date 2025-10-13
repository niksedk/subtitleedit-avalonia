using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia;
using System;
using System.Globalization;
using MenuItem = Avalonia.Controls.MenuItem;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class InitWaveform
{
    public static Grid MakeWaveform(MainViewModel vm)
    {
        var languageHints = Se.Language.Main.Waveform;
        var shortcuts = ShortcutsMain.GetUsedShortcuts(vm);

        // Create main layout grid
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Margin = new Thickness(10, 0, 10, 0),
        };

        // waveform area
        if (vm.AudioVisualizer == null)
        {
            vm.AudioVisualizer = new AudioVisualizer
            {
                DrawGridLines = Se.Settings.Waveform.DrawGridLines,
                WaveformColor = Se.Settings.Waveform.WaveformColor.FromHexToColor(),
                WaveformSelectedColor = Se.Settings.Waveform.WaveformSelectedColor.FromHexToColor(),
                WaveformCursorColor = Se.Settings.Waveform.WaveformCursorColor.FromHexToColor(),
                InvertMouseWheel = Se.Settings.Waveform.InvertMouseWheel,
            };
            vm.AudioVisualizer.OnNewSelectionInsert += vm.AudioVisualizerOnNewSelectionInsert;
            vm.AudioVisualizer.OnVideoPositionChanged += vm.AudioVisualizerOnVideoPositionChanged;
            vm.AudioVisualizer.OnToggleSelection += vm.AudioVisualizerOnToggleSelection;
            //vm.AudioVisualizer.OnStatus += vm.AudioVisualizerOnStatus;
            vm.AudioVisualizer.OnParagraphDoubleTapped += vm.OnWaveformDoubleTapped;

            // Create a Flyout for the DataGrid
            var flyout = new MenuFlyout();

            //flyout.Opening += vm.AudioVisualizerContextOpening;
            vm.AudioVisualizer.FlyoutMenuOpening += vm.AudioVisualizerFlyoutMenuOpening;

            var insertSelectionMenuItem = new MenuItem
            {
                Header = Se.Language.General.InsertNewSelection,
                Command = vm.WaveformInsertNewSelectionCommand,
            };
            flyout.Items.Add(insertSelectionMenuItem);
            vm.MenuItemAudioVisualizerInsertNewSelection = insertSelectionMenuItem;

            var insertNewMenuItem = new MenuItem
            {
                Header = Se.Language.General.InsertAtPosition,
                Command = vm.WaveformInsertAtPositionCommand,
            };
            flyout.Items.Add(insertNewMenuItem);
            vm.MenuItemAudioVisualizerInsertAtPosition = insertNewMenuItem;

            var deleteAtPositionMenuItem = new MenuItem
            {
                Header = Se.Language.General.DeleteAtPosition,
                Command = vm.WaveformDeleteAtPositionCommand,
            };
            flyout.Items.Add(deleteAtPositionMenuItem);
            vm.MenuItemAudioVisualizerDeleteAtPosition = deleteAtPositionMenuItem;

            // Add menu items with commands
            var deleteMenuItem = new MenuItem
            {
                Header = Se.Language.General.Delete,
                Command = vm.DeleteSelectedLinesCommand
            };
            flyout.Items.Add(deleteMenuItem);
            vm.MenuItemAudioVisualizerDelete = deleteMenuItem;

            var insertBeforeMenuItem = new MenuItem
            {
                Header = Se.Language.General.InsertBefore,
                Command = vm.InsertLineBeforeCommand
            };
            flyout.Items.Add(insertBeforeMenuItem);
            vm.MenuItemAudioVisualizerInsertBefore = insertBeforeMenuItem;

            var insertAfterMenuItem = new MenuItem
            {
                Header = Se.Language.General.InsertAfter,
                Command = vm.InsertLineAfterCommand
            };
            flyout.Items.Add(insertAfterMenuItem);
            vm.MenuItemAudioVisualizerInsertAfter = insertAfterMenuItem;

            var separator1 = new Separator();
            flyout.Items.Add(separator1);
            vm.MenuItemAudioVisualizerSeparator1 = separator1;

            var splitMenuItem = new MenuItem
            {
                Header = Se.Language.General.SplitLine,
                Command = vm.SplitCommand
            };
            flyout.Items.Add(splitMenuItem);
            vm.MenuItemAudioVisualizerSplit = splitMenuItem;

            flyout.Items.Add(new Separator());

            var menuItemFilterByLayer = new MenuItem
            {
                Header = Se.Language.General.FilterByLayer,
                Command = vm.ShowPickLayerFilterCommand,
            }.BindIsVisible(vm, nameof(vm.IsFormatAssa));
            flyout.Items.Add(menuItemFilterByLayer);

            var menuItemGuessTimeCodes = new MenuItem
            {
                Header = Se.Language.Waveform.GuessTimeCodesDotDotDot,
                Command = vm.ShowWaveformGuessTimeCodesCommand,
            };
            flyout.Items.Add(menuItemGuessTimeCodes);

            var menuItemAddShotChange = new MenuItem
            {
                Header = Se.Language.Waveform.ToggleShotChange,
                Command = vm.ToggleShotChangesAtVideoPositionCommand,
            };
            flyout.Items.Add(menuItemAddShotChange);


            var menuItemSeekSilence = new MenuItem
            {
                Header = Se.Language.Waveform.SeekSilenceDotDotDot,
                Command = vm.ShowWaveformSeekSilenceCommand,
            };
            flyout.Items.Add(menuItemSeekSilence);

            vm.AudioVisualizer.MenuFlyout = flyout;
        }
        else
        {
            vm.AudioVisualizer.RemoveControlFromParent();
        }

        Grid.SetRow(vm.AudioVisualizer, 0);
        mainGrid.Children.Add(vm.AudioVisualizer);

        // Footer
        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        controlsPanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsWaveformToolbarVisible)));

        var buttonPlay = new Button
        {
            Margin = new Thickness(0, 0, 3, 0),
            Command = vm.TogglePlayPauseCommand,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.PlayPauseHint, shortcuts, nameof(vm.TogglePlayPauseCommand)),
        };
        Attached.SetIcon(buttonPlay, IconNames.Play);
        vm.ButtonWaveformPlay = buttonPlay;

        var buttonNew = new Button
        {
            Margin = new Thickness(0, 0, 3, 0),
            Command = vm.WaveformInsertAtPositionCommand,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.NewHint, shortcuts, nameof(vm.WaveformInsertAtPositionCommand)),
        };
        Attached.SetIcon(buttonNew, IconNames.Plus);

        var buttonSetStartAndOffsetTheRest = new Button
        {
            Margin = new Thickness(0, 0, 3, 0),
            Command = vm.WaveformSetStartAndOffsetTheRestCommand,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SetStartAndOffsetTheRestHint, shortcuts, nameof(vm.WaveformSetStartAndOffsetTheRestCommand)),
        };
        Attached.SetIcon(buttonSetStartAndOffsetTheRest, IconNames.ArrowExpandRight);

        var buttonSetStart = new Button
        {
            Margin = new Thickness(0, 0, 3, 0),
            Command = vm.WaveformSetStartCommand,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SetStartHint, shortcuts, nameof(vm.WaveformSetStartCommand)),
        };
        Attached.SetIcon(buttonSetStart, IconNames.RayStart);

        var buttonSetEnd = new Button
        {
            Margin = new Thickness(0, 0, 3, 0),
            Command = vm.WaveformSetEndCommand,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SetEndHint, shortcuts, nameof(vm.WaveformSetEndCommand)),
        };
        Attached.SetIcon(buttonSetEnd, IconNames.RayEnd);

        var buttonRepeat = new Button
        {
            Margin = new Thickness(0, 0, 3, 0),
            //Command = vm.WaveformSetStartAndOffsetTheRestCommand,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.RepeatHint, shortcuts, nameof(vm.WaveformSetEndCommand)),
        };
        Attached.SetIcon(buttonRepeat, IconNames.Repeat);


        var iconHorizontal = new Icon
        {
            Value = IconNames.ArrowLeftRightBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 4, 0)
        };

        var sliderHorizontalZoom = new Slider
        {
            Minimum = 0.1,
            Maximum = 20.0,
            Width = 80,
            VerticalAlignment = VerticalAlignment.Center,
            Value = 1,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.ZoomHorizontalHint, shortcuts),
        };
        sliderHorizontalZoom.TemplateApplied += (s, e) =>
        {
            if (e.NameScope.Find<Thumb>("thumb") is Thumb thumb)
            {
                thumb.Width = 14;
                thumb.Height = 14;
            }
        };
        sliderHorizontalZoom.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.AudioVisualizer) + "." + nameof(vm.AudioVisualizer.ZoomFactor)));

        var iconVertical = new Icon
        {
            Value = IconNames.ArrowUpDownBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 4, 0)
        };

        var sliderVerticalZoom = new Slider
        {
            Minimum = 0.1,
            Maximum = 20.0,
            Width = 80,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            Value = 1,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.ZoomVerticalHint, shortcuts),
        };
        sliderVerticalZoom.TemplateApplied += (s, e) =>
        {
            if (e.NameScope.Find<Thumb>("thumb") is Thumb thumb)
            {
                thumb.Width = 14;
                thumb.Height = 14;
            }
        };
        sliderVerticalZoom.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.AudioVisualizer) + "." + nameof(vm.AudioVisualizer.VerticalZoomFactor)));

        var sliderPosition = new Slider
        {
            Minimum = 0,
            Width = 160,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            Focusable = true,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.VideoPosition, shortcuts),
        };
        sliderPosition.TemplateApplied += (s, e) =>
        {
            if (e.NameScope.Find<Thumb>("thumb") is Thumb thumb)
            {
                thumb.Width = 14;
                thumb.Height = 14;
            }
        };
        sliderPosition.Bind(RangeBase.MaximumProperty, new Binding(nameof(vm.VideoPlayerControl) + "." + nameof(vm.VideoPlayerControl.Duration)));
        sliderPosition.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.VideoPlayerControl) + "." + nameof(vm.VideoPlayerControl.Position)));

        var labelSpeed = UiUtil.MakeLabel(Se.Language.General.Speed);
        var comboBoxSpeed = new ComboBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            ItemsSource = new[] { "0.5x", "0.75x", "1.0x", "1.25x", "1.5x", "1.75x", "2.0x", "3.0x" },
            SelectedItem = "1.0x",
            Margin = new Thickness(0, 0, 10, 0),
            FontSize = 12,
            MaxHeight = 22,
            MinHeight = 22,
            Padding = new Thickness(2, 2, 0, 2),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
        };
        comboBoxSpeed.SelectionChanged += (s, e) =>
        {
            if (vm.AudioVisualizer != null && comboBoxSpeed.SelectedItem is string s1 && s1.EndsWith("x") &&
                double.TryParse(s1.Trim('x'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double speed))
            {
                vm.VideoPlayerControl?.SetSpeed(speed);
            }
        };
        var panelSpeed = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            Children =
            {
                labelSpeed,
                comboBoxSpeed
            }
        };

        var toggleButtonAutoSelectOnPlay = new ToggleButton
        {
            DataContext = vm,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.SelectCurrentSubtitleWhilePlaying)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(3, 0, 0, 0),
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SelectCurrentLineWhilePlayingHint, shortcuts),
        };
        Attached.SetIcon(toggleButtonAutoSelectOnPlay, IconNames.AnimationPlay);
        toggleButtonAutoSelectOnPlay.IsCheckedChanged += (s, e) => vm.AutoSelectOnPlayCheckedChanged();

        var toggleButtonCenter = new ToggleButton
        {
            DataContext = vm,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.WaveformCenter)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(3, 0, 0, 0),
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.CenterWaveformHint, shortcuts),
        };
        Attached.SetIcon(toggleButtonCenter, IconNames.AlignHorizontalCenter);
        toggleButtonCenter.IsCheckedChanged += (s, e) => vm.WaveformCenterCheckedChanged();

        var buttonMore = new Button
        {
            Margin = new Thickness(8, 0, 0, 0),
        };
        Attached.SetIcon(buttonMore, "fa-ellipsis-v");

        var flyoutMore = new MenuFlyout();
        buttonMore.Flyout = flyoutMore;
        buttonMore.Click += (s, e) => flyoutMore.ShowAt(buttonMore, true);
        var menuItemHideControls = new MenuItem
        {
            Header = string.Format(languageHints.HideWaveformToolbar, string.Empty),
            Command = vm.HideWaveformToolbarCommand,
        };
        flyoutMore.Items.Add(menuItemHideControls);
        var menuItemResetZoom = new MenuItem
        {
            Header = string.Format(languageHints.ResetZoom, string.Empty),
            Command = vm.ResetWaveformZoomCommand,
        };
        flyoutMore.Items.Add(menuItemResetZoom);

        controlsPanel.Children.Add(buttonPlay);
        controlsPanel.Children.Add(buttonNew);
        controlsPanel.Children.Add(buttonSetStartAndOffsetTheRest);
        controlsPanel.Children.Add(buttonSetStart);
        controlsPanel.Children.Add(buttonSetEnd);
        //  controlsPanel.Children.Add(buttonRepeat);
        //controlsPanel.Children.Add(UiUtil.MakeSeparatorForHorizontal());
        controlsPanel.Children.Add(iconHorizontal);
        controlsPanel.Children.Add(sliderHorizontalZoom);
        controlsPanel.Children.Add(iconVertical);
        controlsPanel.Children.Add(sliderVerticalZoom);
        controlsPanel.Children.Add(sliderPosition);
        controlsPanel.Children.Add(panelSpeed);

        controlsPanel.Children.Add(toggleButtonAutoSelectOnPlay);
        controlsPanel.Children.Add(toggleButtonCenter);

        controlsPanel.Children.Add(buttonMore);

        mainGrid.Children.Add(controlsPanel);
        Grid.SetRow(controlsPanel, 1);

        return mainGrid;
    }
}