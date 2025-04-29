using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsPage : UserControl
{
    private TextBox _searchBox;
    private StackPanel _contentPanel;
    private List<SettingsSection> _sections;

    public SettingsPage()
    {
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
                    Items = { "English", "Danish", "Spanish" },
                    SelectedIndex = 0,
                    Width = 150
                }),
                new SettingsItem("Enable Logging", () => new CheckBox { IsChecked = true })
            }),

            new SettingsSection("Appearance", new[]
            {
                new SettingsItem("Theme", () => new ComboBox
                {
                    Items = { "Light", "Dark" },
                    SelectedIndex = 0,
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
