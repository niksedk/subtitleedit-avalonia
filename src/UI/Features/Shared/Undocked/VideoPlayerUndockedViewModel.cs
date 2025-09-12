using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main.Layout;
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

    internal async Task Initialize(VideoPlayerControl originalVideoPlayerControl, Main.MainViewModel mainViewModel)
    {
        VideoPlayer = InitVideoPlayer.MakeLayoutVideoPlayer(mainViewModel);
        if (mainViewModel.VideoPlayerControl is VideoPlayerControl videoPlayerControl)
        {
            videoPlayerControl.FullScreenIsVisible = false;
            await videoPlayerControl.Open(originalVideoPlayerControl.VideoPlayerInstance.FileName);
        }

        MainViewModel = mainViewModel;
    }

    internal void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!AllowClose)
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
        if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
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
    }
}
