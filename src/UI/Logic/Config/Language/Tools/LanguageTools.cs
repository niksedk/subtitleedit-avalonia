namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageTools
{
    public LanguageFixCommonErrors FixCommonErrors { get; set; } = new();
    public LanguageAdjustDisplayDurations AdjustDurations { get; set; } = new();
    public LanguageApplyDurationLimits ApplyDurationLimits { get; set; } = new();
    public LanguageApplyMinGaps ApplyMinGaps { get; set; } = new();
    public LanguageBridgeGaps BridgeGaps { get; set; } = new();
    public LanguageBatchConvert BatchConvert { get; set; } = new();
    public LanguageChangeCasing ChangeCasing { get; set; } = new();
    public LanguageJoinSubtitles JoinSubtitles { get; set; } = new();
    public LanguageSplitSubtitle SplitSubtitle { get; set; } = new();
    public LanguageSplitBreakLongLines SplitBreakLongLines { get; set; } = new();
    public LanguageMergeShortLines MergeShortLines { get; set; } = new();
    public LanguageMergeLineswithSameText MergeLinesWithSameText { get; set; } = new();
    public LanguageMergeLineswithSameTimeCodes MergeLinesWithSameTimeCodes { get; set; } = new();
    public LanguageNetflixCheckAndFix NetflixCheckAndFix { get; set; } = new();
    public string PickAlignmentTitle { get; set; }
    public string PickFontNameTitle { get; set; }
    public string ColorPickerTitle { get; set; }
    public string PickLayersTitle { get; set; }

    public LanguageTools()
    {
        PickAlignmentTitle = "Choose alignment";
        PickFontNameTitle = "Choose font name";
        ColorPickerTitle = "Choose color";
        PickLayersTitle = "Choose layers to display in audio waveform/specgtrogram";
    }
}