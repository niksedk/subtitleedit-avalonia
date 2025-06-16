using System;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeBatchConvert
{
    public string[] ActiveFunctions { get; set; } = [];
    public string OutputFolder { get; set; }
    public bool Overwrite { get; set; }
    public string TargetFormat { get; set; }
    public string TargetEncoding { get; set; }

    public bool FormattingRemoveAll { get; set; }
    public bool FormattingRemoveItalic { get; set; }
    public bool FormattingRemoveUnderline { get; set; }
    public bool FormattingRemoveFontTags { get; set; }
    public bool FormattingRemoveColorTags { get; set; }
    public bool FormattingRemoveBold { get; set; }
    public bool FormattingRemoveAlignmentTags { get; set; }

    public double OffsetTimeCodesMilliseconds { get; set; }
    public bool OffsetTimeCodesForward { get; set; }

    public string AdjustVia { get; set; }
    public double AdjustDurationSeconds { get; set; }
    public int AdjustDurationPercentage { get; set; }
    public int AdjustDurationFixedMilliseconds { get; set; }
    public double AdjustOptimalCps { get; set; }
    public double AdjustMaxCps { get; set; }

    public double ChangeFrameRateFrom { get; set; }
    public double ChangeFrameRateTo { get; set; }
    public bool SaveInSourceFolder { get; set; }

    public SeBatchConvert()
    {
        OutputFolder = string.Empty;
        SaveInSourceFolder = true;
        TargetFormat = string.Empty;
        TargetEncoding = string.Empty;
        OffsetTimeCodesForward = true;
        AdjustVia = "Seconds"; //AdjustDurationType.Seconds.ToString();
        AdjustDurationSeconds = 0.1;
        AdjustDurationPercentage = 100;
        AdjustDurationFixedMilliseconds = 3000;
        ChangeFrameRateFrom = 23.976;
        ChangeFrameRateTo = 24;
    }
}