﻿using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
public class PiperVoice
{
    public string Voice { get; set; }
    public string Language { get; set; }
    public string Quality { get; set; }
    public string Model { get; set; }
    public string ModelShort => Model.Split('/').Last();

    public string Config { get; set; }
    public string ConfigShort => Config.Split('/').Last();

    public override string ToString()
    {
        return $"{Language} - {Voice} ({Quality})";
    }

    public PiperVoice(string voice, string language, string quality, string model, string config)
    {
        Voice = voice;
        Language = language;
        Quality = quality;
        Model = model;
        Config = config;
    }
}