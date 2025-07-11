﻿namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageAudioToText
{
    public string Title { get; set; }
    public string Transcribe { get; set; }
    public string TranslateToEnglish { get; set; }
    public string Transcribing { get; set; }
    public string TranscribingXOfY { get; internal set; }

    public LanguageAudioToText()
    {
        Title = "Audio to text";
        Transcribe = "Transcribe";
        TranslateToEnglish = "Translate to English";
        Transcribing = "Transcribing...";
        TranscribingXOfY = "Transcribing {0} of {1}...";
    }
}