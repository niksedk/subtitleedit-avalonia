using System;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeVideo
{
    public SeVideoBurnIn BurnIn { get; set; } 
    public SeVideoTransparent Transparent { get; set; } 
    public SeVideoTextToSpeech TextToSpeech { get; set; }
    public string VideoPlayer { get; set; }
    public double Volume { get; set; }
    public bool ShowStopButton { get; set; }
    public bool ShowFullscreenButton { get; set; }
    public bool AutoOpen { get; set; }
    public string CutType { get; set; }
    public string ShowChangesFFmpegArguments { get; set; }
    public bool VideoPlayerDisplayTimeLeft { get; set; }

    public SeVideo()
    {
        BurnIn = new();
        Transparent = new();
        TextToSpeech = new();
        VideoPlayer = "mpv";
        Volume = 60;
        ShowStopButton = true;
        ShowFullscreenButton = true;
        AutoOpen = true;
        CutType = Features.Video.CutVideo.CutType.MergeSegments.ToString();
        ShowChangesFFmpegArguments = "-i \"{0}\" -vf \"select=gt(scene\\,{1}),showinfo\" -threads 0 -vsync vfr -f null -";
    }
}