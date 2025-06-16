using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Declarative;

namespace Nikse.SubtitleEdit.Features.Shared;

public class FullScreenVideoWindow : Window
{
    public FullScreenVideoWindow(Controls.VideoPlayer.VideoPlayerControl videoPlayer, Action onClose)
    {
        WindowState = WindowState.FullScreen;
        SystemDecorations = SystemDecorations.None;

        var grid = new Grid()
            .Children(videoPlayer);

        Content = grid;

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };

        videoPlayer.FullscreenCollapseRequested += () =>
        {
            Close();
        };

        Closing += (_, _) =>
        {
            videoPlayer.FullscreenCollapseRequested -= () => Close();
            onClose?.Invoke();
        };
        
        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }
}
