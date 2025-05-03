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

    public SeAutoTranslate()
    {
    }
}