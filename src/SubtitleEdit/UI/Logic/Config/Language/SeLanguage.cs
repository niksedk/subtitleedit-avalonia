﻿using Nikse.SubtitleEdit.Logic.Config.Language.Edit;
using Nikse.SubtitleEdit.Logic.Config.Language.File;
using Nikse.SubtitleEdit.Logic.Config.Language.Options;
using Nikse.SubtitleEdit.Logic.Config.Language.Tools;
using Nikse.SubtitleEdit.Logic.Config.Language.Translate;

namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class SeLanguage
{
    public string Title { get; set; } = "Subtitle Edit";
    public string Version { get; set; } = "Version 5.0.0";
    public string TranslatedBy { get; set; } = string.Empty;
    public string CultureName { get; set; } = "en-US";

    public LanguageGeneral General { get; set; } = new();
    public LanguageMain Main { get; set; } = new();
    public LanguageFile File { get; set; } = new();
    public LanguageEdit Edit { get; set; } = new();
    public LanguageTools Tools { get; set; } = new();
    public LanguageSpellCheck SpellCheck { get; set; } = new();
    public LanguageVideo Video { get; set; } = new();
    public LanguageSync Sync { get; set; } = new();
    public LanguageTranslate Translate { get; set; } = new();
    public LanguageOptions Options { get; set; } = new();
    public LanguageHelp Help { get; set; } = new();
}