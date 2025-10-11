using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;
using System;

namespace Nikse.SubtitleEdit.Features.Shared;

public class FullScreenVideoWindow : Window
{
    private DispatcherTimer? _mouseMoveDetectionTimer;
    private (int X, int Y) _lastCursorPosition;
    private (int X, int Y) _lastPointerMovedCursorPosition;
   
    public FullScreenVideoWindow(Controls.VideoPlayer.VideoPlayerControl videoPlayer, string videoFileName, Action onClose)
    {
        WindowState = WindowState.FullScreen;
        SystemDecorations = SystemDecorations.None;

        var grid = new Grid
        {
            Background = Brushes.Transparent // Enable hit testing for pointer events
        }.Children(videoPlayer);

        Content = grid;

        const int mouseMovementMinPixels = 20;

        // Poll for actual cursor position using platform APIs
        // This works regardless of Avalonia event handling or MPV
        _mouseMoveDetectionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _mouseMoveDetectionTimer.Tick += (s, e) =>
        {
            try
            {
                var cursorPos = CursorPositionHelper.GetCursorPosition();
                if (cursorPos.HasValue)
                {
                    if (Math.Abs(cursorPos.Value.X - _lastCursorPosition.X) > mouseMovementMinPixels ||
                        Math.Abs(cursorPos.Value.Y - _lastCursorPosition.Y) > mouseMovementMinPixels)
                    {
                        _lastCursorPosition = cursorPos.Value;
                        videoPlayer.NotifyUserActivity();
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        };

        // Keep these handlers as fallback if native APIs fail
        grid.PointerMoved += (_, e) =>
        {
            var pos = e.GetCurrentPoint(this);
            if (Math.Abs(pos.Position.X - _lastPointerMovedCursorPosition.X) > mouseMovementMinPixels ||
                Math.Abs(pos.Position.Y - _lastPointerMovedCursorPosition.Y) > mouseMovementMinPixels)
            {
                videoPlayer.NotifyUserActivity();
                _lastPointerMovedCursorPosition = ((int)pos.Position.X, (int)pos.Position.Y);
            }

            videoPlayer.IsFullScreen = true;
        };

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

            // Also notify on any key press
            videoPlayer.NotifyUserActivity();
        };

        videoPlayer.FullscreenCollapseRequested += () => { Close(); };

        Closing += (_, _) =>
        {
            _mouseMoveDetectionTimer?.Stop();
            _mouseMoveDetectionTimer = null;
            videoPlayer.FullscreenCollapseRequested -= () => Close();
            onClose?.Invoke();
        };

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
        Loaded += (_, _) =>
        {
            WindowState = WindowState.Maximized;
            WindowState = WindowState.FullScreen;

            // Start polling for cursor movement
            _mouseMoveDetectionTimer?.Start();

            if (OperatingSystem.IsMacOS() && !string.IsNullOrEmpty(videoFileName))
            {
                videoPlayer.Reload();
            }
        };
    }
}