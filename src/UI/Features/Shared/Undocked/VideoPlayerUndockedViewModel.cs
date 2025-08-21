using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main.Layout;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared;

public partial class VideoPlayerUndockedViewModel : ObservableObject
{
    [ObservableProperty] private string _error;

    public Window? Window { get; set; }
    public Grid? OriginalParent { get; set; }
    public int OriginalRow { get; set; }
    public int OriginalColumn { get; set; }
    public int OriginalIndex { get; set; }
    public Grid? VideoPlayer { get; set; }
    public Main.MainViewModel? MainViewModel { get; set; }

    internal async Task Initialize(VideoPlayerControl originalVideoPlayerControl, Main.MainViewModel mainViewModel)
    {
        VideoPlayer = InitVideoPlayer.MakeLayoutVideoPlayer(mainViewModel);
        await mainViewModel.VideoPlayerControl!.Open(originalVideoPlayerControl.VideoPlayerInstance.FileName);

        MainViewModel = mainViewModel;
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        MainViewModel?.OnKeyDownHandler(sender, e);
    }

    internal void Onloaded(object? sender, RoutedEventArgs e)
    {
        Window!.Content = VideoPlayer;
    }

    internal void Redock()
    {
        return;

        if (OriginalParent is not null && VideoPlayer is not null)
        {
            if (Window != null)
            {
                Window.Content = null;
            }

            Grid.SetRow(VideoPlayer, OriginalRow);
            Grid.SetColumn(VideoPlayer, OriginalColumn);

            if (OriginalIndex >= 0 && OriginalIndex <= OriginalParent.Children.Count)
            {
                OriginalParent.Children.Insert(OriginalIndex, VideoPlayer);
            }
            else
            {
                OriginalParent.Children.Add(VideoPlayer);
            }
        }
    }
}
