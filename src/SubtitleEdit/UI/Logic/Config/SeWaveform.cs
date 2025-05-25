namespace Nikse.SubtitleEdit.Logic.Config;

public class SeWaveform
{
    public bool CenterVideoPosition { get; set; } 
    public bool DrawGridLines { get; set; }
    public bool FocusTextBoxAfterInsertNew { get; set; }
    public bool ShowToolbar { get; internal set; }

    public SeWaveform()
    {
        DrawGridLines = true;
        FocusTextBoxAfterInsertNew = true;
    }
}