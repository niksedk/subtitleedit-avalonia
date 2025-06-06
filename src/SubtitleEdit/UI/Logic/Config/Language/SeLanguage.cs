using Nikse.SubtitleEdit.Logic.Config.Language.Edit;
using Nikse.SubtitleEdit.Logic.Config.Language.File;
using Nikse.SubtitleEdit.Logic.Config.Language.Tools;

namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class SeLanguage
{
    public LanguageGeneral General { get; set; } = new();
    public LanguageFile File { get; set; } = new();
    public LanguageEdit Edit { get; set; } = new();
    public LanguageTools Tools { get; set; } = new();
    public LanguageSpellCheck SpellCheck { get; set; } = new();
    public LanguageVideo Video { get; set; } = new();
    public LanguageSync Sync { get; set; } = new();
    public LanguageOptions Options { get; set; } = new();
    public LanguageHelp Help { get; set; } = new();

}