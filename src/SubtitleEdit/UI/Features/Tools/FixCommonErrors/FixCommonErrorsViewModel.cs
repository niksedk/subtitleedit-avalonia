﻿using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Config.Language;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public partial class FixCommonErrorsViewModel : ObservableObject, IFixCallbacks
{
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private ObservableCollection<LanguageDisplayItem> _languages;
    [ObservableProperty] private LanguageDisplayItem? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<FixRuleDisplayItem> _fixRules;
    [ObservableProperty] private ObservableCollection<FixDisplayItem> _fixes;
    [ObservableProperty] private FixDisplayItem? _selectedFix;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _paragraphs;
    [ObservableProperty] private SubtitleLineViewModel? _selectedParagraph;
    [ObservableProperty] private string _editText;
    [ObservableProperty] private TimeSpan _editShow;
    [ObservableProperty] private TimeSpan _editDuration;
    [ObservableProperty] private ObservableCollection<string> _profiles;
    [ObservableProperty] private string? _selectedProfile;
    [ObservableProperty] private bool _step1IsVisible;
    [ObservableProperty] private bool _step2IsVisible;

    public DataGrid? Step1Grid { get; set; }
    public DataGrid? Step2Grid { get; set; }
    public FixCommonErrorsWindow? Window { get; set; }

    public bool OkPressed { get; private set; }
    public SubtitleFormat Format { get; set; } = new SubRip();
    public Encoding Encoding { get; set; } = Encoding.UTF8;
    public string Language { get; set; } = "en";

    private List<FixRuleDisplayItem> _allFixRules = new();
    private Subtitle _originalSubtitle = new();
    private Subtitle _fixSubtitle = new();
    private readonly LanguageFixCommonErrors _language;
    private int _totalFixes;
    private int _totalErrors;
    private bool _previewMode = true;
    private List<FixDisplayItem> _oldFixes = new();
    private LanguageDisplayItem _oldSelectedLanguage = new(CultureInfo.InvariantCulture, "English");
    private string? _oldSelectedProfile;

    private readonly INamesList _namesList;
    private readonly IWindowService _windowService;

    public FixCommonErrorsViewModel(INamesList namesList, IWindowService windowService)
    {
        _namesList = namesList;
        _windowService = windowService;

        SearchText = string.Empty;
        Languages = new ObservableCollection<LanguageDisplayItem>();
        Language = new string(' ', 0);
        FixRules = new ObservableCollection<FixRuleDisplayItem>();
        Fixes = new ObservableCollection<FixDisplayItem>();
        Paragraphs = new ObservableCollection<SubtitleLineViewModel>();
        EditText = string.Empty;
        _language = Se.Language.FixCommonErrors;
        Step1IsVisible = true;
        InitLanguage();

        Profiles = new ObservableCollection<string>(Se.Settings.Tools.FixCommonErrors.Profiles.Select(p => p.ProfileName));
        if (Profiles.Count == 0)
        {
            Profiles.Add("Default");
        }

        var profileName = Se.Settings.Tools.FixCommonErrors.LastProfileName;
        SelectedProfile = Profiles.Contains(profileName)
            ? profileName
            : Profiles.First();
    }

    private string InitLanguage()
    {
        var languages = new List<LanguageDisplayItem>();
        foreach (var ci in Utilities.GetSubtitleLanguageCultures(true))
        {
            languages.Add(new LanguageDisplayItem(ci, ci.EnglishName));
        }

        Languages = new ObservableCollection<LanguageDisplayItem>(languages.OrderBy(p => p.ToString()));

        var languageCode =
            LanguageAutoDetect.AutoDetectGoogleLanguage(_originalSubtitle); // Guess language based on subtitle contents
        Language = languageCode;

        SelectedLanguage = Languages.FirstOrDefault(p => p.Code.TwoLetterISOLanguageName == languageCode);
        if (SelectedLanguage != null)
        {
            SelectedLanguage = Languages.First(p => p.Code.TwoLetterISOLanguageName == "en");
        }

        return languageCode;
    }

    private void UpdateRulesSelection()
    {
        if (string.IsNullOrEmpty(SelectedProfile))
        {
            return;
        }

        var profile = Se.Settings.Tools.FixCommonErrors.Profiles.FirstOrDefault(p => p.ProfileName == SelectedProfile);
        if (profile == null)
        {
            return;
        }

        foreach (var rule in FixRules)
        {
            rule.IsSelected = profile.SelectedRules.Contains(rule.Name);
        }
    }

    private void SaveRulesSelection()
    {
        if (string.IsNullOrEmpty(SelectedProfile))
        {
            return;
        }

        SaveRulesSelection(SelectedProfile);
    }

    private void SaveRulesSelection(string profileName)
    {
        var profile = Se.Settings.Tools.FixCommonErrors.Profiles.FirstOrDefault(p => p.ProfileName == profileName);
        if (profile == null)
        {
            return;
        }

        profile.SelectedRules.Clear();
        foreach (var rule in FixRules)
        {
            if (rule.IsSelected)
            {
                profile.SelectedRules.Add(rule.Name);
            }
        }

        Se.Settings.Tools.FixCommonErrors.LastProfileName = profileName;
        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task ShowProfile()
    {
        await _windowService.ShowDialogAsync<FixCommonErrorsProfileWindow, FixCommonErrorsProfileViewModel>(Window!, vm => { });
    }

    [RelayCommand]
    public void DoRefreshFixes()
    {
        RefreshFixes();
    }

    [RelayCommand]
    private void DoApplyFixes()
    {
        _previewMode = false;
        ApplyFixes();

        RefreshFixes();
    }

    [RelayCommand]
    private void BackToFixList()
    {
        Step2IsVisible = false;
        Step1IsVisible = true;
    }

    [RelayCommand]
    private void ToApplyFixes()
    {
        Step1IsVisible = false;
        Step2IsVisible = true;

        SaveRulesSelection();
        _oldSelectedLanguage = SelectedLanguage!;
        _oldSelectedProfile = SelectedProfile!;

        ApplyFixes();
        _previewMode = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    public void RulesSelectAll()
    {
        foreach (var rule in FixRules)
        {
            rule.IsSelected = true;
        }
    }

    [RelayCommand]
    public void RulesInverseSelected()
    {
        foreach (var rule in FixRules)
        {
            rule.IsSelected = !rule.IsSelected;
        }
    }

    [RelayCommand]
    public void FixesSelectAll()
    {
        foreach (var fix in Fixes)
        {
            fix.IsSelected = true;
        }
    }

    [RelayCommand]
    public void FixesInverseSelected()
    {
        foreach (var fix in Fixes)
        {
            fix.IsSelected = !fix.IsSelected;
        }
    }

    [RelayCommand]
    public void ApplySelectedFixes()
    {
        _previewMode = false;
        ApplyFixes();

        RefreshFixes();
    }

    private void RefreshFixes()
    {
        _oldFixes = new List<FixDisplayItem>(Fixes);
        Fixes.Clear();
        _previewMode = true;
        ApplyFixes();
    }

    private void ApplyFixes()
    {
        _totalErrors = 0;

        var subtitle = _previewMode ? new Subtitle(_fixSubtitle, false) : _fixSubtitle;
        foreach (var paragraph in subtitle.Paragraphs)
        {
            paragraph.Text = string.Join(Environment.NewLine, paragraph.Text.SplitToLines());
        }

        foreach (var fix in _allFixRules)
        {
            if (fix.IsSelected)
            {
                var fixCommonError = fix.GetFixCommonErrorFunction();
                fixCommonError.Fix(subtitle, this);
            }
        }

        Paragraphs.Clear();
        Paragraphs.AddRange(_fixSubtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p)));
    }

    public void InitStep1(string languageCode, Subtitle subtitle)
    {
        _fixSubtitle = subtitle;

        _allFixRules = new List<FixRuleDisplayItem>
        {
            new (_language.RemovedEmptyLinesUnusedLineBreaks, "Has only one valid line!</br><i> -> Has only one valid line!", 1, true, nameof(FixEmptyLines)),
            new (_language.FixOverlappingDisplayTimes, string.Empty, 1, true, nameof(FixOverlappingDisplayTimes)),
            new (_language.FixShortDisplayTimes, string.Empty, 1, true, nameof(FixShortDisplayTimes)),
            new (_language.FixLongDisplayTimes, string.Empty, 1, true, nameof(FixLongDisplayTimes)),
            new (_language.FixShortGaps, string.Empty, 1, true, nameof(FixShortGaps)),
            new (_language.FixInvalidItalicTags, _language.FixInvalidItalicTagsExample, 1, true, nameof(FixInvalidItalicTags)),
            new (_language.RemoveUnneededSpaces, _language.RemoveUnneededSpacesExample,1, true, nameof(FixUnneededSpaces)),
            new (_language.FixMissingSpaces, _language.FixMissingSpacesExample, 1, true, nameof(FixMissingSpaces)),
            new (_language.RemoveUnneededPeriods, _language.RemoveUnneededPeriodsExample, 1, true, nameof(FixUnneededPeriods)),
            new (_language.FixCommas, ",, -> ,", 1, true, nameof(FixCommas)),
            new (_language.BreakLongLines, string.Empty, 1, true, nameof(FixLongLines)),
            new (_language.RemoveLineBreaks, "Foo</br>bar! -> Foo bar!", 1, true, nameof(FixShortLines)),
            new (_language.RemoveLineBreaksAll, string.Empty, 1, true, nameof(FixShortLinesAll)),
            new (_language.RemoveLineBreaksPixelWidth, string.Empty, 1, true, nameof(FixShortLinesPixelWidth)),
            new (_language.FixDoubleApostrophes, "''Has double single quotes'' -> \"Has single double quote\"", 1, true, nameof(FixDoubleApostrophes)),
            new (_language.FixMusicNotation, _language.FixMusicNotationExample, 1, true, nameof(FixMusicNotation)),
            new (_language.AddPeriods, "Hello world -> Hello world.", 1, true, nameof(FixMissingPeriodsAtEndOfLine)),
            new (_language.StartWithUppercaseLetterAfterParagraph, "p1: Foobar! || p2: foobar! -> p1: Foobar! || p2: Foobar!", 1, true, nameof(FixStartWithUppercaseLetterAfterParagraph)),
            new (_language.StartWithUppercaseLetterAfterPeriodInsideParagraph, "Hello there! how are you?  -> Hello there! How are you?", 1, true, nameof(FixStartWithUppercaseLetterAfterPeriodInsideParagraph)),
            new (_language.StartWithUppercaseLetterAfterColon, "Speaker: hello world! -> Speaker: Hello world!", 1, true, nameof(FixStartWithUppercaseLetterAfterColon)),
            new (_language.AddMissingQuotes, _language.AddMissingQuotesExample, 1, true, nameof(AddMissingQuotes)),
            new (_language.BreakDialogsOnOneLine, _language.FixDialogsOneLineExample, 1, true, nameof(FixDialogsOnOneLine)),
            new ( string.Format(_language.FixHyphensInDialogs, GetDialogStyle(Configuration.Settings.General.DialogStyle)), string.Empty, 1, true, nameof(FixHyphensInDialog)),
            new ( _language.RemoveHyphensSingleLine, "- Foobar. -> Foobar.", 1, true, nameof(FixHyphensRemoveDashSingleLine)),
            new (_language.Fix3PlusLines, "Foo</br>bar</br>baz! -> Foo bar baz!", 1, true, nameof(Fix3PlusLines)),
            new (_language.FixDoubleDash, _language.FixDoubleDashExample, 1, true, nameof(FixDoubleDash)),
            new (_language.FixDoubleGreaterThan, _language.FixDoubleGreaterThanExample, 1, true, nameof(FixDoubleGreaterThan)),
            new ( string.Format(_language.FixContinuationStyleX, Se.Language.Settings.GetContinuationStyleName(Configuration.Settings.General.ContinuationStyle)), string.Empty, 1, true, nameof(FixContinuationStyle)),
            new (_language.FixMissingOpenBracket, _language.FixMissingOpenBracketExample, 1, true, nameof(FixMissingOpenBracket)),
            //new (_language.FixCommonOcrErrors, _language.FixOcrErrorExample, 1, true, () => FixOcrErrorsViaReplaceList(threeLetterIsoLanguageName), ce.FixOcrErrorsViaReplaceListTicked),
            new (_language.FixUppercaseIInsideLowercaseWords, _language.FixUppercaseIInsideLowercaseWordsExample, 1, true, nameof(FixUppercaseIInsideWords)),
            new (_language.RemoveSpaceBetweenNumber, _language.FixSpaceBetweenNumbersExample, 1, true, nameof(RemoveSpaceBetweenNumbers)),
            new (_language.RemoveDialogFirstInNonDialogs, _language.RemoveDialogFirstInNonDialogsExample, 1, true, nameof(RemoveDialogFirstLineInNonDialogs)),
            new (_language.NormalizeStrings, string.Empty, 1, true, nameof(NormalizeStrings)),
    };

        if (Configuration.Settings.General.ContinuationStyle == ContinuationStyle.None)
        {
            _allFixRules.Add(
                new FixRuleDisplayItem(_language.FixEllipsesStart, _language.FixEllipsesStartExample, 1,
                true, nameof(FixEllipsesStart)));
        }

        if (languageCode == "en")
        {
            _allFixRules.Add(new FixRuleDisplayItem(_language.FixLowercaseIToUppercaseI,
                _language.FixLowercaseIToUppercaseIExample, 1, true, nameof(FixAloneLowercaseIToUppercaseI)));
        }

        if (languageCode == "tr")
        {
            _allFixRules.Add(new FixRuleDisplayItem(_language.FixTurkishAnsi,
                "Ý > İ, Ð > Ğ, Þ > Ş, ý > ı, ð > ğ, þ > ş", 1, true, nameof(FixTurkishAnsiToUnicode)));
        }

        if (languageCode == "da")
        {
            _allFixRules.Add(new FixRuleDisplayItem(_language.FixDanishLetterI,
                "Jeg synes i er søde. -> Jeg synes I er søde.", 1, true, nameof(FixDanishLetterI)));
        }

        if (languageCode == "es")
        {
            _allFixRules.Add(new FixRuleDisplayItem(_language.FixSpanishInvertedQuestionAndExclamationMarks,
                "Hablas bien castellano? -> ¿Hablas bien castellano?", 1, true,
                nameof(FixSpanishInvertedQuestionAndExclamationMarks)));
        }

        FixRules = new ObservableCollection<FixRuleDisplayItem>(_allFixRules);
        UpdateRulesSelection();
    }

    private static string GetDialogStyle(DialogType dialogStyle)
    {
        if (dialogStyle == DialogType.DashSecondLineWithoutSpace)
        {
            return Se.Language.Settings.DialogStyleDashSecondLineWithoutSpace;
        }

        if (dialogStyle == DialogType.DashSecondLineWithSpace)
        {
            return Se.Language.Settings.DialogStyleDashSecondLineWithSpace;
        }

        if (dialogStyle == DialogType.DashBothLinesWithoutSpace)
        {
            return Se.Language.Settings.DialogStyleDashBothLinesWithoutSpace;
        }

        return Se.Language.Settings.DialogStyleDashBothLinesWithSpace;
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void TextBoxSearch_TextChanged(object? sender, TextChangedEventArgs e)
    {
        FixRules.Clear();
        foreach (var rule in _allFixRules)
        {
            if (string.IsNullOrEmpty(SearchText) || rule.Name.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()))
            {
                FixRules.Add(rule);
            }
        }
    }

    public bool AllowFix(Paragraph p, string action)
    {
        if (_previewMode)
        {
            return true;
        }

        var allowFix = Fixes.Any(f => f.Paragraph.Id == p.Id && f.Action == action && f.IsSelected);
        return allowFix;
    }

    public void AddFixToListView(Paragraph p, string action, string before, string after)
    {
        if (!_previewMode)
        {
            return;
        }

        var oldFix = _oldFixes.FirstOrDefault(f => f.Paragraph.Id == p.Id && f.Action == action);
        var isSelected = oldFix is not { IsSelected: false };

        Fixes.Add(new FixDisplayItem(p, p.Number, action, before, after, isSelected));
    }

    public void AddFixToListView(Paragraph p, string action, string before, string after, bool isChecked)
    {
        if (!_previewMode)
        {
            return;
        }

        var oldFix = _oldFixes.FirstOrDefault(f => f.Paragraph.Id == p.Id && f.Action == action);
        var isSelected = isChecked;
        if (oldFix is { IsSelected: false })
        {
            isSelected = false;
        }

        Fixes.Add(new FixDisplayItem(p, p.Number, action, before, after, isSelected));
    }

    public void LogStatus(string sender, string message)
    {
        //TODO: Implement logging functionality
    }

    public void LogStatus(string sender, string message, bool isImportant)
    {
        //TODO: Implement logging functionality
    }

    public void UpdateFixStatus(int fixes, string message)
    {
        if (_previewMode)
        {
            return;
        }

        if (fixes > 0)
        {
            _totalFixes += fixes;
            //            LogStatus(message, string.Format(LanguageSettings.Current.FixCommonErrors.XFixesApplied, fixes));
        }
    }

    public bool IsName(string candidate)
    {
        return _namesList.IsName(candidate);
    }

    public HashSet<string> GetAbbreviations()
    {
        return _namesList.GetAbbreviations();
    }

    public void AddToTotalErrors(int count)
    {
        _totalErrors += count;
    }

    public void AddToDeleteIndices(int index)
    {
        //TODO: Implement functionality to handle delete indices
    }
}