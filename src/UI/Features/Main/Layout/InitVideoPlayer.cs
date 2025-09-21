using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitVideoPlayer
{
    public static Grid MakeLayoutVideoPlayer(MainViewModel vm)
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


        if (vm.VideoPlayerControl == null)
        {
            VideoPlayerControl control = MakeVideoPlayer();
            control.FullScreenCommand = vm.VideoFullScreenCommand;
            vm.VideoPlayerControl = control;
            vm.VideoPlayerControl.Volume = Se.Settings.Video.Volume;
            vm.VideoPlayerControl.VolumeChanged += (double v) =>
            {
                Se.Settings.Video.Volume = v;
            };

            Grid.SetRow(control, 0);
            mainGrid.Children.Add(control);
        }
        else
        {
            vm.VideoPlayerControl.RemoveControlFromParent();
            Grid.SetRow(vm.VideoPlayerControl!, 0);
            mainGrid.Children.Add(vm.VideoPlayerControl!);
            vm.VideoPlayerControl.IsVisible = true;
        }

        return mainGrid;
    }

    public static VideoPlayerControl MakeVideoPlayer()
    {
        VideoPlayerControl control;

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
        {
            try
            {
                var videoPlayerInstanceMpv = new VideoPlayerInstanceMpv();
                control = new VideoPlayerControl(videoPlayerInstanceMpv)
                {
                    PlayerContent = videoPlayerInstanceMpv.MpvView,
                    StopIsVisible = Se.Settings.Video.ShowStopButton,
                    FullScreenIsVisible = Se.Settings.Video.ShowFullscreenButton,
                };
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
        }

        control.VerticalAlignment = VerticalAlignment.Stretch;
        control.HorizontalAlignment = HorizontalAlignment.Stretch;

        return control;
    }
}
