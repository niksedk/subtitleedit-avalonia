using System;
using System.Collections.Generic;
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
                MakeMenuItem(Se.Language.General.Rules, vm.ScrollToSectionCommand, IconNames.MdiPoliceBadge),
                MakeMenuItem(Se.Language.General.General, vm.ScrollToSectionCommand,  IconNames.MdiCogs),
                MakeMenuItem(Se.Language.General.SubtitleFormats, vm.ScrollToSectionCommand, IconNames.MdiClosedCaption),
                MakeMenuItem(Se.Language.Options.Settings.SyntaxColoring, vm.ScrollToSectionCommand, IconNames.MdiPalette),
                MakeMenuItem(Se.Language.General.VideoPlayer, vm.ScrollToSectionCommand, IconNames.MdiPlayBox),
                MakeMenuItem(Se.Language.Options.Settings.WaveformSpectrogram, vm.ScrollToSectionCommand,  IconNames.MdiWaveform),
                MakeMenuItem(Se.Language.General.Tools, vm.ScrollToSectionCommand,  IconNames.MdiTools),
                MakeMenuItem(Se.Language.General.Toolbar, vm.ScrollToSectionCommand,  IconNames.MdiDotsHorizontal),
                MakeMenuItem(Se.Language.General.Appearance, vm.ScrollToSectionCommand,  IconNames.MdiEyeSettings),
                MakeMenuItem(Se.Language.Options.Settings.Network, vm.ScrollToSectionCommand,  IconNames.MdiNetwork),
                MakeMenuItem(Se.Language.Options.Settings.FileTypeAssociations, vm.ScrollToSectionCommand, IconNames.MdiFileCog),
            }
        };

        grid.Children.Add(menu);
        Grid.SetRow(menu, 1);
        Grid.SetColumn(menu, 0);

        grid.Children.Add(scrollViewer);
        Grid.SetRow(scrollViewer, 1);
        Grid.SetColumn(scrollViewer, 1);

        var buttonOk = UiUtil.MakeButtonOk(vm.CommandOkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);

        var buttonBar = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
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
        return new List<SettingsSection>
        {
            new SettingsSection(Se.Language.General.Rules,
            [
                MakeNumericSetting("Single line max length", nameof(_vm.SingleLineMaxLength)),
                MakeNumericSetting("Optimal chars/sec", nameof(_vm.OptimalCharsPerSec)),
                MakeNumericSetting("Max chars/sec", nameof(_vm.MaxCharsPerSec)),
                MakeNumericSetting("Max words/min", nameof(_vm.MaxWordsPerMin)),
                MakeNumericSetting("Min duration (ms)", nameof(_vm.MinDurationMs)),
                MakeNumericSetting("Max duration (ms)", nameof(_vm.MaxDurationMs)),
                MakeNumericSetting("Min gap (ms)", nameof(_vm.MinGapMs)),
                MakeNumericSetting("Max number of lines", nameof(_vm.MaxLines)),
            ]),

            new SettingsSection(Se.Language.General.General,
            [
                MakeNumericSetting("Default new subtitle duration (ms)", nameof(_vm.NewEmptyDefaultMs)),
                MakeCheckboxSetting("Prompt for delete lines", nameof(_vm.PromptDeleteLines)),
                MakeCheckboxSetting("Lock time codes", nameof(_vm.LockTimeCodes)),
                MakeCheckboxSetting("Remember window position and size", nameof(_vm.RememberPositionAndSize)),
                MakeSeparator(),
                MakeCheckboxSetting("Auto-backup", nameof(_vm.AutoBackupOn)),
                MakeNumericSetting("Auto-backup interval (minutes)", nameof(_vm.AutoBackupIntervalMinutes)),
                MakeNumericSetting("Auto-backup retention (months)", nameof(_vm.AutoBackupDeleteAfterMonths)),
                new SettingsItem("Default encoding", () => new ComboBox
                {
                    Width = 200,
                    DataContext = _vm,
                    [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.Encodings)),
                    [!SelectingItemsControl.SelectedItemProperty] =
                        new Binding(nameof(_vm.DefaultEncoding)) { Mode = BindingMode.TwoWay },
                    ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                        new TextBlock { Text = f?.Name }, true)
                }),
            ]),
            
            new SettingsSection(Se.Language.General.SubtitleFormats,
            [
                new SettingsItem("Default format", () => new ComboBox
                {
                    Width = 200,
                    DataContext = _vm,
                    [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.DefaultSubtitleFormats)),
                    [!SelectingItemsControl.SelectedItemProperty] =
                        new Binding(nameof(_vm.SelectedDefaultSubtitleFormat)) { Mode = BindingMode.TwoWay },
                    ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                        new TextBlock { Text = f?.Name }, true)
                }),

                new SettingsItem("Default save as format", () => new ComboBox
                {
                    Width = 200,
                    DataContext = _vm,
                    [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.SaveSubtitleFormats)),
                    [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedSaveSubtitleFormat))
                        { Mode = BindingMode.TwoWay },
                    ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                        new TextBlock { Text = f?.Name }, true)
                }),
            ]),

            new SettingsSection(Se.Language.Options.Settings.SyntaxColoring,
            [
                MakeCheckboxSetting("Color duration if too short", nameof(_vm.ColorDurationTooShort)),
                MakeCheckboxSetting("Color duration if too long", nameof(_vm.ColorDurationTooLong)),
                MakeSeparator(),
                MakeCheckboxSetting("Color text if too long", nameof(_vm.ColorTextTooLong)),
                MakeCheckboxSetting("Color text if too wide (pixels)", nameof(_vm.ColorTextTooWide)),
                MakeCheckboxSetting("Color text if more than 2 lines", nameof(_vm.ColorTextTooManyLines)),
                MakeSeparator(),
                MakeCheckboxSetting("Color time code overlap", nameof(_vm.ColorOverlap)),
                MakeSeparator(),
                MakeCheckboxSetting("Color if gap is too short", nameof(_vm.ColorGapTooShort)),
                MakeSeparator(),
                new SettingsItem("Error background color", () => new ColorPicker
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
            ]),

            new SettingsSection(Se.Language.General.VideoPlayer,
            [
                new SettingsItem("Video player", () => new StackPanel
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
                MakeCheckboxSetting("Show stop button", nameof(_vm.ShowStopButton)),
                MakeCheckboxSetting("Show fullscreen button", nameof(_vm.ShowFullscreenButton)),
                MakeCheckboxSetting("Auto-open video file when opening subtitle", nameof(_vm.AutoOpenVideoFile)),
                new SettingsItem("Download mpv", () => new StackPanel
                {
                    Children =
                    {
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 10,
                            Children =
                            {
                                UiUtil.MakeButton("Download", _vm.DownloadLibMpvCommand),
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

            ]),

            new SettingsSection(Se.Language.Options.Settings.WaveformSpectrogram,
            [
                MakeCheckboxSetting("Draw grid lines", nameof(_vm.WaveformDrawGridLines)),
                MakeCheckboxSetting("Center video position", nameof(_vm.WaveformCenterVideoPosition)),
                MakeCheckboxSetting("Show toolbar", nameof(_vm.WaveformShowToolbar)),
                MakeCheckboxSetting("Focus text box after insert", nameof(_vm.WaveformFocusTextboxAfterInsertNew)),
                MakeCheckboxSetting("Invert mouse-wheel", nameof(_vm.WaveformInvertMouseWheel)),
                new SettingsItem("Waveform color", () => UiUtil.MakeColorPicker(_vm, nameof(_vm.WaveformColor))),
                new SettingsItem("Waveform selected color", () => UiUtil.MakeColorPicker(_vm, nameof(_vm.WaveformSelectedColor))),
                new SettingsItem("Download ffmpeg", () => new StackPanel
                {
                    Children =
                    {
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 10,
                            Children =
                            {
                                UiUtil.MakeButton("Download", _vm.DownloadFfmpegCommand),
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
            ]),

            new SettingsSection(Se.Language.General.Tools,
            [
                new SettingsItem("TODO", () => new CheckBox { IsChecked = false }),
            ]),

            new SettingsSection("Toolbar",
            [
                MakeCheckboxSetting("Show new icon", nameof(_vm.ShowToolbarNew)),
                MakeCheckboxSetting("Show open icon", nameof(_vm.ShowToolbarOpen)),
                MakeCheckboxSetting("Show save icon", nameof(_vm.ShowToolbarSave)),
                MakeCheckboxSetting("Show save as icon", nameof(_vm.ShowToolbarSaveAs)),
                MakeCheckboxSetting("Show find icon", nameof(_vm.ShowToolbarFind)),
                MakeCheckboxSetting("Show replace icon", nameof(_vm.ShowToolbarReplace)),
                MakeCheckboxSetting("Show spell check icon", nameof(_vm.ShowToolbarSpellCheck)),
                MakeCheckboxSetting("Show settings icon", nameof(_vm.ShowToolbarSettings)),
                MakeCheckboxSetting("Show layout icon", nameof(_vm.ShowToolbarLayout)),
                MakeCheckboxSetting("Show help icon", nameof(_vm.ShowToolbarHelp)),
                MakeCheckboxSetting("Show encoding", nameof(_vm.ShowToolbarEncoding)),
                MakeCheckboxSetting("Show icon button hints", nameof(_vm.ShowToolbarHints)),
            ]),

            new SettingsSection(Se.Language.General.Appearance,
            [
                new SettingsItem("Theme", () => UiUtil.MakeComboBox(_vm.Themes, _vm, nameof(_vm.SelectedTheme))),
                MakeNumericSetting("Text box font size", nameof(_vm.TextBoxFontSize)),
                MakeCheckboxSetting("Text box font bold", nameof(_vm.TextBoxFontBold)),
            ]),

            new SettingsSection(Se.Language.Options.Settings.Network,
            [
                new SettingsItem("Proxy address", () => new TextBox { Width = 250 }),
                new SettingsItem("Username", () => new TextBox { Width = 250 }),
                new SettingsItem("Password", () => new TextBox { Width = 250 }),
            ]),

            new SettingsSection(Se.Language.Options.Settings.FileTypeAssociations,
            [
                new SettingsItem("Associate file types", () => new ItemsControl
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
            ])
        };
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