using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeWindowPosition
{
    public string WindowName { get; set; }
    public bool IsFullScreen { get; set; }
    public bool IsMaximized { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public SeWindowPosition()
    {
        WindowName = string.Empty;
        IsFullScreen = false;
        IsMaximized = false;
    }

    public SeWindowPosition(string windowName, bool isFullScreen, bool isMaximized, int x, int y, int width, int height)
    {
        WindowName = windowName;
        IsFullScreen = isFullScreen;
        IsMaximized = isMaximized;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public static SeWindowPosition SaveState(Window? window)
    {
        if (window == null || window.Name == null)
        {
            return new SeWindowPosition();
        }

        var state = new SeWindowPosition
        {
            WindowName = window.Name,
            IsFullScreen = window.WindowState == WindowState.FullScreen,
            IsMaximized = window.WindowState == WindowState.Maximized,
            X = window.Position.X,
            Y = window.Position.Y,
            Width = (int)window.Width,
            Height = (int)window.Height
        };

        return state;
    }

    public void ApplyState(Window? window)
    {
        if (window == null)
        {
            return;
        }
        if (IsFullScreen)
        {
            window.WindowState = WindowState.FullScreen;
        }
        else if (IsMaximized)
        {
            window.WindowState = WindowState.Maximized;
        }
        else
        {
            window.WindowState = WindowState.Normal;
            window.Position = new Avalonia.PixelPoint(X, Y);
            window.Width = Width;
            window.Height = Height;
        }
    }

}
