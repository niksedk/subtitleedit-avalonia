using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class VideoPlayerItem : ObservableObject
{
    [ObservableProperty] private string _code;
    [ObservableProperty] private string _name;

    public VideoPlayerItem()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<VideoPlayerItem> ListVideoPlayerItem()
    {
        return
        [
            new VideoPlayerItem { Name = Se.Language.Options.Settings.MpvOpenGl, Code = "mpv-opengl" },
            new VideoPlayerItem { Name = Se.Language.Options.Settings.MpvSoftwareRendering, Code = "mpv-sw" }
        ];
    }
}