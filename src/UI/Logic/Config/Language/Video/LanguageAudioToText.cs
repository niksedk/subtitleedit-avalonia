namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageAudioToText
{
    public string Title { get; set; }
    public string Transcribe { get; set; }
    public string TranslateToEnglish { get; set; }
    public string Transcribing { get; set; }
    public string TranscribingXOfY { get; set; }
    public string InputLanguage { get; set; }
    public string AdvancedWhisperSettings { get; set; }
    public string DownloadingWhisperEngine { get; set; }
    public string EnableVad { get; set; }

    public LanguageAudioToText()
    {
        Title = "Speech to text";
        Transcribe = "Transcribe";
        TranslateToEnglish = "Translate to English";
        Transcribing = "Transcribing...";
        TranscribingXOfY = "Transcribing {0} of {1}...";
        InputLanguage = "Input language";
        AdvancedWhisperSettings = "Advanced Whisper settings";
        DownloadingWhisperEngine = "Downloading Whisper engine";
        EnableVad = "Enable VAD";
    }
}