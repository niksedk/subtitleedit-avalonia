namespace Nikse.SubtitleEdit.Logic.Config;

public class SeTools
{
    public SeAudioToText AudioToText { get; set; } = new();
    public SeFixCommonErrors FixCommonErrors { get; set; } = new();
    public SeAdjustDisplayDurations AdjustDurations { get; set; } = new();
    public SeBridgeGaps BridgeGaps { get; set; } = new();
    public SeBatchConvert BatchConvert { get; set; } = new();
    public SeChangeCasing ChangeCasing { get; set; } = new();
    public SeRemoveTextForHi RemoveTextForHi { get; set; } = new();
    public string OllamaPrompt { get; set; }
    public string LmStudioPrompt { get; set; }
    public string AnthropicPrompt { get; set; }
    public string GroqPrompt { get; set; }
    public string OpenRouterPrompt { get; set; }
    public bool JoinKeepTimeCodes { get; set; }
    public int JoinAppendMilliseconds { get; set; }

    public SeTools()
    {
        OllamaPrompt = string.Empty;
        LmStudioPrompt = string.Empty;
        AnthropicPrompt = string.Empty;
        GroqPrompt = string.Empty;
        OpenRouterPrompt = string.Empty;
        JoinKeepTimeCodes = true;
    }
}