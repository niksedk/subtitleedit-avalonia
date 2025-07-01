namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageTextToSpeech
{
    public string Title { get; set; }
    public string TextToSpeechEngine { get; set; }

    public LanguageTextToSpeech()
    {
        Title = "Text to speech";
        TextToSpeechEngine = "Text to speech engine";
    }
}