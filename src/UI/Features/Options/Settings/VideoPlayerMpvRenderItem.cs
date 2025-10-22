using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.LibMpv.Avalonia;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class VideoPlayerMpvRenderItem : ObservableObject
{
    [ObservableProperty] private string _name;
    
    public string Code { get; set; } 

    public VideoPlayerMpvRenderItem()
    {
        Name = string.Empty;
        Code = string.Empty;    
    }

    public VideoPlayerMpvRenderItem(string name, string code) : this()
    {
        Name = name;
        Code = code;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<VideoPlayerMpvRenderItem> ListVideoPlayerMpvRenderItems()
    {
        return
        [
            new(name: Se.Language.Video.MpvRenderAuto, code: nameof(VideoRenderer.Auto)),
            new(name: Se.Language.Video.MpvRenderNative, code: nameof(VideoRenderer.Native)),
            new(name: Se.Language.Video.MpvRenderOpenGl, code: nameof(VideoRenderer.OpenGl)),
            new(name: Se.Language.Video.MpvRenderSoftware, code: nameof(VideoRenderer.Software))
        ];
    }
}