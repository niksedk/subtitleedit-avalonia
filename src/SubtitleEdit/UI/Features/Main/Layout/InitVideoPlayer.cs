using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

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

        if (OperatingSystem.IsWindows() && Se.Settings.Video.VideoPlayer.Equals("vlc", StringComparison.OrdinalIgnoreCase))
        {
            if (vm.VideoPlayerControl == null)
            {
                var videoPlayerInstance = new VideoPlayerInstanceVlc();
                var control = new VideoPlayerControl(videoPlayerInstance)
                {
                    PlayerContent = videoPlayerInstance.VideoViewVlc,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };
                control.FullScreenCommand = vm.VideoFullScreenCommand;
                vm.VideoPlayerControl = control;

                Grid.SetRow(control, 0);
                mainGrid.Children.Add(control);
            }
            else if (vm.VideoPlayerControl != null && vm.MpvView != null)
            {
                vm.VideoPlayerControl.RemoveControlFromParent();
                Grid.SetRow(vm.VideoPlayerControl!, 0);
                mainGrid.Children.Add(vm.VideoPlayerControl!);
            }
        }
        else 
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
            }
            else if (vm.VideoPlayerControl != null && vm.MpvView != null)
            {
                vm.VideoPlayerControl.RemoveControlFromParent();
                Grid.SetRow(vm.VideoPlayerControl!, 0);
                mainGrid.Children.Add(vm.VideoPlayerControl!);
            }
        }

        return mainGrid;
    }
}
