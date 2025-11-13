using System;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeVideo
{
    public SeVideoBurnIn BurnIn { get; set; } 
    public SeVideoTransparent Transparent { get; set; } 
    public SeVideoTextToSpeech TextToSpeech { get; set; }
    public string VideoPlayer { get; set; }
    public string VideoPlayerMpvRender { get; set; }
    public double Volume { get; set; }
    public bool ShowStopButton { get; set; }
    public bool ShowFullscreenButton { get; set; }
    public bool AutoOpen { get; set; }
    public bool OpenSearchParentFolder { get; set; }
    public string CutType { get; set; }
    public string ShowChangesFFmpegArguments { get; set; }
    public bool VideoPlayerDisplayTimeLeft { get; set; }
    public string CutDefaultVideoExtension { get; set; }
    public int MoveVideoPositionCustom1 { get; set; }
    public int MoveVideoPositionCustom2 { get; set; }

    public SeVideo()
    {
        BurnIn = new();
        Transparent = new();
        TextToSpeech = new();
        VideoPlayer = "mpv";
        VideoPlayerMpvRender = "auto";
        Volume = 60;
        ShowStopButton = true;
        ShowFullscreenButton = true;
        AutoOpen = true;
        OpenSearchParentFolder = true;
        CutType = Features.Video.CutVideo.CutType.MergeSegments.ToString();
        CutDefaultVideoExtension = ".mkv";
        ShowChangesFFmpegArguments = "-i \"{0}\" -vf \"select=gt(scene\\,{1}),showinfo\" -threads 0 -vsync vfr -f null -";
        MoveVideoPositionCustom1 = 5000; // 5 seconds
        MoveVideoPositionCustom2 = 10_000; // 10 seconds
    }
}