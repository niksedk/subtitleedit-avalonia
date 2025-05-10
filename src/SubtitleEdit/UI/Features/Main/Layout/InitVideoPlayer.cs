using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using System;
using Nikse.SubtitleEdit.Controls;
using HanumanInstitute.LibMpv.Avalonia;
using Avalonia.Data;

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
                vm.MediaPlayerMpv = new HanumanInstitute.LibMpv.MpvContext();

                var mpvView = new MpvView
                {
                    Margin = new Thickness(0),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };
                vm.MpvView = mpvView;

                mpvView.Bind(MpvView.MpvContextProperty, new Binding(nameof(vm.MediaPlayerMpv)));

                var control = new VideoPlayerControl
                {
                    PlayerContent = mpvView,
                    Volume = 80,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };
                control.PlayCommand = vm.PlayPauseCommand;
                control.StopCommand = vm.StopCommand;
                control.FullScreenCommand = vm.VideoFullScreenCommand;
                control.ScreenshotCommand = vm.VideoScreenshotCommand;
                control.SettingsCommand = vm.VideoSettingsCommand;
                vm.VideoPlayerControl = control;
                control.PositionChanged += vm.VideoPlayerControlPositionChanged;
                control.VolumeChanged += vm.VideoPlayerControlVolumeChanged;

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
                        decorator.Child = null;
                }
                else if (parent is ContentControl contentControl)
                {
                    if (contentControl.Content == vm.VideoPlayerControl)
                        contentControl.Content = null;
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