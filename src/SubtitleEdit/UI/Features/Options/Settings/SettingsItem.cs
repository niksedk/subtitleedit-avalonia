using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsItem
{
    private readonly string _label;
    private readonly Func<Control> _controlFactory;
    public bool IsVisible { get; private set; } = true;

    public SettingsItem(string label, Func<Control> controlFactory)
    {
        _label = label;
        _controlFactory = controlFactory;
    }

    public void Filter(string filter)
    {
        IsVisible = string.IsNullOrWhiteSpace(filter) ||
                    _label.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }

    public Control Build()
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(0, 0, 0, 15),
            Children =
            {
                new TextBlock
                {
                    Text = _label,
                    MinWidth = 200,
                    VerticalAlignment = VerticalAlignment.Top,
                },
                _controlFactory()
            }
        };
    }
}