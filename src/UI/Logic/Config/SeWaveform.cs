using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeWaveform
{
    public bool ShowToolbar { get; set; }
    public bool CenterVideoPosition { get; set; }
    public bool DrawGridLines { get; set; }
    public bool FocusTextBoxAfterInsertNew { get; set; }
    public int SpectrogramCombinedWaveformHeight { get; set; }
    public bool ShowWaveformVerticalZoom { get; set; }
    public bool ShowWaveformHorizontalZoom { get; set; }
    public bool ShowWaveformVideoPositionSlider { get; set; }
    public bool ShowWaveformPlaybackSpeed { get; set; }
    public string WaveformColor { get; set; }
    public string WaveformBackgroundColor { get; set; }
    public string WaveformSelectedColor { get; set; }
    public string WaveformCursorColor { get; set; }
    public string WaveformFancyHighColor { get; set; }
    public string ParagraphBackground { get; set; }
    public string ParagraphSelectedBackground { get; set; }
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
    public bool GenerateSpectrogram { get; set; }
    public string SpectrogramStyle { get; set; }
    public string WaveformDrawStyle { get; set; }
    public string LastDisplayMode { get; set; }
    public bool PauseOnSingleClick { get; set; }
    public bool CenterOnSingleClick { get; set; }
    public bool SingleClickSelectsSubtitle { get; set; }
    public string SingleClickSetSelectedStartOrEndModifier { get; set; }
    public string SingleClickSetSelectedOffsetModifier { get; set; }


    public SeWaveform()
    {
        ShowToolbar = true;
        DrawGridLines = false;
        FocusTextBoxAfterInsertNew = true;
        SpectrogramCombinedWaveformHeight = 50;
        WaveformColor = Color.FromArgb(255, 0, 70, 0).FromColorToHex();
        WaveformBackgroundColor = Color.FromArgb(255, 0, 0, 0).FromColorToHex();
        WaveformSelectedColor = Color.FromArgb(150, 0, 120, 255).FromColorToHex();
        WaveformCursorColor = Colors.Cyan.FromColorToHex();
        WaveformFancyHighColor = Colors.Orange.FromColorToHex();
        ParagraphBackground = Color.FromArgb(90, 70, 70, 70).FromColorToHex();
        ParagraphSelectedBackground = Color.FromArgb(90, 70, 70, 120).FromColorToHex();
        ShotChangesSensitivity = 0.4;
        ShotChangesImportTimeCodeFormat = "Seconds";
        SnapToShotChangesPixels = 8;
        SnapToShotChanges = true;
        ShotChangesAutoGenerate = false;
        ShowWaveformVerticalZoom = true;
        ShowWaveformHorizontalZoom = true;
        ShowWaveformVideoPositionSlider = true;
        ShowWaveformPlaybackSpeed = true;
        SpectrogramStyle = SeSpectrogramStyle.Classic.ToString();
        LastDisplayMode = WaveformDisplayMode.OnlyWaveform.ToString();
        WaveformDrawStyle = Controls.AudioVisualizerControl.WaveformDrawStyle.Fancy.ToString();
        PauseOnSingleClick = true;
        CenterOnSingleClick = false;
        SingleClickSetSelectedStartOrEndModifier = "Shift";
        SingleClickSetSelectedOffsetModifier = "Alt";

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