namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageVideo
{
    public LanguageBurnIn BurnIn { get; set; } = new();
    public LanguageTransparentVideo VideoTransparent { get; set; } = new();
    public LanguageAudioToText AudioToText { get; set; } = new();
    public LanguageTextToSpeech TextToSpeech { get; set; } = new();
    public string GoToVideoPosition { get; set; }
    public string GenerateBlankVideoDotDotDot { get; set; }
    public string GenerateBlankVideoTitle { get; set; }
    public string ReEncodeVideoForBetterSubtitlingTitle { get; set; }
    public string ReEncodeVideoForBetterSubtitlingDotDotDot { get; set; }
    public string CutVideoTitle { get; set; }
    public string CutVideoDotDotDot { get; set; }
    public string GenerateTimeCodes { get; set; }
    public string CheckeredImage { get; set; }

    public LanguageVideo()
    {

        GoToVideoPosition = "Go to video position";
        GenerateBlankVideoTitle = "Generate blank video...";
        GenerateBlankVideoDotDotDot = "Generate blank video...";
        ReEncodeVideoForBetterSubtitlingTitle = "Re-encode video for better subtitling";
        ReEncodeVideoForBetterSubtitlingDotDotDot = "Re-encode video for better subtitling...";
        CutVideoTitle = "Cut video";
        CutVideoDotDotDot = "Cut video...";
        GenerateTimeCodes = "Generate time codes";
        CheckeredImage = "Checkered image";
    }
}