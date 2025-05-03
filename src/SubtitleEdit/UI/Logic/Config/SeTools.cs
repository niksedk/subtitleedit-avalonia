namespace Nikse.SubtitleEdit.Logic.Config;

public class SeTools
{
    public SeAudioToText AudioToText { get; set; } = new();
    public SeFixCommonErrors FixCommonErrors { get; set; } = new();
    public SeAdjustDisplayDurations AdjustDurations { get; set; } = new();
    public SeBatchConvert BatchConvert { get; set; } = new();
    public SeChangeCasing ChangeCasing { get; set; } = new();
    public SeRemoveTextForHi RemoveTextForHi { get; set; } = new();
    public SeAutoTranslate AutoTranslate { get; set; } = new();
    public string OllamaPrompt { get; internal set; }
    public string LmStudioPrompt { get; internal set; }
    public string AnthropicPrompt { get; internal set; }
    public string GroqPrompt { get; internal set; }
    public string OpenRouterPrompt { get; internal set; }
}