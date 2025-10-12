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
    public string WaveformCursorColor { get; set; }
    public bool InvertMouseWheel { get; set; }
    public double ShotChangesSensitivity { get; set; }
    public string ShotChangesImportTimeCodeFormat { get; set; }
    public bool SnapToShotChanges { get; set; }
    public bool ShotChangesAutoGenerate { get; set; }
    public int SnapToShotChangesPixels { get; set; }
    public bool FocusOnMouseOver { get; set; }
    public bool GuessTimeCodeStartFromBeginning { get; set; }
    public int GuessTimeCodeScanBlockSize { get; set; }
    public int GuessTimeCodeScanBlockAverageMin { get; set; }
    public int GuessTimeCodeScanBlockAverageMax { get; set; }
    public int GuessTimeCodeSplitLongSubtitlesAtMs { get; set; }
    public double SeekSilenceMinDurationSeconds { get; set; }
    public double SeekSilenceMaxVolume { get; set; }
    public bool SeekSilenceSeekForward { get; set; }

    public SeWaveform()
    {
        DrawGridLines = true;
        FocusTextBoxAfterInsertNew = true;
        WaveformColor = Color.FromArgb(150, 100, 100, 100).FromColorToHex();
        WaveformSelectedColor = Color.FromArgb(150, 0, 120, 255).FromColorToHex();
        WaveformCursorColor = Colors.Cyan.FromColorToHex();
        ShotChangesSensitivity = 0.4;
        ShotChangesImportTimeCodeFormat = "Seconds";
        SnapToShotChangesPixels = 8;
        SnapToShotChanges = true;
        ShotChangesAutoGenerate = false;

        GuessTimeCodeStartFromBeginning = false;
        GuessTimeCodeScanBlockSize = 100;
        GuessTimeCodeScanBlockAverageMin = 35;
        GuessTimeCodeScanBlockAverageMax = 70;
        GuessTimeCodeSplitLongSubtitlesAtMs = 3500;

        SeekSilenceSeekForward = true;
        SeekSilenceMinDurationSeconds = 0.3;
        SeekSilenceMaxVolume = 0.1;
    }
}