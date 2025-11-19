using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitVideoPlayer
{
    public static Grid MakeLayoutVideoPlayer(MainViewModel vm)
    {
        return MakeLayoutVideoPlayer(vm, out _);
    }

    public static Grid MakeLayoutVideoPlayer(MainViewModel vm, out VideoPlayerControl videoPlayerControl)
    {
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(0),
        };

        DragDrop.SetAllowDrop(mainGrid, true);
        mainGrid.AddHandler(DragDrop.DragOverEvent, vm.VideoOnDragOver, RoutingStrategies.Bubble);
        mainGrid.AddHandler(DragDrop.DropEvent, vm.VideoOnDrop, RoutingStrategies.Bubble);

        var control = MakeVideoPlayer();
        control.FullScreenCommand = vm.VideoFullScreenCommand;
        videoPlayerControl = control;
        vm.VideoPlayerControl = control;
        vm.VideoPlayerControl.Volume = Se.Settings.Video.Volume;
        vm.VideoPlayerControl.VideoPlayerDisplayTimeLeft = Se.Settings.Video.VideoPlayerDisplayTimeLeft;
        vm.VideoPlayerControl.VolumeChanged += v =>
        {
            Se.Settings.Video.Volume = v;
        };
        vm.VideoPlayerControl.ToggleDisplayProgressTextModeRequested += () =>
        {
            vm.ToggleVideoPlayerDisplayTimeLeftCommand.Execute(null);
        };

        Grid.SetRow(control, 0);
        mainGrid.Children.Add(control);

        return mainGrid;
    }

    public static VideoPlayerControl MakeVideoPlayer()
    {
        VideoPlayerControl control;

        try
        {
            //if (Se.Settings.Video.VideoPlayer.Equals("vlc", StringComparison.OrdinalIgnoreCase))
            //{
            //   var videoPlayerInstance = new VideoPlayerInstanceVlc();
            //   control = new VideoPlayerControl(videoPlayerInstance)
            //   {
            //       PlayerContent = videoPlayerInstance.VideoViewVlc,
            //       StopIsVisible = Se.Settings.Video.ShowStopButton,
            //       FullScreenIsVisible = Se.Settings.Video.ShowFullscreenButton,
            //   };
            //}
            //else
            if (Se.Settings.Video.VideoPlayer.Equals("mpv-sw", StringComparison.OrdinalIgnoreCase))
            {
                var libMpv = new LibMpvDynamicPlayer();
                var view = new LibMpvDynamicSoftwareControl(libMpv);
                control = new VideoPlayerControl(libMpv)
                {
                    PlayerContent = view,
                    StopIsVisible = Se.Settings.Video.ShowStopButton,
                    FullScreenIsVisible = Se.Settings.Video.ShowFullscreenButton,
                };
            }
            else // libmpv OpenGL
            {
                var libMpv = new LibMpvDynamicPlayer();
                var view = new LibMpvDynamicOpenGlControl(libMpv);
                control = new VideoPlayerControl(libMpv)
                {
                    PlayerContent = view,
                    StopIsVisible = Se.Settings.Video.ShowStopButton,
                    FullScreenIsVisible = Se.Settings.Video.ShowFullscreenButton,
                };
            }
        }
        catch
        {
            var videoPlayerInstanceNone = new VideoPlayerInstanceNone();
            control = new VideoPlayerControl(videoPlayerInstanceNone)
            {
                PlayerContent = new Label(),
                StopIsVisible = false,
                FullScreenIsVisible = false,
            };
        }

        control.VerticalAlignment = VerticalAlignment.Stretch;
        control.HorizontalAlignment = HorizontalAlignment.Stretch;

        return control;
    }
}
