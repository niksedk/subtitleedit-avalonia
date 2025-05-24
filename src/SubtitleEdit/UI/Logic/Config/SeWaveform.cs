namespace Nikse.SubtitleEdit.Logic.Config;

public class SeWaveform
{
    public bool CenterVideoPosition { get; set; } 
    public bool DrawGridLines { get; set; } 

    public SeWaveform()
    {
        DrawGridLines = true;
    }
}