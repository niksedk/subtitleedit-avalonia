using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
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

    internal void Initialize(VideoPlayerControl originalVideoPlayerControl, Main.MainViewModel mainViewModel)
    {
        VideoPlayer = InitVideoPlayer.MakeLayoutVideoPlayer(mainViewModel);
        if (mainViewModel.VideoPlayerControl is VideoPlayerControl videoPlayerControl)
        {
            videoPlayerControl.FullScreenIsVisible = false;
            if (!string.IsNullOrEmpty(originalVideoPlayerControl.VideoPlayerInstance.FileName))
            {
                Dispatcher.UIThread.Post(async() =>
                {
                    Task.Delay(100).Wait();
                    await videoPlayerControl.Open(originalVideoPlayerControl.VideoPlayerInstance.FileName);
                });
            }
        }

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

            if (Window is { })
            {
                if (Window.WindowState == WindowState.FullScreen)
                {
                    Window.WindowState = WindowState.Normal;
                }
                else
                {
                    Window.WindowState = WindowState.FullScreen;
                }
            }

            return;
        }

        MainViewModel?.OnKeyDownHandler(sender, e);
    }

    internal void Onloaded(object? sender, RoutedEventArgs e)
    {
        Window!.Content = VideoPlayer;
        UiUtil.RestoreWindowPosition(Window);
    }
}
