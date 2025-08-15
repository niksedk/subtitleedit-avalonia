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
        CutType = Nikse.SubtitleEdit.Features.Video.CutVideo.CutType.MergeSegments.ToString();
    }
}