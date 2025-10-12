namespace Nikse.SubtitleEdit.Logic.Config.Language.Waveform;

public class LanguageWaveform
{
    public string GuessTimeCodes { get; set; }
    public string GuessTimeCodesScanBlockSize { get; set; }
    public string GuessTimeCodesScanBlockAverageMin { get; set; }
    public string GuessTimeCodesScanBlockAverageMax { get; set; }
    public string GuessTimeCodesSplitLongSubtitlesAt { get; set; }
    public string SeekSilence { get; set; }
    public string MinSilenceDurationSeconds { get; set; }
    public string MaxSilenceVolume { get; set; }
    public string GuessTimeCodesDotDotDot { get; set; }
    public string SeekSilenceDotDotDot { get; set; }
    public string ToggleShotChange { get; set; }

    public LanguageWaveform()
    {
        GuessTimeCodes = "Guess time codes";
        GuessTimeCodesDotDotDot = "Guess time codes...";
        GuessTimeCodesScanBlockSize = "Scan block size (ms):";
        GuessTimeCodesScanBlockAverageMin = "Scan block average minimum (% of max):";
        GuessTimeCodesScanBlockAverageMax = "Scan block average maximum (% of max):";
        GuessTimeCodesSplitLongSubtitlesAt = "Split long subtitles at (ms):";

        SeekSilence = "Seek silence";
        SeekSilenceDotDotDot = "Seek silence...";
        MinSilenceDurationSeconds = "Min. silence duration (seconds):";
        MaxSilenceVolume = "Max. silence volume (0.0 - 1.0):";
        ToggleShotChange = "Toggle shot change";
    }
}