using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using System;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic.VideoPlayers.MpvLogic;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class InitVideoPlayer
{
    public static Grid MakeLayoutVideoPlayer(MainView mainPage, MainViewModel vm)
    {
        // Create main layout grid
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*"), // Simplified to 3 rows
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(0),
        };

        if (true)
        {
            if (vm.MediaPlayerMpv == null)
            {
                vm.MediaPlayerMpv = new MpvVideoPlayer
                {
                    Margin = new Thickness(0),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };

                var control = new VideoPlayerControl
                {
                    PlayerContent = vm.MediaPlayerMpv,
                    Volume = 80,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };
                control.PlayCommand = vm.PlayPauseCommand;
                control.StopCommand = vm.StopCommand;
                control.FullScreenCommand = vm.VideoFullScreenCommand;
                control.ScreenshotCommand = vm.VideoScreenshotCommand;
                control.SettingsCommand = vm.VideoSettingsCommand;

                Grid.SetRow(control, 0);
                mainGrid.Children.Add(control);
                return mainGrid;
            }
        }

        if (OperatingSystem.IsWindows() && false)
        {
            // Video player area
            if (vm.LibVLC == null || vm.VideoViewVlc == null)
            {
                vm.MediaPlayerVlc?.Dispose();
                vm.LibVLC?.Dispose();
                vm.LibVLC = new LibVLC();
                vm.MediaPlayerVlc = new MediaPlayer(vm.LibVLC);
                vm.VideoViewVlc = new VideoView
                {
                    Margin = new Thickness(0),
                    MediaPlayer = vm.MediaPlayerVlc,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };
            }
            else
            {
                var grid = vm.VideoViewVlc.Parent as Grid;
                grid?.Children.Remove(vm.VideoViewVlc);
            }

            Grid.SetRow(vm.VideoViewVlc, 0);
            mainGrid.Children.Add(vm.VideoViewVlc);
        }


        return mainGrid;
    }
}