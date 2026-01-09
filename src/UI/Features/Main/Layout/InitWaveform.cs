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
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = double.NaN, // Auto height
        };

        // waveform area
        if (vm.AudioVisualizer == null)
        {
            vm.AudioVisualizer = new AudioVisualizer
            {
                DrawGridLines = Se.Settings.Waveform.DrawGridLines,
                WaveformColor = Se.Settings.Waveform.WaveformColor.FromHexToColor(),
                WaveformBackgroundColor = Se.Settings.Waveform.WaveformBackgroundColor.FromHexToColor(),
                WaveformSelectedColor = Se.Settings.Waveform.WaveformSelectedColor.FromHexToColor(),
                WaveformCursorColor = Se.Settings.Waveform.WaveformCursorColor.FromHexToColor(),
                WaveformFancyHighColor = Se.Settings.Waveform.WaveformFancyHighColor.FromHexToColor(),
                ParagraphBackground = Se.Settings.Waveform.ParagraphBackground.FromHexToColor(),
                ParagraphSelectedBackground = Se.Settings.Waveform.ParagraphSelectedBackground.FromHexToColor(),
                InvertMouseWheel = Se.Settings.Waveform.InvertMouseWheel,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = double.NaN, // Auto height
                WaveformDrawStyle = GetWaveformDrawStyle(Se.Settings.Waveform.WaveformDrawStyle),
                MinGapSeconds = Se.Settings.General.MinimumMillisecondsBetweenLines / 1000.0,
                FocusOnMouseOver = Se.Settings.Waveform.FocusOnMouseOver,
            };
            vm.AudioVisualizer.VerticalAlignment = VerticalAlignment.Stretch;
            vm.AudioVisualizer.Height = double.NaN; // Auto height
            vm.AudioVisualizer.OnNewSelectionInsert += vm.AudioVisualizerOnNewSelectionInsert;
            vm.AudioVisualizer.OnVideoPositionChanged += vm.AudioVisualizerOnVideoPositionChanged;
            vm.AudioVisualizer.OnToggleSelection += vm.AudioVisualizerOnToggleSelection;
            //vm.AudioVisualizer.OnStatus += vm.AudioVisualizerOnStatus;
            vm.AudioVisualizer.OnParagraphDoubleTapped += vm.OnWaveformDoubleTapped;
            vm.AudioVisualizer.OnDeletePressed += vm.AudioVisualizerOnDeletePressed;
            vm.AudioVisualizer.PointerReleased += vm.ControlMacPointerReleased;
            vm.AudioVisualizer.IsReadOnly = Se.Settings.General.LockTimeCodes;
            vm.AudioVisualizer.WaveformHeightPercentage = Se.Settings.Waveform.SpectrogramCombinedWaveformHeight;

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
                Header = Se.Language.General.InsertAtPositionAndFocusTextBox,
                Command = vm.WaveformInsertAtPositionAndFocusTextBoxCommand,
            };
            flyout.Items.Add(insertNewMenuItem);
            vm.MenuItemAudioVisualizerInsertAtPosition = insertNewMenuItem;

            var pasteFromClipboardMenuItem = new MenuItem
            {
                Header = Se.Language.General.WaveformPasteFromClipboard,
                Command = vm.WaveformPasteFromClipboardCommand,
            };
            flyout.Items.Add(pasteFromClipboardMenuItem);
            vm.MenuItemAudioVisualizerPasteFromClipboardMenuItem = pasteFromClipboardMenuItem;

            var insertSubtitleFileAtPositionMenuItem = new MenuItem
            {
                Header = Se.Language.General.InsertSubtitleFileAtVideoPositionDotDotDot,
                Command = vm.InsertSubtitleFileAtVideoPositionCommand,
            };
            flyout.Items.Add(insertSubtitleFileAtPositionMenuItem);
            vm.MenuIteminsertSubtitleFileAtPositionMenuItem = insertSubtitleFileAtPositionMenuItem;

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
                Command = vm.SplitInWaveformCommand,
            };
            flyout.Items.Add(splitMenuItem);
            vm.MenuItemAudioVisualizerSplit = splitMenuItem;

            var splitAtPositionMenuItem = new MenuItem
            {
                Header = Se.Language.General.SplitLine,
                Command = vm.SplitAtPositionInWaveformCommand,
            };
            flyout.Items.Add(splitAtPositionMenuItem);
            vm.MenuItemAudioVisualizerSplitAtPosition = splitAtPositionMenuItem;

            var MergeWithPreviousMenuItem = new MenuItem
            {
                Header = Se.Language.General.MergeBefore,
                Command = vm.MergeWithLineBeforeCommand,
            };
            flyout.Items.Add(MergeWithPreviousMenuItem);
            vm.MenuItemAudioVisualizerMergeWithPrevious = MergeWithPreviousMenuItem;

            var MergeWithNextMenuItem = new MenuItem
            {
                Header = Se.Language.General.MergeAfter,
                Command = vm.MergeWithLineAfterCommand,
            };
            flyout.Items.Add(MergeWithNextMenuItem);
            vm.MenuItemAudioVisualizerMergeWithNext = MergeWithNextMenuItem;

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

            var separatorDisplayMode = new Separator();
            separatorDisplayMode.DataContext = vm;
            separatorDisplayMode.Bind(Separator.IsVisibleProperty, new Binding(nameof(vm.ShowWaveformDisplayModeSeparator)));
            flyout.Items.Add(separatorDisplayMode);

            var showOnlyWaveformMenuItem = new MenuItem
            {
                Header = Se.Language.Waveform.ShowOnlyWaveform,
                Command = vm.WaveformShowOnlyWaveformCommand,
            }.BindIsVisible(vm, nameof(vm.ShowWaveformOnlyWaveform));
            flyout.Items.Add(showOnlyWaveformMenuItem);

            var showOnlySpectrogramMenuItem = new MenuItem
            {
                Header = Se.Language.Waveform.ShowOnlySpectrogram,
                Command = vm.WaveformShowOnlySpectrogramCommand,
            }.BindIsVisible(vm, nameof(vm.ShowWaveformOnlySpectrogram));
            flyout.Items.Add(showOnlySpectrogramMenuItem);

            var showWaveformAndSpectrogramMenuItem = new MenuItem
            {
                Header = Se.Language.Waveform.ShowWaveformAndSpectrogram,
                Command = vm.WaveformShowWaveformAndSpectrogramCommand,
            }.BindIsVisible(vm, nameof(vm.ShowWaveformWaveformAndSpectrogram));
            flyout.Items.Add(showWaveformAndSpectrogramMenuItem);

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
            FontWeight = FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.PlayPauseHint, shortcuts, nameof(vm.TogglePlayPauseCommand)),
        };
        Attached.SetIcon(buttonPlay, IconNames.Play);
        vm.ButtonWaveformPlay = buttonPlay;

        var toggleButtonRepeat = new ToggleButton
        {
            DataContext = vm,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.IsRepeatOn)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 3, 0),
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.RepeatHint, shortcuts, nameof(vm.RepeatLineToggleCommand)),
        };
        Attached.SetIcon(toggleButtonRepeat, IconNames.Refresh);
        toggleButtonRepeat.IsCheckedChanged += (s, e) => vm.RepeatLineToggleCommand.Execute(null);

        var buttonNew = new Button
        {
            Margin = new Thickness(0, 0, 3, 0),
            Command = vm.WaveformInsertAtPositionAndFocusTextBoxCommand,
            FontWeight = FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.NewHint, shortcuts, nameof(vm.WaveformInsertAtPositionAndFocusTextBoxCommand)),
        };
        Attached.SetIcon(buttonNew, IconNames.Plus);

        var buttonSetStartAndOffsetTheRest = new Button { Margin = new Thickness(0, 0, 3, 0), Command = vm.WaveformSetStartAndOffsetTheRestCommand, FontWeight = FontWeight.Bold, [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SetStartAndOffsetTheRestHint, shortcuts, nameof(vm.WaveformSetStartAndOffsetTheRestCommand)), };
        Attached.SetIcon(buttonSetStartAndOffsetTheRest, IconNames.ArrowExpandRight);

        var buttonSetStart = new Button
        {
            Margin = new Thickness(0, 0, 3, 0),
            Command = vm.WaveformSetStartCommand,
            FontWeight = FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SetStartHint, shortcuts, nameof(vm.WaveformSetStartCommand)),
        };
        Attached.SetIcon(buttonSetStart, IconNames.RayStart);

        var buttonSetEnd = new Button
        {
            Margin = new Thickness(0, 0, 3, 0),
            Command = vm.WaveformSetEndCommand,
            FontWeight = FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SetEndHint, shortcuts, nameof(vm.WaveformSetEndCommand)),
        };
        Attached.SetIcon(buttonSetEnd, IconNames.RayEnd);

        var iconHorizontal = new Icon
        {
            Value = IconNames.ArrowLeftRightBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 4, 0)
        }.BindIsVisible(vm, nameof(vm.ShowWaveformHorizontalZoom));
        var sliderHorizontalZoom = new Slider
        {
            Minimum = 0.1,
            Maximum = 20.0,
            Width = 80,
            VerticalAlignment = VerticalAlignment.Center,
            Value = 1,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.ZoomHorizontalHint, shortcuts),
        }.BindIsVisible(vm, nameof(vm.ShowWaveformHorizontalZoom));
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
        }.BindIsVisible(vm, nameof(vm.ShowWaveformVerticalZoom));
        var sliderVerticalZoom = new Slider
        {
            Minimum = 0.1,
            Maximum = 20.0,
            Width = 80,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            Value = 1,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.ZoomVerticalHint, shortcuts),
        }.BindIsVisible(vm, nameof(vm.ShowWaveformVerticalZoom));
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
            Value = 0,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            Focusable = true,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.VideoPosition, shortcuts),
        }.BindIsVisible(vm, nameof(vm.ShowWaveformVideoPositionSlider));
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

        var labelSpeed = UiUtil.MakeLabel(Se.Language.General.Speed).BindIsVisible(vm, nameof(vm.ShowWaveformPlaybackSpeed));
        var comboBoxSpeed = new ComboBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            FontSize = 12,
            MaxHeight = 22,
            MinHeight = 22,
            Padding = new Thickness(2, 2, 0, 2),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
        }.BindIsVisible(vm, nameof(vm.ShowWaveformPlaybackSpeed));
        comboBoxSpeed.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.Speeds)));
        comboBoxSpeed.Bind(SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedSpeed)) { Mode = BindingMode.TwoWay });
        comboBoxSpeed.SelectionChanged += (s, e) =>
        {
            if (vm.AudioVisualizer != null && comboBoxSpeed.SelectedItem is string s1 && s1.EndsWith("x") &&
                double.TryParse(s1.Trim('x'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double speed))
            {
                vm.GetVideoPlayerControl()?.SetSpeed(speed);
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
        var menuItemResetZoom = new MenuItem
        {
            Header = string.Format(languageHints.ResetZoomAndSpeed, UiUtil.MakeShortcutsString(shortcuts, nameof(vm.ResetWaveformZoomAndSpeedCommand))),
            Command = vm.ResetWaveformZoomAndSpeedCommand,
        };
        flyoutMore.Items.Add(menuItemResetZoom);
        var menuItemHideControls = new MenuItem
        {
            Header = string.Format(languageHints.HideWaveformToolbar, string.Empty),
            Command = vm.HideWaveformToolbarCommand,
        };
        flyoutMore.Items.Add(menuItemHideControls);

        controlsPanel.Children.Add(buttonPlay);
        controlsPanel.Children.Add(toggleButtonRepeat);
        controlsPanel.Children.Add(buttonNew);
        controlsPanel.Children.Add(buttonSetStartAndOffsetTheRest);
        controlsPanel.Children.Add(buttonSetStart);
        controlsPanel.Children.Add(buttonSetEnd);
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

    public static WaveformDrawStyle GetWaveformDrawStyle(string waveformDrawStyle)
    {
        if (Enum.TryParse<WaveformDrawStyle>(waveformDrawStyle, ignoreCase: true, out var value))
        {
            return value;
        }

        return WaveformDrawStyle.Classic;
    }
}