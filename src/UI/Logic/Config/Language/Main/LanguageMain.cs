using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Main;

public class LanguageMain
{
    public LanguageMainMenu Menu { get; set; } = new();
    public LanguageMainToolbar Toolbar { get; set; } = new();
    public LanguageMainWaveform Waveform { get; set; } = new();

    public string CharactersPerSecond { get; set; }
    public string SingleLineLength { get; set; }
    public string TotalCharacters { get; set; }
    public string ExtractingWaveInfo { get; set; }
    public string LoadingWaveInfoFromCache { get; set; }
    public string FailedToExtractWaveInfo { get; set; }
    public string ParsingMatroskaFile { get; set; }
    public string SubtitleImportedFromMatroskaFile { get; set; }
    public string LineXTextAndTimingChanged { get; set; }
    public string LineXTextChangedFromYToZ { get; set; }
    public string LineXTimingChanged { get; set; }
    public string UndoPerformed { get; set; }
    public string RedoPerformed { get; set; }
    public string RedoPerformedXActionLeft { get; set; }
    public string UndoPerformedXActionLeft { get; set; }
    public string SaveXFileAs { get; set; }
    public string SaveLanguageFile { get; set; }
    public string JoinedSubtitleLoaded { get; set; }
    public string CreatedEmptyTranslation { get; set; }
    public string AutoBreakHint { get; set; }
    public string UnbreakHint { get; set; }
    public string ItalicHint { get; set; }
    public string ReplacedXWithYCountZ { get; set; }
    public string ReplacedXWithYInLineZ { get; set; }
    public string XShotChangedLoaded { get; set; }
    public string RuleProfileIsX { get; set; }
    public string XLinesCopiedFromOriginal { get; set; }
    public string OneLineCopiedFromOriginal { get; set; }
    public string XLinesSwitched { get; set; }
    public string OneLineSwitched { get; set; }
    public string XLinesMerged{ get; set; }
    public string OneLineMerged { get; set; }
    public string SpeedIsNowX { get; set; }

    public LanguageMain()
    {
        CharactersPerSecond = "Chars/second: {0}";
        SingleLineLength = "Line length: ";
        TotalCharacters = "Total chars: {0}";
        ExtractingWaveInfo = "Extracting wave info...";
        LoadingWaveInfoFromCache = "Loading wave info from cache...";
        FailedToExtractWaveInfo = "Failed to extract wave info.";
        ParsingMatroskaFile = "Parsing Matroska file...";
        SubtitleImportedFromMatroskaFile = "Subtitle imported from Matroska file";
        LineXTextAndTimingChanged = "Line {0}: Text and timing changed";
        LineXTextChangedFromYToZ = "Line {0}: Text changed from '{1}' to '{2}'";
        LineXTimingChanged = "Line {0}: Timing changed";
        UndoPerformed = "Undo performed";
        RedoPerformed = "Redo performed";
        RedoPerformedXActionLeft = "Redo performed (actions left: {0})";
        UndoPerformedXActionLeft = "Undo performed (actions left: {0})";
        SaveXFileAs = "Save {0} file as";
        SaveLanguageFile = "Save language file";
        JoinedSubtitleLoaded = "Joined subtitle loaded";
        CreatedEmptyTranslation = "Created empty translation from current subtitle";
        AutoBreakHint = "Auto-break selected lines";
        UnbreakHint = "Unbreak selected lines";
        ItalicHint = "Italic selected lines/text";
        ReplacedXWithYCountZ = "Replaced '{0}' with '{1}' ({2} occurrences)";
        ReplacedXWithYInLineZ = "Replaced '{0}' with '{1}' in line {2}";
        XShotChangedLoaded = "{0} shot changes loaded";
        RuleProfileIsX = "Rule profile is now '{0}'";
        XLinesCopiedFromOriginal = "{0} lines copied from original subtitle";
        OneLineCopiedFromOriginal = "One line copied from original subtitle";
        XLinesSwitched = "{0} lines switched";
        OneLineSwitched = "One line switched";
        XLinesMerged = "X lines merged";
        OneLineMerged = "One line merged";
        SpeedIsNowX = "Speed is now '{0}'";
    }
}