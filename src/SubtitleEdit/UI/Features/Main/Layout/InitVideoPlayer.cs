using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using System;
using Nikse.SubtitleEdit.Controls.VideoPlayer;

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
            if (vm.VideoPlayerControl == null)
            {
                var videoPlayerInstanceMpv = new VideoPlayerInstanceMpv();
                var control = new VideoPlayerControl(videoPlayerInstanceMpv)
                {
                    PlayerContent = videoPlayerInstanceMpv.MpvView,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };
                control.FullScreenCommand = vm.VideoFullScreenCommand;
                vm.VideoPlayerControl = control;

                Grid.SetRow(control, 0);
                mainGrid.Children.Add(control);
                return mainGrid;
            }
            else if (vm.VideoPlayerControl != null && vm.MpvView != null)
            {
                // Remove old mpvView if it exists
                var parent = vm.VideoPlayerControl?.Parent;

                if (parent is Panel panel)
                {
                    panel.Children.Remove(vm.VideoPlayerControl!);
                }
                else if (parent is Decorator decorator)
                {
                    if (decorator.Child == vm.VideoPlayerControl)
                    {
                        decorator.Child = null;
                    }
                }
                else if (parent is ContentControl contentControl)
                {
                    if (contentControl.Content == vm.VideoPlayerControl)
                    {
                        contentControl.Content = null;
                    }
                }
              
                Grid.SetRow(vm.VideoPlayerControl!, 0);
                mainGrid.Children.Add(vm.VideoPlayerControl!);
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