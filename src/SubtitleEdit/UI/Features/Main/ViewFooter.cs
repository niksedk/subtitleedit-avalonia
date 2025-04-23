using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Main;

public static class ViewFooter
{
    public static Border Make(MainViewModel vm)
    {
        return new Border
        {
            Background = Brushes.Black,
            Height = 25,
            Child = new TextBlock
            {
                Text = "Footer: 00:00:00 / 00:05:00 | EN subtitles | Saved",
                Margin = new Thickness(10, 4),
                VerticalAlignment = VerticalAlignment.Center
            }
        };
    }
    
}