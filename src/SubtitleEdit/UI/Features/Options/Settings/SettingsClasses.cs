using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsSection
{
    private readonly string _title;
    private readonly List<SettingsItem> _items;
    private StackPanel? _panel;

    public bool IsVisible => _items.Any(i => i.IsVisible);

    public SettingsSection(string title, IEnumerable<SettingsItem> items)
    {
        _title = title;
        _items = items.ToList();
    }

    public void Filter(string filter)
    {
        foreach (var item in _items)
            item.Filter(filter);
    }

    public Control Build()
    {
        _panel = new StackPanel { Spacing = 6 };

        _panel.Children.Add(new TextBlock
        {
            Text = _title,
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 10, 0, 5)
        });

        foreach (var item in _items.Where(i => i.IsVisible))
        {
            _panel.Children.Add(item.Build());
        }

        return _panel;
    }
}

public class SettingsItem
{
    private readonly string _label;
    private readonly Control _control;
    public bool IsVisible { get; private set; } = true;

    public SettingsItem(string label, Control control)
    {
        _label = label;
        _control = control;
    }

    public void Filter(string filter)
    {
        IsVisible = string.IsNullOrWhiteSpace(filter) || _label.ToLower().Contains(filter.ToLower());
    }

    public Control Build()
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                new TextBlock { Text = _label, Width = 150, VerticalAlignment = VerticalAlignment.Center },
                _control
            }
        };
    }
}
