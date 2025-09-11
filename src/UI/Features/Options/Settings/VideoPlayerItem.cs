using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class VideoPlayerItem : ObservableObject
{
    [ObservableProperty] private string _name;

    public VideoPlayerItem()
    {
        Name = string.Empty;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<VideoPlayerItem> ListVideoPlayerItem()
    {
        var list = new List<VideoPlayerItem>();

        list.Add(new VideoPlayerItem { Name = "mpv" });

        //if (OperatingSystem.IsWindows())
        //{
        //    list.Add(new VideoPlayerItem { Name = "vlc" });
        //}

        return list;
    }
}