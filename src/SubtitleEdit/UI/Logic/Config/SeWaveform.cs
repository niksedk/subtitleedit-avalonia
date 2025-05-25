using Avalonia.Media;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeWaveform
{
    public bool CenterVideoPosition { get; set; }
    public bool DrawGridLines { get; set; }
    public bool FocusTextBoxAfterInsertNew { get; set; }
    public bool ShowToolbar { get; internal set; }
    public string WaveformColor { get; set; }
    public string WaveformSelectedColor { get; set; }
    public bool InvertMouseWheel { get; set; }

    public SeWaveform()
    {
        DrawGridLines = true;
        FocusTextBoxAfterInsertNew = true;
        WaveformColor = Color.FromArgb(150, 144, 238, 144).FromColorToHex();
        WaveformSelectedColor = Color.FromArgb(210, 254, 10, 10).FromColorToHex();
    }
}