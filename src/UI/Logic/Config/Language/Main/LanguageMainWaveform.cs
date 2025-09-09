namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageMainWaveform
{
    public string PlayPauseHint { get; set; }
    public string SetStartAndOffsetTheRestHint { get; set; }
    public string RepeatHint { get; set; }
    public string SetStartHint { get; set; }
    public string SetEndHint { get; set; }
    public string NewHint { get; set; }

    public LanguageMainWaveform()
    {
        PlayPauseHint = "Play / Pause {0}";
        SetStartAndOffsetTheRestHint = "Set start of current subtitle and offset the rest {0}";
        RepeatHint = "Repeat playing current selection {0}";
        SetStartHint = "Set start of current subtitle {0}";
        SetEndHint = "Set end of current subtitle {0}";
        NewHint = "Insert new subtitle at video position {0}";
    }
}