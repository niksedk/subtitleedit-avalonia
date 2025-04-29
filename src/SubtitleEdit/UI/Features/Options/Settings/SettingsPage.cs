using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsPage : UserControl
{
    private TextBox _searchBox;
    private StackPanel _contentPanel;
    private List<SettingsSection> _sections;
    private SettingsPageViewModel _vm;
    
    public SettingsPage()
    {
        _vm = new SettingsPageViewModel();
        _sections = CreateSections();

        _searchBox = new TextBox
        {
            Watermark = "Search settings...",
            Margin = new Thickness(10)
        };
        DockPanel.SetDock(_searchBox, Dock.Top);

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

        var dockPanel = new DockPanel
        {
            Children =
            {
                _searchBox,
                scrollViewer
            }
        };

        Content = dockPanel;

        UpdateVisibleSections("");

        _searchBox.TextChanged += (s, e) => UpdateVisibleSections(_searchBox.Text ?? string.Empty);
            
    }

    private void UpdateVisibleSections(string filter)
    {
        _contentPanel.Children.Clear();

        foreach (var section in _sections)
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
