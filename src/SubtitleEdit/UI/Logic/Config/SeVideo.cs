namespace Nikse.SubtitleEdit.Logic.Config;

public class SeVideo
{
    public SeVideoBurnIn BurnIn { get; set; } 
    public SeVideoTransparent Transparent { get; set; } 
    public SeVideoTextToSpeech TextToSpeech { get; set; }

    public SeVideo()
    {
        BurnIn = new();
        Transparent = new();
        TextToSpeech = new();
    }
}