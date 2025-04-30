using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Logic;

public static class UiUtil
{
    public static Button MakeButton(string text, IRelayCommand command)
    {
        return new Button
        {
            Content = text,
            Margin = new Thickness(0),
            Padding = new Thickness(12, 6), // horizontal: 12, vertical: 6
            MinWidth = 80,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = command,
        };
    }
}