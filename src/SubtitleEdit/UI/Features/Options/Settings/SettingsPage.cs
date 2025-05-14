using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class FormatViewModel
{
    public string Name { get; set; }
    public bool IsFavorite { get; set; }
}

public class FileTypeAssociationViewModel
{
    public string Extension { get; set; } = string.Empty;
    public bool IsAssociated { get; set; } = false;
    public string IconPath { get; set; } = string.Empty; // Optional: use if you have images per extension
}

public class SettingsPage : UserControl
{
    private TextBox _searchBox;
    private StackPanel _contentPanel;
    private SettingsViewModel _vm;

    public SettingsPage(SettingsViewModel vm)
    {
        _vm = vm;
        _vm.Sections = CreateSections();

        _searchBox = new TextBox
        {
            Watermark = "Search settings...",
            Margin = new Thickness(10)
        };

        //DockPanel.SetDock(_searchBox, Dock.Top);

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
                UiUtil.MakeMenuItem("General", vm.ScrollToSectionCommand, "General"),
                UiUtil.MakeMenuItem("Subtitle formats", vm.ScrollToSectionCommand, "Subtitle formats"),
                UiUtil.MakeMenuItem("Syntax coloring", vm.ScrollToSectionCommand, "Syntax coloring"),
                UiUtil.MakeMenuItem("Video player", vm.ScrollToSectionCommand, "Video player"),
                UiUtil.MakeMenuItem("Waveform/spectrogram", vm.ScrollToSectionCommand, "Waveform/spectrogram"),
                UiUtil.MakeMenuItem("Tools", vm.ScrollToSectionCommand, "Tools"),
                UiUtil.MakeMenuItem("Toolbar", vm.ScrollToSectionCommand, "Toolbar"),
                UiUtil.MakeMenuItem("Appearance", vm.ScrollToSectionCommand, "Appearance"),
                UiUtil.MakeMenuItem("Network", vm.ScrollToSectionCommand, "Network"),
                UiUtil.MakeMenuItem("File type associations", vm.ScrollToSectionCommand, "File type associations"),
            }
        };

        grid.Children.Add(menu);
        Grid.SetRow(menu, 1);
        Grid.SetColumn(menu, 0);

        grid.Children.Add(scrollViewer);
        Grid.SetRow(scrollViewer, 1);
        Grid.SetColumn(scrollViewer, 1);

        var buttonOK = UiUtil.MakeButton("OK", vm.CommandOkCommand);
        var buttonCancel = UiUtil.MakeButton("Cancel", vm.CommandCancelCommand);

        var buttonBar = UiUtil.MakeButtonBar(buttonOK, buttonCancel);
        grid.Children.Add(buttonBar);
        Grid.SetRow(buttonBar, 2);
        Grid.SetColumn(buttonBar, 0);
        Grid.SetColumnSpan(buttonBar, 2);

        UpdateVisibleSections(string.Empty);

        _searchBox.TextChanged += (s, e) => UpdateVisibleSections(_searchBox.Text ?? string.Empty);
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
            new SettingsSection("General", new[]
            {
                 // Rules
                MakeNumericSetting("Single line max length", nameof(_vm.SingleLineMaxLength)),
                MakeNumericSetting("Optimal chars/sec", nameof(_vm.OptimalCharsPerSec)),
                MakeNumericSetting("Max chars/sec", nameof(_vm.MaxCharsPerSec)),
                MakeNumericSetting("Max words/min", nameof(_vm.MaxWordsPerMin)),
                MakeNumericSetting("Min duration (ms)", nameof(_vm.MinDurationMs)),
                MakeNumericSetting("Max duration (ms)", nameof(_vm.MaxDurationMs)),
                MakeNumericSetting("Min gap (ms)", nameof(_vm.MinGapMs)),
                MakeNumericSetting("Max number of lines", nameof(_vm.MaxLines)),
                MakeNumericSetting("Unbreak subtitles shorter than (ms)", nameof(_vm.UnbreakShorterThanMs)),
            }),

            new SettingsSection("Subtitle formats", new[]
            {
                new SettingsItem("Default format", () => new ComboBox
                {
                    Width = 200,
                    DataContext = _vm,
                    [!ComboBox.ItemsSourceProperty] = new Binding(nameof(_vm.AvailableFormats)),
                    [!ComboBox.SelectedItemProperty] = new Binding(nameof(_vm.DefaultFormat)) { Mode = BindingMode.TwoWay },
                    ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                        new TextBlock { Text = f?.Name }, true)
                }),

                new SettingsItem("Default save as format", () => new ComboBox
                {
                    Width = 200,
                    DataContext = _vm,
                    [!ComboBox.ItemsSourceProperty] = new Binding(nameof(_vm.AvailableFormats)),
                    [!ComboBox.SelectedItemProperty] = new Binding(nameof(_vm.DefaultSaveAsFormat)) { Mode = BindingMode.TwoWay },
                    ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                        new TextBlock { Text = f?.Name }, true)
                }),

                new SettingsItem("Favorite formats", () => new ItemsControl
                {
                    DataContext = _vm,
                    [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.AvailableFormats)),
                    ItemTemplate = new FuncDataTemplate<FormatViewModel>((formatVm, _) =>
                        new CheckBox
                        {
                            Content = formatVm.Name,
                            [!CheckBox.IsCheckedProperty] = new Binding(nameof(FormatViewModel.IsFavorite)) { Source = formatVm, Mode = BindingMode.TwoWay }
                        }, true)
                })
            }),
            
            new SettingsSection("Syntax coloring", new[]
            {
                new SettingsItem("Developer Mode", () => new CheckBox { IsChecked = false }),
                new SettingsItem("Verbose Output", () => new CheckBox { IsChecked = false })
            }),
            
            new SettingsSection("Video player", new[]
            {
                new SettingsItem("Developer Mode", () => new CheckBox { IsChecked = false }),
                new SettingsItem("Verbose Output", () => new CheckBox { IsChecked = false })
            }),

            new SettingsSection("Waveform/spectrogram", new[]
            {
                new SettingsItem("Developer Mode", () => new CheckBox { IsChecked = false }),
                new SettingsItem("Verbose Output", () => new CheckBox { IsChecked = false })
            }),

            new SettingsSection("Tools", new[]
            {
                new SettingsItem("Developer Mode", () => new CheckBox { IsChecked = false }),
                new SettingsItem("Verbose Output", () => new CheckBox { IsChecked = false })
            }),

            new SettingsSection("Toolbar", new[]
            {
                MakeCheckboxSetting("Show New icon", nameof(_vm.ShowToolbarNew)),
                MakeCheckboxSetting("Show Open icon", nameof(_vm.ShowToolbarOpen)),
                MakeCheckboxSetting("Show Save icon", nameof(_vm.ShowToolbarSave)),
                MakeCheckboxSetting("Show Save As icon", nameof(_vm.ShowToolbarSaveAs)),
                MakeCheckboxSetting("Show Find icon", nameof(_vm.ShowToolbarFind)),
            }),

            new SettingsSection("Appearance", new[]
            {
                new SettingsItem("Theme", () =>
                UiUtil.MakeComboBox(_vm.Themes, _vm, nameof(_vm.SelectedTheme))),
                new SettingsItem("Font Size", () => new Slider
                {
                    Minimum = 10,
                    Maximum = 30,
                    Value = 14,
                    Width = 150
                })
            }),

            new SettingsSection("Network", new[]
            {
                new SettingsItem("Developer Mode", () => new CheckBox { IsChecked = false }),
                new SettingsItem("Verbose Output", () => new CheckBox { IsChecked = false })
            }),

            new SettingsSection("File type associations", new[]
            {
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
                                    [!CheckBox.IsCheckedProperty] = new Binding(nameof(FileTypeAssociationViewModel.IsAssociated))
                                    { Source = fileType, Mode = BindingMode.TwoWay }
                                },
                                new Image
                                {
                                    Source = new Avalonia.Media.Imaging.Bitmap(AssetLoader.Open(new Uri(fileType.IconPath))),
                                    Width = 32,
                                    Height = 32,
                                    Margin = new Thickness(2),                                    
                                },
                                new TextBlock
                                {
                                    Text = fileType.Extension,
                                    VerticalAlignment = VerticalAlignment.Center
                                }
                            }
                        }, true)
                })
            })
        };
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
            [!CheckBox.IsCheckedProperty] = new Binding(bindingProperty) { Source = _vm, Mode = BindingMode.TwoWay }
        });
    }
}
