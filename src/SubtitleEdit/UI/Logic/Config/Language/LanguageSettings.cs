using Nikse.SubtitleEdit.Core.Enums;

namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageSettings
{
    public LanguageSettingsShortcuts Shortcuts { get; set; }

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

    public LanguageSettings()
    {
        Shortcuts = new LanguageSettingsShortcuts();

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
    }

    public string GetContinuationStyleName(ContinuationStyle continuationStyle)
    {
        return continuationStyle switch
        {
            Nikse.SubtitleEdit.Core.Enums.ContinuationStyle.NoneTrailingDots => ContinuationStyleNoneTrailingDots,
            Nikse.SubtitleEdit.Core.Enums.ContinuationStyle.NoneLeadingTrailingDots => ContinuationStyleNoneLeadingTrailingDots,
            Nikse.SubtitleEdit.Core.Enums.ContinuationStyle.NoneTrailingEllipsis => ContinuationStyleNoneTrailingEllipsis,
            Nikse.SubtitleEdit.Core.Enums.ContinuationStyle.NoneLeadingTrailingEllipsis => ContinuationStyleNoneLeadingTrailingEllipsis,
            Nikse.SubtitleEdit.Core.Enums.ContinuationStyle.OnlyTrailingDots => ContinuationStyleOnlyTrailingDots,
            Nikse.SubtitleEdit.Core.Enums.ContinuationStyle.LeadingTrailingDots => ContinuationStyleLeadingTrailingDots,
            Nikse.SubtitleEdit.Core.Enums.ContinuationStyle.OnlyTrailingEllipsis => ContinuationStyleOnlyTrailingEllipsis,
            Nikse.SubtitleEdit.Core.Enums.ContinuationStyle.LeadingTrailingEllipsis => ContinuationStyleLeadingTrailingEllipsis,
            Nikse.SubtitleEdit.Core.Enums.ContinuationStyle.LeadingTrailingDash => ContinuationStyleLeadingTrailingDash,
            Nikse.SubtitleEdit.Core.Enums.ContinuationStyle.LeadingTrailingDashDots => ContinuationStyleLeadingTrailingDashDots,
            Nikse.SubtitleEdit.Core.Enums.ContinuationStyle.Custom => ContinuationStyleCustom,
            _ => ContinuationStyleNone,
        };
    }
}