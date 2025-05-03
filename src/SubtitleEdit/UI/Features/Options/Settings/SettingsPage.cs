using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

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
            Margin = new Thickness(10, 10, 20, 10),
            Children =
            {
                UiUtil.MakeLink("General", vm.ScrollToSectionCommand),
                UiUtil.MakeLink("Subtitle formats", vm.ScrollToSectionCommand),
                UiUtil.MakeLink("Syntax coloring", vm.ScrollToSectionCommand),
                UiUtil.MakeLink("Video player", vm.ScrollToSectionCommand),
                UiUtil.MakeLink("Waveform/spectrogram", vm.ScrollToSectionCommand),
                UiUtil.MakeLink("Tools", vm.ScrollToSectionCommand),
                UiUtil.MakeLink("Toolbar", vm.ScrollToSectionCommand),
                UiUtil.MakeLink("Appearance", vm.ScrollToSectionCommand),
                UiUtil.MakeLink("Network", vm.ScrollToSectionCommand),
                UiUtil.MakeLink("File type associations", vm.ScrollToSectionCommand),
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
                new SettingsItem("Language", () => new ComboBox
                {
                    DataContext = _vm,
                    [!ComboBox.ItemsSourceProperty] = new Binding(nameof(_vm.Languages)),
                    [!ComboBox.SelectedItemProperty] = new Binding(nameof(_vm.SelectedLanguage)) { Mode = BindingMode.TwoWay },
                    Width = 150
                }),
                new SettingsItem("Enable Logging", () => new CheckBox { IsChecked = true })
            }),

            new SettingsSection("Appearance", new[]
            {
                new SettingsItem("Theme", () => new ComboBox
                {
                    DataContext = _vm,
                    [!ComboBox.ItemsSourceProperty] = new Binding(nameof(_vm.Themes)),
                    [!ComboBox.SelectedItemProperty] = new Binding(nameof(_vm.SelectedTheme)) { Mode = BindingMode.TwoWay },
                    Width = 150
                }),
                new SettingsItem("Font Size", () => new Slider
                {
                    Minimum = 10,
                    Maximum = 30,
                    Value = 14,
                    Width = 150
                })
            }),

            new SettingsSection("Advanced", new[]
            {
                new SettingsItem("Developer Mode", () => new CheckBox { IsChecked = false }),
                new SettingsItem("Verbose Output", () => new CheckBox { IsChecked = false })
            })
        };
    }
}
