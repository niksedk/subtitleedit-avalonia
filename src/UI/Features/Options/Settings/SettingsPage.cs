using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsPage : UserControl
{
    private readonly TextBox _searchBox;
    private readonly StackPanel _contentPanel;
    private readonly SettingsViewModel _vm;

    public SettingsPage(SettingsViewModel vm)
    {
        _vm = vm;
        _vm.Sections = CreateSections();

        _searchBox = new TextBox
        {
            Watermark = Se.Language.Options.Settings.SearchSettingsDotDoDot,
            Margin = new Thickness(10),
            MaxWidth = 500,
            MinWidth = 360,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };

        _contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 15,
            Margin = new Thickness(10)
        };

        var scrollViewer = new ScrollViewer
        {
            Content = _contentPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        _vm.ScrollView = scrollViewer;

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("Auto,*")
        };
        grid.Children.Add(_searchBox);
        Grid.SetRow(_searchBox, 0);
        Grid.SetColumn(_searchBox, 1);
        Grid.SetColumnSpan(_searchBox, 2);


        Content = grid;

        var menu = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Margin = new Thickness(10, 10, 40, 10),
            Children =
            {
                MakeMenuItem(Se.Language.General.Rules, vm.ScrollToSectionCommand, IconNames.PoliceBadge),
                MakeMenuItem(Se.Language.General.General, vm.ScrollToSectionCommand,  IconNames.Cogs),
                MakeMenuItem(Se.Language.General.SubtitleFormats, vm.ScrollToSectionCommand, IconNames.ClosedCaption),
                MakeMenuItem(Se.Language.Options.Settings.SyntaxColoring, vm.ScrollToSectionCommand, IconNames.Palette),
                MakeMenuItem(Se.Language.General.VideoPlayer, vm.ScrollToSectionCommand, IconNames.PlayBox),
                MakeMenuItem(Se.Language.Options.Settings.WaveformSpectrogram, vm.ScrollToSectionCommand,  IconNames.Waveform),
                MakeMenuItem(Se.Language.General.Tools, vm.ScrollToSectionCommand,  IconNames.Tools),
                MakeMenuItem(Se.Language.General.Appearance, vm.ScrollToSectionCommand,  IconNames.EyeSettings),
                MakeMenuItem(Se.Language.General.Toolbar, vm.ScrollToSectionCommand,  IconNames.DotsHorizontal),
                MakeMenuItem(Se.Language.Options.Settings.Network, vm.ScrollToSectionCommand,  IconNames.Network),
            }
        };

        if (OperatingSystem.IsWindows())
        {
            menu.Children.Add(MakeMenuItem(Se.Language.Options.Settings.FileTypeAssociations, vm.ScrollToSectionCommand, IconNames.FileCog));
        }

        menu.Children.Add(MakeMenuItem(Se.Language.Options.Settings.FilesAndLogs, vm.ScrollToSectionCommand, IconNames.FileMultiple));

        grid.Children.Add(menu);
        Grid.SetRow(menu, 1);
        Grid.SetColumn(menu, 0);

        grid.Children.Add(scrollViewer);
        Grid.SetRow(scrollViewer, 1);
        Grid.SetColumn(scrollViewer, 1);

        var buttonReset = UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetAllSettingsCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.CommandOkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonReset, buttonOk, buttonCancel);
        grid.Children.Add(buttonBar);
        Grid.SetRow(buttonBar, 2);
        Grid.SetColumn(buttonBar, 0);
        Grid.SetColumnSpan(buttonBar, 2);

        UpdateVisibleSections(string.Empty);

        _searchBox.TextChanged += (_, e) => UpdateVisibleSections(_searchBox.Text ?? string.Empty);
    }

    public static Button MakeMenuItem(string text, IRelayCommand command, string iconName)
    {
        var commandParameter = text;
        var label = new Label { Content = text, Padding = new Thickness(4, 0, 0, 0) };
        var image = new ContentControl();
        Attached.SetIcon(image, iconName);
        var stackPanelApplyFixes = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { image, label }
        };

        var link = new Button
        {
            Content = stackPanelApplyFixes,
            FontWeight = FontWeight.DemiBold,
            Margin = new Thickness(0),
            Padding = new Thickness(10, 5, 10, 5),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Command = command,
            CommandParameter = commandParameter,
        };

        return link;
    }

    private void UpdateVisibleSections(string filter)
    {
        _contentPanel.Children.Clear();

        foreach (var section in _vm.Sections)
        {
            section.Filter(filter);
            if (section.IsVisible)
            {
                _contentPanel.Children.Add(section.Build());
            }
        }
    }

    private List<SettingsSection> CreateSections()
    {
        var sections = new List<SettingsSection>();

        sections.Add(new SettingsSection(Se.Language.General.Rules,
        [
            MakeNumericSettingInt(Se.Language.Options.Settings.SingleLineMaxLength, nameof(_vm.SingleLineMaxLength)),
            MakeNumericSetting(Se.Language.Options.Settings.OptimalCharsPerSec, nameof(_vm.OptimalCharsPerSec)),
            MakeNumericSetting(Se.Language.Options.Settings.MaxCharsPerSec, nameof(_vm.MaxCharsPerSec)),
            MakeNumericSetting(Se.Language.Options.Settings.MaxWordsPerMin, nameof(_vm.MaxWordsPerMin)),
            MakeNumericSettingInt(Se.Language.Options.Settings.MinDurationMs, nameof(_vm.MinDurationMs)),
            MakeNumericSettingInt(Se.Language.Options.Settings.MaxDurationMs, nameof(_vm.MaxDurationMs)),
            MakeNumericSettingInt(Se.Language.Options.Settings.MinGapMs, nameof(_vm.MinGapMs)),
            MakeNumericSettingInt(Se.Language.Options.Settings.MaxLines, nameof(_vm.MaxLines)),
        ]));

        sections.Add(new SettingsSection(Se.Language.General.General,
        [
            MakeNumericSettingInt(Se.Language.Options.Settings.NewEmptyDefaultMs, nameof(_vm.NewEmptyDefaultMs)),
            MakeCheckboxSetting(Se.Language.Options.Settings.PromptDeleteLines, nameof(_vm.PromptDeleteLines)),
            MakeCheckboxSetting(Se.Language.General.LockTimeCodes, nameof(_vm.LockTimeCodes)),
            MakeCheckboxSetting(Se.Language.Options.Settings.RememberPositionAndSize, nameof(_vm.RememberPositionAndSize)),
            MakeSeparator(),
            MakeCheckboxSetting(Se.Language.Options.Settings.AutoBackupOn, nameof(_vm.AutoBackupOn)),
            MakeNumericSettingInt(Se.Language.Options.Settings.AutoBackupIntervalMinutes, nameof(_vm.AutoBackupIntervalMinutes)),
            MakeNumericSettingInt(Se.Language.Options.Settings.AutoBackupDeleteAfterMonths, nameof(_vm.AutoBackupDeleteAfterMonths)),
            new SettingsItem(Se.Language.Options.Settings.DefaultEncoding, () => new ComboBox
            {
                Width = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.Encodings)),
                [!SelectingItemsControl.SelectedItemProperty] =
                    new Binding(nameof(_vm.DefaultEncoding)) { Mode = BindingMode.TwoWay },
                ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                    new TextBlock { Text = f?.Name }, true)
            }),
        ]));

        sections.Add(new SettingsSection(Se.Language.General.SubtitleFormats,
        [
            new SettingsItem(Se.Language.Options.Settings.DefaultFormat, () => new ComboBox
            {
                Width = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.DefaultSubtitleFormats)),
                [!SelectingItemsControl.SelectedItemProperty] =
                    new Binding(nameof(_vm.SelectedDefaultSubtitleFormat)) { Mode = BindingMode.TwoWay },
                ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                    new TextBlock { Text = f?.Name }, true)
            }),

            new SettingsItem(Se.Language.Options.Settings.DefaultSaveAsFormat, () => new ComboBox
            {
                Width = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.SaveSubtitleFormats)),
                [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedSaveSubtitleFormat))
                    { Mode = BindingMode.TwoWay },
                ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                    new TextBlock { Text = f?.Name }, true)
            }),
        ]));

        sections.Add(new SettingsSection(Se.Language.Options.Settings.SyntaxColoring,
            [
                MakeCheckboxSetting(Se.Language.Options.Settings.ColorDurationTooShort, nameof(_vm.ColorDurationTooShort)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ColorDurationTooLong, nameof(_vm.ColorDurationTooLong)),
                MakeSeparator(),
                MakeCheckboxSetting(Se.Language.Options.Settings.ColorTextTooLong, nameof(_vm.ColorTextTooLong)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ColorTextTooWide, nameof(_vm.ColorTextTooWide)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ColorTextTooManyLines, nameof(_vm.ColorTextTooManyLines)),
                MakeSeparator(),
                MakeCheckboxSetting(Se.Language.Options.Settings.ColorOverlap, nameof(_vm.ColorOverlap)),
                MakeSeparator(),
                MakeCheckboxSetting(Se.Language.Options.Settings.ColorGapTooShort, nameof(_vm.ColorGapTooShort)),
                MakeSeparator(),
                new SettingsItem(Se.Language.Options.Settings.ErrorBackgroundColor, () => new ColorPicker()
                {
                    Width = 200,
                    IsAlphaEnabled = true,
                    IsAlphaVisible = true,
                    IsColorSpectrumSliderVisible = false,
                    IsColorComponentsVisible = true,
                    IsColorModelVisible = false,
                    IsColorPaletteVisible = false,
                    IsAccentColorsVisible = false,
                    IsColorSpectrumVisible = true,
                    IsComponentTextInputVisible = true,
                    [!ColorPicker.ColorProperty] = new Binding(nameof(_vm.ErrorColor))
                    {
                        Source = _vm,
                        Mode = BindingMode.TwoWay
                    },
                }),
            ]));

        sections.Add(new SettingsSection(Se.Language.General.VideoPlayer,
            [
                new SettingsItem(Se.Language.General.VideoPlayer, () => new StackPanel
                {
                    Children =
                    {
                        new ComboBox
                        {
                            Width = 200,
                            Height = 30,
                            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.VideoPlayers)),
                            [!SelectingItemsControl.SelectedItemProperty] =
                                new Binding(nameof(_vm.SelectedVideoPlayer)),
                            DataContext = _vm,
                            ItemTemplate = new FuncDataTemplate<object>((item, _) =>
                                new TextBlock
                                {
                                    [!TextBlock.TextProperty] = new Binding(nameof(VideoPlayerItem.Name)),
                                    Width = 150,
                                }, true)
                        }
                    }
                }),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowStopButton, nameof(_vm.ShowStopButton)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowFullscreenButton, nameof(_vm.ShowFullscreenButton)),
                MakeCheckboxSetting(Se.Language.Options.Settings.AutoOpenVideoFile, nameof(_vm.AutoOpenVideoFile)),
                new SettingsItem(Se.Language.Options.Settings.DownloadMpv, () => new StackPanel
                {
                    Children =
                    {
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 10,
                            Children =
                            {
                                UiUtil.MakeButton(Se.Language.General.Download, _vm.DownloadLibMpvCommand),
                                new TextBlock
                                {
                                    DataContext = _vm,
                                    [!TextBlock.TextProperty] = new Binding(nameof(_vm.LibMpvStatus)),
                                    Margin = new Thickness(0, 0, 0, 0),
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                }
                            }
                        },
                        new TextBlock
                        {
                            DataContext = _vm,
                            [!TextBlock.TextProperty] = new Binding(nameof(_vm.LibMpvPath)),
                            Margin = new Thickness(10, 0, 0, 0),
                            VerticalAlignment = VerticalAlignment.Center,
                            Opacity = 0.5,
                            FontSize = 10,
                        }
                    },
                    [!StackPanel.IsVisibleProperty] = new Binding(nameof(_vm.IsLibMpvDownloadVisible)) { Source = _vm }
                }),

            ]));

        sections.Add(new SettingsSection(Se.Language.Options.Settings.WaveformSpectrogram,
            [
                MakeCheckboxSetting(Se.Language.Options.Settings.WaveformFocusOnMouseOver, nameof(_vm.WaveformFocusOnMouseOver)),
                MakeCheckboxSetting(Se.Language.Options.Settings.WaveformCenterVideoPosition, nameof(_vm.WaveformCenterVideoPosition)),
                MakeCheckboxSetting(Se.Language.Options.Settings.WaveformShowToolbar, nameof(_vm.WaveformShowToolbar)),
                MakeCheckboxSetting(Se.Language.Options.Settings.WaveformSnapToShotChanges, nameof(_vm.WaveformSnapToShotChanges)),
                MakeCheckboxSetting(Se.Language.Options.Settings.WaveformShotChangesAutoGenerate, nameof(_vm.WaveformShotChangesAutoGenerate)),
                MakeCheckboxSetting(Se.Language.Options.Settings.WaveformFocusTextboxAfterInsertNew, nameof(_vm.WaveformFocusTextboxAfterInsertNew)),
                MakeCheckboxSetting(Se.Language.Options.Settings.WaveformInvertMouseWheel, nameof(_vm.WaveformInvertMouseWheel)),
                MakeCheckboxSetting(Se.Language.Options.Settings.WaveformDrawGridLines, nameof(_vm.WaveformDrawGridLines)),
                new SettingsItem(Se.Language.Options.Settings.WaveformColor, () => UiUtil.MakeColorPicker(_vm, nameof(_vm.WaveformColor))),
                new SettingsItem(Se.Language.Options.Settings.WaveformSelectedColor, () => UiUtil.MakeColorPicker(_vm, nameof(_vm.WaveformSelectedColor))),
                new SettingsItem(Se.Language.Options.Settings.WaveformCursorColor, () => UiUtil.MakeColorPicker(_vm, nameof(_vm.WaveformCursorColor))),
                new SettingsItem(Se.Language.Options.Settings.DownloadFfmpeg, () => new StackPanel
                {
                    Children =
                    {
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 10,
                            Children =
                            {
                                UiUtil.MakeButton(Se.Language.General.Download, _vm.DownloadFfmpegCommand),
                                new TextBlock
                                {
                                    DataContext = _vm,
                                    [!TextBlock.TextProperty] = new Binding(nameof(_vm.FfmpegStatus)),
                                    Margin = new Thickness(0, 0, 0, 0),
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                }
                            }
                        },
                        new TextBlock
                        {
                            DataContext = _vm,
                            [!TextBlock.TextProperty] = new Binding(nameof(_vm.FfmpegPath)),
                            Margin = new Thickness(10, 0, 0, 0),
                            VerticalAlignment = VerticalAlignment.Center,
                            Opacity = 0.5,
                            FontSize = 10,
                        }
                    }
                }),
            ]));

        sections.Add(new SettingsSection(Se.Language.General.Tools,
            [
                MakeCheckboxSetting(Se.Language.Options.Settings.GoToLineNumberSetsVideoPosition, nameof(_vm.GoToLineNumberAlsoSetVideoPosition)),
            ]));


        sections.Add(new SettingsSection(Se.Language.General.Appearance,
            [
                new SettingsItem(Se.Language.Options.Settings.Theme, () => UiUtil.MakeComboBox(_vm.Themes, _vm, nameof(_vm.SelectedTheme))),
                new SettingsItem(Se.Language.Options.Settings.DarkThemeBackgroundColor, () => UiUtil.MakeColorPicker(_vm, nameof(_vm.DarkModeBackgroundColor))),
                new SettingsItem(Se.Language.Options.Settings.UiFont, () => UiUtil.MakeComboBox(_vm.FontNames, _vm, nameof(_vm.SelectedFontName))),
                MakeNumericSetting(Se.Language.Options.Settings.TextBoxFontSize, nameof(_vm.TextBoxFontSize)),
                MakeCheckboxSetting(Se.Language.Options.Settings.TextBoxFontBold, nameof(_vm.TextBoxFontBold)),
                MakeCheckboxSetting(Se.Language.Options.Settings.TextBoxCenterText, nameof(_vm.TextBoxCenterText)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowButtonHints, nameof(_vm.ShowButtonHints)),
                MakeCheckboxSetting(Se.Language.Options.Settings.GridCompactMode, nameof(_vm.GridCompactMode)),
                new SettingsItem(Se.Language.Options.Settings.ShowGridLines, () => UiUtil.MakeComboBox(_vm.GridLinesVisibilities, _vm, nameof(_vm.SelectedGridLinesVisibility))),
                new SettingsItem(Se.Language.Options.Settings.BookmarkColor, () => UiUtil.MakeColorPicker(_vm, nameof(_vm.BookmarkColor))),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowAssaLayer, nameof(_vm.ShowAssaLayer)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowHorizontalLineAboveToolbar, nameof(_vm.ShowHorizontalLineAboveToolbar)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowHorizontalLineBelowToolbar, nameof(_vm.ShowHorizontalLineBelowToolbar)),
            ]));

        sections.Add(new SettingsSection(Se.Language.General.Toolbar,
            [
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowToolbarNew, nameof(_vm.ShowToolbarNew)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowToolbarOpen, nameof(_vm.ShowToolbarOpen)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowToolbarSave, nameof(_vm.ShowToolbarSave)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowToolbarSaveAs, nameof(_vm.ShowToolbarSaveAs)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowToolbarFind, nameof(_vm.ShowToolbarFind)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowToolbarReplace, nameof(_vm.ShowToolbarReplace)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowToolbarSpellCheck, nameof(_vm.ShowToolbarSpellCheck)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowToolbarSettings, nameof(_vm.ShowToolbarSettings)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowToolbarLayout, nameof(_vm.ShowToolbarLayout)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowToolbarHelp, nameof(_vm.ShowToolbarHelp)),
                MakeCheckboxSetting(Se.Language.Options.Settings.ShowToolbarEncoding, nameof(_vm.ShowToolbarEncoding)),
            ]));

        sections.Add(new SettingsSection(Se.Language.Options.Settings.Network,
            [
                new SettingsItem(Se.Language.Options.Settings.ProxyAddress, () => new TextBox { Width = 250 }),
                new SettingsItem(Se.Language.Options.Settings.Username, () => new TextBox { Width = 250 }),
                new SettingsItem(Se.Language.Options.Settings.Password, () => new TextBox { Width = 250 }),
            ]));

        sections.Add(new SettingsSection(Se.Language.Options.Settings.FileTypeAssociations,
            [
                new SettingsItem(string.Empty, () => new ItemsControl
                {
                    DataContext = _vm,
                    [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.FileTypeAssociations)),
                    ItemTemplate = new FuncDataTemplate<FileTypeAssociationViewModel>((fileType, _) =>
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 10,
                            Children =
                            {
                                new CheckBox
                                {
                                    [!ToggleButton.IsCheckedProperty] =new Binding(nameof(FileTypeAssociationViewModel.IsAssociated))
                                    {
                                        Source = fileType, Mode = BindingMode.TwoWay
                                    },
                                },
                                new Image
                                {
                                    Source = new Avalonia.Media.Imaging.Bitmap(
                                        AssetLoader.Open(new Uri(fileType.IconPath))),
                                    Width = 32,
                                    Height = 32,
                                    Margin = new Thickness(2),
                                },
                                new TextBlock
                                {
                                    Text = fileType.Extension,
                                    VerticalAlignment = VerticalAlignment.Center,
                                }
                            }
                        }, true)
                })
            ]));

        sections.Add(new SettingsSection(Se.Language.Options.Settings.FilesAndLogs,
            [
                new SettingsItem(Se.Language.Options.Settings.ShowErrorLogFile, () => UiUtil.MakeLink(Se.GetErrorLogFilePath(), _vm.ShowErrorLogFileCommand)),
                new SettingsItem(Se.Language.Options.Settings.ShowWhisperLogFile, () => UiUtil.MakeLink(Se.GetWhisperLogFilePath(), _vm.ShowWhisperLogFileCommand)),
                new SettingsItem(Se.Language.Options.Settings.ShowSettingsFile, () => UiUtil.MakeLink(Se.GetSettingsFilePath(), _vm.ShowSettingsFileCommand)),
            ]));


        return sections;
    }

    private static SettingsItem MakeSeparator()
    {
        return new SettingsItem(string.Empty, () => new Label());
    }

    private SettingsItem MakeNumericSetting(string label, string bindingProperty)
    {
        return new SettingsItem(label, () => new NumericUpDown
        {
            Width = 150,
            [!NumericUpDown.ValueProperty] = new Binding(bindingProperty) { Source = _vm, Mode = BindingMode.TwoWay },
        });
    }

    private SettingsItem MakeNumericSettingInt(string label, string bindingProperty)
    {
        return new SettingsItem(label, () => new NumericUpDown
        {
            Width = 150,
            FormatString = "F0",
            [!NumericUpDown.ValueProperty] = new Binding(bindingProperty) { Source = _vm, Mode = BindingMode.TwoWay },
        });
    }

    private SettingsItem MakeCheckboxSetting(string label, string bindingProperty)
    {
        return new SettingsItem(label, () => new CheckBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(bindingProperty) { Source = _vm, Mode = BindingMode.TwoWay }
        });
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        Dispatcher.UIThread.Invoke(() =>
        {
            _searchBox.Focus(); // hack to make OnKeyDown work
        });
    }
}