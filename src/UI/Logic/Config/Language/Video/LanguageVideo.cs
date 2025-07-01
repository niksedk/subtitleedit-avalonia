namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageVideo
{
    public LanguageBurnIn BurnIn { get; set; } = new();
    public LanguageTransparentVideo VideoTransparent { get; set; } = new();
    public LanguageAudioToText AudioToText { get; set; } = new();
    public LanguageTextToSpeech TextToSpeech { get; set; } = new();


    public LanguageVideo()
    {

    }
}