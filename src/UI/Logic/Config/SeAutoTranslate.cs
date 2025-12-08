using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAutoTranslate
{
    public string AutoTranslateLastName { get; set; } = string.Empty;
    public string AutoTranslateLastSource { get; set; } = string.Empty;
    public string AutoTranslateLastTarget { get; set; } = string.Empty;
    public string ChatGptPrompt { get; set; }
    public string OllamaPrompt { get; set; }
    public string OllamaModel { get; set; }
    public string OllamaUrl { get; set; }
    public string LmStudioPrompt { get; set; }
    public string AnthropicPrompt { get; set; }
    public string GroqPrompt { get; set; }
    public string OpenRouterPrompt { get; set; }
    public decimal RequestMaxBytes { get; set; }
    public decimal RequestDelaySeconds { get; set; }
    public int CopyPasteMaxBlockSize { get; set; }
    public string CopyPasteLineSeparator { get; set; }
    public string LibreTranslateUrl { get; internal set; }
    public string LmStudioUrl { get; internal set; }
    public string NnlServeUrl { get; internal set; }
    public string NnlApiUrl { get; internal set; }
    public string LibreTranslateApiKey { get; internal set; }

    public SeAutoTranslate()
    {
        ChatGptPrompt = Configuration.Settings.Tools.ChatGptPrompt;
        OllamaPrompt = Configuration.Settings.Tools.OllamaPrompt;
        LmStudioPrompt = Configuration.Settings.Tools.LmStudioPrompt;
        AnthropicPrompt = Configuration.Settings.Tools.AnthropicPrompt;
        GroqPrompt = Configuration.Settings.Tools.GroqPrompt;
        OpenRouterPrompt = Configuration.Settings.Tools.OpenRouterPrompt;
        LibreTranslateUrl = "http://localhost:5000/";
        LibreTranslateApiKey = string.Empty;
        LmStudioUrl = "http://localhost:1234/v1/chat/completions/";
        NnlServeUrl = "http://127.0.0.1:6060/";
        NnlApiUrl = "http://localhost:7860/api/v4/";
        RequestMaxBytes = 1000;
        CopyPasteMaxBlockSize = 5000;
        CopyPasteLineSeparator = "(...)";
        OllamaModel = string.Empty;
        OllamaUrl = "http://localhost:11434/api/generate/";
    }
}