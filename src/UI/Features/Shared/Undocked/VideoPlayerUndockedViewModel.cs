using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared;

public partial class VideoPlayerUndockedViewModel : ObservableObject
{
    [ObservableProperty] private string _error = string.Empty;

    public Window? Window { get; set; }
    public Grid? OriginalParent { get; set; }
    public int OriginalRow { get; set; }
    public int OriginalColumn { get; set; }
    public int OriginalIndex { get; set; }
    public Grid? VideoPlayer { get; set; }
    public Main.MainViewModel? MainViewModel { get; set; }
    public bool AllowClose { get; set; }

    private VideoPlayerControl? _videoPlayerControl;
    private DispatcherTimer? _mouseMoveDetectionTimer;
    private (int X, int Y) _lastCursorPosition;
    private (int X, int Y) _lastPointerMovedCursorPosition;

    [RelayCommand]
    private void ToggleFullScreen()
    {
        if (Window == null || _videoPlayerControl == null)
        {
            return;
        }

        if (Window.WindowState == WindowState.FullScreen)
        {
            Window.WindowState = WindowState.Normal;
            _videoPlayerControl.IsFullScreen = false;
        }
        else
        {
            Window.WindowState = WindowState.FullScreen;
            _videoPlayerControl.IsFullScreen = true;
        }
    }

    internal void Initialize(VideoPlayerControl originalVideoPlayerControl, Main.MainViewModel mainViewModel)
    {
        var originalVolume = originalVideoPlayerControl.Volume;
        VideoPlayer = InitVideoPlayer.MakeLayoutVideoPlayer(mainViewModel, out var videoPlayerControl);
        _videoPlayerControl = videoPlayerControl;
        _videoPlayerControl.FullScreenCommand = ToggleFullScreenCommand;
        _videoPlayerControl.FullscreenCollapseRequested += () => ToggleFullScreen();

        if (!string.IsNullOrEmpty(originalVideoPlayerControl.VideoPlayerInstance.FileName))
        {
            Dispatcher.UIThread.Post(async () =>
            {
                Task.Delay(100).Wait();
                await videoPlayerControl.Open(originalVideoPlayerControl.VideoPlayerInstance.FileName);
                videoPlayerControl.Volume = originalVolume;
            });
        }

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
                        _videoPlayerControl.NotifyUserActivity();
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        };

        // Keep these handlers as fallback if native APIs fail
        originalVideoPlayerControl.PointerMoved += (_, e) =>
        {
            var pos = e.GetCurrentPoint(originalVideoPlayerControl);
            if (Math.Abs(pos.Position.X - _lastPointerMovedCursorPosition.X) > mouseMovementMinPixels ||
                Math.Abs(pos.Position.Y - _lastPointerMovedCursorPosition.Y) > mouseMovementMinPixels)
            {
                _videoPlayerControl.NotifyUserActivity();
                _lastPointerMovedCursorPosition = ((int)pos.Position.X, (int)pos.Position.Y);
            }

            if (Window != null)
            {
                _videoPlayerControl.IsFullScreen = Window.WindowState == WindowState.FullScreen;
            }
        };

        MainViewModel = mainViewModel;
    }

    internal void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!AllowClose && e.CloseReason != WindowCloseReason.OwnerWindowClosing)
        {
            e.Cancel = true;

            if (Window != null)
            {
                Window.WindowState = WindowState.Minimized;
            }
        }
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Alt) || e.Key == Key.F11)
        {
            e.Handled = true;

            ToggleFullScreen();

            _videoPlayerControl?.NotifyUserActivity();
            return;
        }

        if (_videoPlayerControl != null)
        {
            if (e.Key == Key.Escape && Window is { } && Window.WindowState == WindowState.FullScreen)
            {
                ToggleFullScreen();
                e.Handled = true;
                _videoPlayerControl.NotifyUserActivity();
                return;
            }

            if (e.Key == Key.Space)
            {
                _videoPlayerControl.TogglePlayPause();
                e.Handled = true;
                _videoPlayerControl.NotifyUserActivity();
                return;
            }

            if (e.Key == Key.Right)
            {
                e.Handled = true;
                _videoPlayerControl.Position += 2;
                _videoPlayerControl.NotifyUserActivity();
                return;
            }

            if (e.Key == Key.Left)
            {
                e.Handled = true;
                _videoPlayerControl.Position -= 2;
                _videoPlayerControl.NotifyUserActivity();
                return;
            }

            if (e.Key == Key.Up && e.KeyModifiers == KeyModifiers.None)
            {
                e.Handled = true;
                _videoPlayerControl.Volume += 2;
                _videoPlayerControl.NotifyUserActivity();
                return;
            }

            if (e.Key == Key.Down && e.KeyModifiers == KeyModifiers.None)
            {
                e.Handled = true;
                _videoPlayerControl.Volume -= 2;
                _videoPlayerControl.NotifyUserActivity();
                return;
            }

            _videoPlayerControl.NotifyUserActivity();
        }

        MainViewModel?.OnKeyDownHandler(sender, e);
    }

    internal void Onloaded(object? sender, RoutedEventArgs e)
    {
        Window!.Content = VideoPlayer;
        UiUtil.RestoreWindowPosition(Window);
    }

    internal void OnKeyUp(object? sender, KeyEventArgs e)
    {
        MainViewModel?.OnKeyUpHandler(sender, e);
    }
}
