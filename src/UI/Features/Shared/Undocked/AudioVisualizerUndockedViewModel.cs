using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Shared;

public partial class AudioVisualizerUndockedViewModel : ObservableObject
{
    [ObservableProperty] private string _error = string.Empty;

    public Window? Window { get; set; }
    public Grid? OriginalParent { get; set; }
    public int OriginalRow { get; set; }
    public int OriginalColumn { get; set; }
    public int OriginalIndex { get; set; }
    public Grid? AudioVisualizer { get; set; }
    public Main.MainViewModel? MainViewModel { get; set; }
    public bool AllowClose { get; set; }

    internal void Initialize(AudioVisualizer audioVisualizer, Main.MainViewModel mainViewModel)
    {
        AudioVisualizer = InitWaveform.MakeWaveform(mainViewModel);
        mainViewModel.AudioVisualizer!.WavePeaks = audioVisualizer.WavePeaks;
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
        var videoPlayer = MainViewModel?.VideoPlayerControl;

        if (videoPlayer != null)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                videoPlayer.TogglePlayPause();
                return;
            }
            else if (e.Key == Key.Right)
            {
                e.Handled = true;
                videoPlayer.Position += 2;
                return;
            }
            else if (e.Key == Key.Left)
            {
                e.Handled = true;
                videoPlayer.Position -= 2;
                return;
            }
            else if (e.Key == Key.Up && e.KeyModifiers == KeyModifiers.None)
            {
                e.Handled = true;
                videoPlayer.Volume += 2;
                return;
            }
            else if (e.Key == Key.Down && e.KeyModifiers == KeyModifiers.None)
            {
                e.Handled = true;
                videoPlayer.Volume -= 2;
                return;
            }
        }

        MainViewModel?.OnKeyDownHandler(sender, e);
    }

    internal void Onloaded(object? sender, RoutedEventArgs e)
    {
        Window!.Content = AudioVisualizer;
        UiUtil.RestoreWindowPosition(Window);
    }
}
