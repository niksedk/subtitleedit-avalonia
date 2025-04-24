using Avalonia.Controls;
using Avalonia.Input;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class LayoutWindow : Window
{
    public LayoutWindow()
    {
        Title = "Choose layout";
        Width = 600;
        Height = 400;
        CanResize = false; 
        Content = new TextBlock { Text = "This is a new window!" };
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}
