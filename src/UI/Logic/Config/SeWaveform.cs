using Avalonia.Media;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeWaveform
{
    public bool CenterVideoPosition { get; set; }
    public bool DrawGridLines { get; set; }
    public bool FocusTextBoxAfterInsertNew { get; set; }
    public bool ShowToolbar { get; set; }
    public string WaveformColor { get; set; }
    public string WaveformSelectedColor { get; set; }
    public bool InvertMouseWheel { get; set; }
    public double ShotChangesSensitivity { get; set; }
    public string ShotChangesImportTimeCodeFormat { get; set; }
    public bool WaveformSnapToShotChanges { get; set; }
    public bool WaveformShotChangesAutoGenerate { get; set; }
    public int WaveformSnapToShotChangesPixels { get; set; }


    public SeWaveform()
    {
        DrawGridLines = true;
        FocusTextBoxAfterInsertNew = true;
        WaveformColor = Color.FromArgb(150, 100, 100, 100).FromColorToHex();
        WaveformSelectedColor = Color.FromArgb(150, 0, 120, 255).FromColorToHex();
        ShotChangesSensitivity = 0.4;
        ShotChangesImportTimeCodeFormat = "Seconds";
        WaveformSnapToShotChangesPixels = 8;
        WaveformSnapToShotChanges = true;
        WaveformShotChangesAutoGenerate = false;
    }
}