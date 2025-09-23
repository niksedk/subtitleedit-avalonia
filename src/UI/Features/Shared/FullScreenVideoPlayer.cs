using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Declarative;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared;

public class FullScreenVideoWindow : Window
{
    public FullScreenVideoWindow(Controls.VideoPlayer.VideoPlayerControl videoPlayer, string videoFileName, Action onClose)
    {
        WindowState = WindowState.FullScreen;
        SystemDecorations = SystemDecorations.None;

        var grid = new Grid().Children(videoPlayer);

        Content = grid;

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.Key == Key.F11)
            {
                Close();
            }
            else if (e.Key == Key.Space)
            {
                videoPlayer.TogglePlayPause();
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
        Loaded += (_, _) =>
        {
                WindowState = WindowState.Maximized;

            if (OperatingSystem.IsMacOS() && !string.IsNullOrEmpty(videoFileName))
            {
                var position = videoPlayer.Position;
                videoPlayer.Close();
                Dispatcher.UIThread.Post(async void () =>
                {
                    try
                    {
                        Task.Delay(100).Wait();
                        await videoPlayer.Open(videoFileName);
                        Task.Delay(100).Wait();
                        videoPlayer.Position = position;
                    }
                    catch (Exception e)
                    {
                        Se.LogError(e, "Failed to reload video");
                    }
                });
            }
        };
    }
}
