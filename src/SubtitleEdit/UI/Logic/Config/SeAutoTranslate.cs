using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAutoTranslate
{
    public string AutoTranslateLastName { get; set; } = string.Empty;
    public string AutoTranslateLastSource { get; set; } = string.Empty;
    public string AutoTranslateLastTarget { get; set; } = string.Empty;
    public string ChatGptPrompt { get; internal set; }
    public string OllamaPrompt { get; internal set; }
    public string LmStudioPrompt { get; internal set; }
    public string AnthropicPrompt { get; internal set; }
    public string GroqPrompt { get; internal set; }
    public string OpenRouterPrompt { get; internal set; }
    public decimal RequestMaxBytes { get; internal set; }
    public decimal RequestDelaySeconds { get; internal set; }

    public SeAutoTranslate()
    {
        ChatGptPrompt = Configuration.Settings.Tools.ChatGptPrompt;
        OllamaPrompt = Configuration.Settings.Tools.OllamaPrompt;
        LmStudioPrompt = Configuration.Settings.Tools.LmStudioPrompt;
        AnthropicPrompt = Configuration.Settings.Tools.AnthropicPrompt;
        GroqPrompt = Configuration.Settings.Tools.GroqPrompt;
        OpenRouterPrompt = Configuration.Settings.Tools.OpenRouterPrompt;
        RequestMaxBytes = 1000;
    }
}