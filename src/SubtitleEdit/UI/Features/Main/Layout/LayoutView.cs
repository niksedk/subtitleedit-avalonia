using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class LayoutView : Window
{
    public LayoutView()
    {
        Title = "New Window";
        Width = 400;
        Height = 300;
        Content = new TextBlock { Text = "This is a new window!" };
    }
}
