using ApvPlayer.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using HanumanInstitute.LibMpv.Avalonia;
using HanumanInstitute.MediaPlayer.Avalonia.Mpv;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using System;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class InitVideoPlayer
{
    public static Grid MakeLayoutVideoPlayer(MainView mainPage, MainViewModel vm)
    {
        // Create main layout grid
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*"),  // Simplified to 3 rows
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(0),
        };


        if (true)
        {
            // Video player area
            if (vm.MpvView == null)
            {
                //var player = new HanumanInstitute.MediaPlayer.Avalonia.MediaPlayer();
                // var mpvPlayerHost = new MpvPlayerHost
                // {
                //     Source = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4",
                //     Volume = 80,
                //     Loop = true
                // };

                var mpvPlayerHost = new Border
                {
                 Background = Brushes.Black,
                  HorizontalAlignment = HorizontalAlignment.Stretch,
                  VerticalAlignment = VerticalAlignment.Stretch,
                };

                var control = new VideoPlayerControl
                {
                    PlayerContent = mpvPlayerHost,
                    Volume = 80,
                    Duration = 300 // e.g., 5 minutes
                };



                Grid.SetRow(control, 0);
                mainGrid.Children.Add(control);
                return mainGrid;
            }

        }

        if (OperatingSystem.IsWindows() && false)
        {
            // Video player area
            if (vm.LibVLC == null || vm.VideoPlayer == null)
            {
                vm.MediaPlayer?.Dispose();
                vm.LibVLC?.Dispose();
                vm.LibVLC = new LibVLC();
                vm.MediaPlayer = new MediaPlayer(vm.LibVLC);
                vm.VideoPlayer = new VideoView
                {
                    Margin = new Thickness(0),
                    MediaPlayer = vm.MediaPlayer,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };
            }
            else
            {
                var grid = vm.VideoPlayer.Parent as Grid;
                grid?.Children.Remove(vm.VideoPlayer);
            }

            Grid.SetRow(vm.VideoPlayer, 0);
            mainGrid.Children.Add(vm.VideoPlayer);
        }

    

        return mainGrid;
    }

    // Helper method to create a button with an image
    private static Button CreateButtonWithIcon(string iconPath)
    {
        return new Button
        {
            Content = new Image
            {
                Source = new Bitmap(iconPath),
                Width = 32,
                Height = 32,
            },
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            Margin = new Thickness(0),
        };
    }
}