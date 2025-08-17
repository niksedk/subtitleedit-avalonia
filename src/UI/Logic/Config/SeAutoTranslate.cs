using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAutoTranslate
{
    public string AutoTranslateLastName { get; set; } = string.Empty;
    public string AutoTranslateLastSource { get; set; } = string.Empty;
    public string AutoTranslateLastTarget { get; set; } = string.Empty;
    public string ChatGptPrompt { get; set; }
    public string OllamaPrompt { get; set; }
    public string LmStudioPrompt { get; set; }
    public string AnthropicPrompt { get; set; }
    public string GroqPrompt { get; set; }
    public string OpenRouterPrompt { get; set; }
    public decimal RequestMaxBytes { get; set; }
    public decimal RequestDelaySeconds { get; set; }

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