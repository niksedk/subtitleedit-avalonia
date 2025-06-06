using Nikse.SubtitleEdit.Logic.Config.Language.Tools;

namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class SeLanguage
{
    public LanguageSettings Settings { get; set; } = new();
    public LanguageEbuSaveOptions EbuSaveOptions { get; set; } = new();
    public LanguageBurnIn VideoBurnIn { get; set; } = new();
    public LanguageTransparentVideo VideoTransparent { get; set; } = new();
    public LanguageGeneral General { get; set; } = new();
    public LanguageTools Tools { get; set; } = new();
}