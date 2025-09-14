using Nikse.SubtitleEdit.Core.Enums;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Options;

public class LanguageSettings
{
    public string DialogStyle { get; set; }

    public string DialogStyleDashSecondLineWithoutSpace { get; set; }
    public string DialogStyleDashSecondLineWithSpace { get; set; }
    public string DialogStyleDashBothLinesWithSpace { get; set; }
    public string DialogStyleDashBothLinesWithoutSpace { get; set; }

    public string ContinuationStyle { get; set; }
    public string ContinuationStyleNone { get; set; }
    public string ContinuationStyleNoneTrailingDots { get; set; }
    public string ContinuationStyleNoneLeadingTrailingDots { get; set; }
    public string ContinuationStyleNoneTrailingEllipsis { get; set; }
    public string ContinuationStyleNoneLeadingTrailingEllipsis { get; set; }
    public string ContinuationStyleOnlyTrailingDots { get; set; }
    public string ContinuationStyleLeadingTrailingDots { get; set; }
    public string ContinuationStyleOnlyTrailingEllipsis { get; set; }
    public string ContinuationStyleLeadingTrailingEllipsis { get; set; }
    public string ContinuationStyleLeadingTrailingDash { get; set; }
    public string ContinuationStyleLeadingTrailingDashDots { get; set; }
    public string ContinuationStyleCustom { get; set; }

    public string CpsLineLengthStyle { get; set; }
    public string CpsLineLengthStyleCalcAll { get; set; }
    public string CpsLineLengthStyleCalcNoSpaceCpsOnly { get; set; }
    public string CpsLineLengthStyleCalcNoSpace { get; set; }
    public string CpsLineLengthStyleCalcCjk { get; set; }
    public string CpsLineLengthStyleCalcCjkNoSpace { get; set; }
    public string CpsLineLengthStyleCalcIncludeCompositionCharacters { get; set; }
    public string CpsLineLengthStyleCalcIncludeCompositionCharactersNotSpace { get; set; }
    public string CpsLineLengthStyleCalcNoSpaceOrPunctuation { get; set; }
    public string CpsLineLengthStyleCalcNoSpaceOrPunctuationCpsOnly { get; set; }

    public string TimeCodeModeHhMmSsMs { get; set; }
    public string TimeCodeModeHhMmSsFf { get; set; }

    public string SplitBehaviorPrevious { get; set; }
    public string SplitBehaviorHalf { get; set; }
    public string SplitBehaviorNext { get; set; }

    public string SubtitleListActionNothing { get; set; }
    public string SubtitleListActionVideoGoToPositionAndPause { get; set; }
    public string SubtitleListActionVideoGoToPositionAndPlay { get; set; }
    public string SubtitleListActionVideoGoToPositionAndPlayCurrentAndPause { get; set; }
    public string SubtitleListActionEditText { get; set; }
    public string SubtitleListActionVideoGoToPositionMinus1SecAndPause { get; set; }
    public string SubtitleListActionVideoGoToPositionMinusHalfSecAndPause { get; set; }
    public string SubtitleListActionVideoGoToPositionMinus1SecAndPlay { get; set; }
    public string SubtitleListActionEditTextAndPause { get; set; }

    public string AutoBackupEveryMinute { get; set; }
    public string AutoBackupEveryXthMinute { get; set; }

    public string AutoBackupDeleteAfterXMonths { get; set; }
    public string SearchSettingsDotDoDot { get; set; }
    public string SyntaxColoring { get; set; }
    public string WaveformSpectrogram { get; set; }
    public string Network { get; set; }
    public string FileTypeAssociations { get; set; }
    public string TextBoxCenterText { get; set; }
    public string TextBoxFontBold { get;  set; }
    public string TextBoxFontSize { get; set; }
    public string ShowButtonHints { get; set; }
    public string GridCompactMode { get; set; }
    public string UiFont { get; set; }
    public string Theme { get; set; }
    public string DarkThemeBackgroundColor { get; set; }
    public string ShowGridLines { get; set; }
    public string ResetSettings { get; set; }
    public string ResetSettingsDetail { get; set; }
    public string ShowHorizontalLineAboveToolbar { get; set; }
    public string ShowHorizontalLineBelowToolbar { get; set; }
    public string BookmarkColor { get; set; }
    public string SingleLineMaxLength { get; set; }
    public string OptimalCharsPerSec { get; set; }
    public string MaxCharsPerSec { get; set; }
    public string MaxWordsPerMin { get; set; }
    public string MinDurationMs { get; set; }
    public string MaxDurationMs { get; set; }
    public string MinGapMs { get; set; }
    public string MaxLines { get; set; }
    public string NewEmptyDefaultMs { get; set; }
    public string PromptDeleteLines { get; set; }
    public string LockTimeCodes { get; set; }
    public string RememberPositionAndSize { get; set; }
    public string AutoBackupOn { get; set; }
    public string AutoBackupIntervalMinutes { get; set; }
    public string AutoBackupDeleteAfterMonths { get; set; }
    public string DefaultEncoding { get; set; }
    public string ColorDurationTooShort { get; set; }
    public string ColorDurationTooLong { get; set; }
    public string ColorTextTooLong { get; set; }
    public string ColorTextTooWide { get; set; }
    public string ColorTextTooManyLines { get; set; }
    public string ColorOverlap { get; set; }
    public string ColorGapTooShort { get; set; }
    public string ErrorBackgroundColor { get; set; }
    public string WaveformDrawGridLines { get; set; }
    public string WaveformCenterVideoPosition { get; set; }
    public string WaveformShowToolbar { get; set; }
    public string WaveformFocusTextboxAfterInsertNew { get; set; }
    public string WaveformInvertMouseWheel { get; set; }
    public string WaveformSnapToShotChanges { get; set; }
    public string WaveformShotChangesAutoGenerate { get; set; }
    public string WaveformColor { get; set; }
    public string WaveformSelectedColor { get; set; }
    public string WaveformAutoAddShotChanges { get; set; }
    public string DownloadFfmpeg { get; set; }

    // Toolbar
    public string ShowToolbarNew { get; set; }
    public string ShowToolbarOpen { get; set; }
    public string ShowToolbarSave { get; set; }
    public string ShowToolbarSaveAs { get; set; }
    public string ShowToolbarFind { get; set; }
    public string ShowToolbarReplace { get; set; }
    public string ShowToolbarSpellCheck { get; set; }
    public string ShowToolbarSettings { get; set; }
    public string ShowToolbarLayout { get; set; }
    public string ShowToolbarHelp { get; set; }
    public string ShowToolbarEncoding { get; set; }

    // Network
    public string ProxyAddress { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public string DefaultFormat { get; set; }
    public string DefaultSaveAsFormat { get; set; }

    public string ShowStopButton { get; set; }
    public string ShowFullscreenButton { get; set; }
    public string AutoOpenVideoFile { get; set; }
    public string DownloadMpv { get; set; }
    public string GoToLineNumberSetsVideoPosition { get; set; }

    public LanguageSettings()
    {
        DialogStyle = "Dialog style";
        DialogStyleDashBothLinesWithSpace = "Dash both lines with space";
        DialogStyleDashBothLinesWithoutSpace = "Dash both lines without space";
        DialogStyleDashSecondLineWithSpace = "Dash second line with space";
        DialogStyleDashSecondLineWithoutSpace = "Dash second line without space";

        ContinuationStyle = "Continuation style";
        ContinuationStyleNone = "None";
        ContinuationStyleNoneTrailingDots = "None, dots for pauses (trailing only)";
        ContinuationStyleNoneLeadingTrailingDots = "None, dots for pauses";
        ContinuationStyleNoneTrailingEllipsis = "None, ellipsis for pauses (trailing only)";
        ContinuationStyleNoneLeadingTrailingEllipsis = "None, ellipsis for pauses";
        ContinuationStyleOnlyTrailingDots = "Dots (trailing only)";
        ContinuationStyleLeadingTrailingDots = "Dots";
        ContinuationStyleOnlyTrailingEllipsis = "Ellipsis (trailing only)";
        ContinuationStyleLeadingTrailingEllipsis = "Ellipsis";
        ContinuationStyleLeadingTrailingDash = "Dash";
        ContinuationStyleLeadingTrailingDashDots = "Dash, but dots for pauses";
        ContinuationStyleCustom = "Custom";

        CpsLineLengthStyle = "Cps/line-length";
        CpsLineLengthStyleCalcAll = "Count all characters";
        CpsLineLengthStyleCalcNoSpaceCpsOnly = "Count all except space, cps only";
        CpsLineLengthStyleCalcNoSpace = "Count all except space";
        CpsLineLengthStyleCalcCjk = "CJK 1, Latin 0.5";
        CpsLineLengthStyleCalcCjkNoSpace = "CJK 1, Latin 0.5, space 0";
        CpsLineLengthStyleCalcIncludeCompositionCharacters = "Include composition characters";
        CpsLineLengthStyleCalcIncludeCompositionCharactersNotSpace = "Include composition characters, not space";
        CpsLineLengthStyleCalcNoSpaceOrPunctuation = "No space or punctuation ()[]-:;,.!?";
        CpsLineLengthStyleCalcNoSpaceOrPunctuationCpsOnly = "No space or punctuation, CPS only";

        TimeCodeModeHhMmSsMs = "HH:MM:SS:MS";
        TimeCodeModeHhMmSsFf = "HH:MM:SS:FF";

        SplitBehaviorPrevious = "Add gap to the left of split point (focus right)";
        SplitBehaviorHalf = "Add gap in the center of split point (focus left)";
        SplitBehaviorNext = "Add gap to the right of split point (focus left)";

        SubtitleListActionNothing = "Nothing";
        SubtitleListActionVideoGoToPositionAndPause = "Go to video position and pause";
        SubtitleListActionVideoGoToPositionAndPlay = "Go to video position and play";
        SubtitleListActionVideoGoToPositionAndPlayCurrentAndPause = "Go to video position, play current, and pause";
        SubtitleListActionEditText = "Go to edit text box";
        SubtitleListActionVideoGoToPositionMinus1SecAndPause = "Go to video position - 1 s and pause";
        SubtitleListActionVideoGoToPositionMinusHalfSecAndPause = "Go to video position - 0.5 s and pause";
        SubtitleListActionVideoGoToPositionMinus1SecAndPlay = "Go to video position - 1 s and play";
        SubtitleListActionEditTextAndPause = "Go to edit text box, and pause at video position";

        AutoBackupEveryMinute = "Every minute";
        AutoBackupEveryXthMinute = "Every {0}th minute";
        AutoBackupDeleteAfterXMonths = "Delete auto-backups after {0} months";

        SearchSettingsDotDoDot = "Search for settings...";
        SyntaxColoring = "Syntax coloring";
        WaveformSpectrogram = "Waveform/spectrogram";
        Network = "Network";
        FileTypeAssociations = "File type associations";
        TextBoxCenterText = "Center text in subtitle text box";
        TextBoxFontBold = "Bold text in subtitle text box";
        TextBoxFontSize = "Font size in subtitle text box";
        ShowButtonHints = "Show button hints";
        GridCompactMode = "Use compact mode for grids";
        UiFont = "UI font";
        Theme = "Theme";
        DarkThemeBackgroundColor = "Dark theme background color";
        ShowGridLines = "Show grid lines";
        ResetSettings = "Reset settings?";
        ResetSettingsDetail = "This will reset all settings to their default values.\n\nContinue?";
        ShowHorizontalLineAboveToolbar = "Show horizontal line above toolbar";
        ShowHorizontalLineBelowToolbar = "Show horizontal line below toolbar";
        BookmarkColor = "Bookmark color";
        SingleLineMaxLength = "Single line max length";
        OptimalCharsPerSec = "Optimal chars/sec";
        MaxCharsPerSec = "Max chars/sec";
        MaxWordsPerMin = "Max words/min";
        MinDurationMs = "Min duration (ms)";
        MaxDurationMs = "Max duration (ms)";
        MinGapMs = "Min gap (ms)";
        MaxLines = "Max number of lines";
        NewEmptyDefaultMs = "Default new subtitle duration (ms)";
        PromptDeleteLines = "Prompt for delete lines";
        LockTimeCodes = "Lock time codes";
        RememberPositionAndSize = "Remember window position and size";
        AutoBackupOn = "Auto-backup";
        AutoBackupIntervalMinutes = "Auto-backup interval (minutes)";
        AutoBackupDeleteAfterMonths = "Auto-backup retention (months)";
        DefaultEncoding = "Default encoding";
        ColorDurationTooShort = "Color duration if too short";
        ColorDurationTooLong = "Color duration if too long";
        ColorTextTooLong = "Color text if too long";
        ColorTextTooWide = "Color text if too wide (pixels)";
        ColorTextTooManyLines = "Color text if more than 2 lines";
        ColorOverlap = "Color time code overlap";
        ColorGapTooShort = "Color if gap is too short";
        ErrorBackgroundColor = "Error background color";
        WaveformDrawGridLines = "Draw grid lines";
        WaveformCenterVideoPosition = "Center video position";
        WaveformShowToolbar = "Show toolbar";
        WaveformFocusTextboxAfterInsertNew = "Focus text box after insert";
        WaveformInvertMouseWheel = "Invert mouse-wheel";
        WaveformSnapToShotChanges = "Snap to shot changes";
        WaveformShotChangesAutoGenerate = "Shot changes auto-generate";
        WaveformColor = "Waveform color";
        WaveformSelectedColor = "Waveform selected color";
        WaveformAutoAddShotChanges = "Auto-generate shot changes";
        DownloadFfmpeg = "Download ffmpeg";

        // Toolbar
        ShowToolbarNew = "Show new icon";
        ShowToolbarOpen = "Show open icon";
        ShowToolbarSave = "Show save icon";
        ShowToolbarSaveAs = "Show save as icon";
        ShowToolbarFind = "Show find icon";
        ShowToolbarReplace = "Show replace icon";
        ShowToolbarSpellCheck = "Show spell check icon";
        ShowToolbarSettings = "Show settings icon";
        ShowToolbarLayout = "Show layout icon";
        ShowToolbarHelp = "Show help icon";
        ShowToolbarEncoding = "Show encoding";

        // Network
        ProxyAddress = "Proxy address";
        Username = "Username";
        Password = "Password";

        ShowStopButton = "Show stop button";
        ShowFullscreenButton = "Show fullscreen button";
        AutoOpenVideoFile = "Auto-open video file when opening subtitle";
        DownloadMpv = "Download mpv";
        GoToLineNumberSetsVideoPosition = "Go-to-line-number also sets video position";
        DefaultFormat = "Default format";
        DefaultSaveAsFormat = "Default 'Save as' format";
    }

    public string GetContinuationStyleName(ContinuationStyle continuationStyle)
    {
        return continuationStyle switch
        {
            Core.Enums.ContinuationStyle.NoneTrailingDots => ContinuationStyleNoneTrailingDots,
            Core.Enums.ContinuationStyle.NoneLeadingTrailingDots => ContinuationStyleNoneLeadingTrailingDots,
            Core.Enums.ContinuationStyle.NoneTrailingEllipsis => ContinuationStyleNoneTrailingEllipsis,
            Core.Enums.ContinuationStyle.NoneLeadingTrailingEllipsis => ContinuationStyleNoneLeadingTrailingEllipsis,
            Core.Enums.ContinuationStyle.OnlyTrailingDots => ContinuationStyleOnlyTrailingDots,
            Core.Enums.ContinuationStyle.LeadingTrailingDots => ContinuationStyleLeadingTrailingDots,
            Core.Enums.ContinuationStyle.OnlyTrailingEllipsis => ContinuationStyleOnlyTrailingEllipsis,
            Core.Enums.ContinuationStyle.LeadingTrailingEllipsis => ContinuationStyleLeadingTrailingEllipsis,
            Core.Enums.ContinuationStyle.LeadingTrailingDash => ContinuationStyleLeadingTrailingDash,
            Core.Enums.ContinuationStyle.LeadingTrailingDashDots => ContinuationStyleLeadingTrailingDashDots,
            Core.Enums.ContinuationStyle.Custom => ContinuationStyleCustom,
            _ => ContinuationStyleNone,
        };
    }
}