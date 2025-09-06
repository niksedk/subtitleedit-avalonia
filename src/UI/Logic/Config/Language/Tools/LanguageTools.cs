namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageTools
{
    public LanguageFixCommonErrors FixCommonErrors { get; set; } = new();
    public LanguageAdjustDisplayDurations AdjustDurations { get; set; } = new();
    public LanguageBridgeGaps BridgeGaps { get; set; } = new();
    public LanguageChangeCasing ChangeCasing { get; set; } = new();
    public LanguageJoinSubtitles JoinSubtitles { get; set; } = new();
    public LanguageSplitSubtitle SplitSubtitle { get; set; } = new();
    public LanguageMergeLineswithSameText MergeLineswithSameText { get; set; } = new();

    public LanguageTools()
    {

    }
}