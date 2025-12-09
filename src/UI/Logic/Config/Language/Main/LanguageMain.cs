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
    public string XLinesMerged { get; set; }
    public string OneLineMerged { get; set; }
    public string SpeedIsNowX { get; set; }
    public string DeleteText { get; set; }
    public string DeleteTextAndShiftCellsUp { get; set; }
    public string InsertEmptyTextAndShiftCellsDown { get; set; }
    public string InsertTextFromSubtitleDotDotDot { get; set; }
    public string PasteFromClipboardDotDotDot { get; set; }
    public string TextUp { get; set; }
    public string TextDown { get; set; }
    public string InsertedXTextsFromSubtitleY { get; set; }
    public string ColumnPaste { get; set; }
    public string ChooseColumn { get; set; }
    public string TimeCodesOnly { get; set; }
    public string TextOnly { get; set; }
    public string OverwriteOrShiftCellsDown { get; set; }
    public string OverwriteExistingCells { get; set; }
    public string ShiftTextCellsDown { get; set; }
    public string NoTextInClipboard { get; set; }
    public string AudioTrackIsNowX { get; set; }
    public string FixedRightToLeftUsingUnicodeControlCharactersX { get; set; }
    public string RemovedUnicodeControlCharactersX { get; set; }
    public string ReversedStartAndEndingsForRightToLeftX { get; set; }
    public string ErrorLoadRar { get; set; }
    public string ErrorLoadZip { get; set; }
    public string ErrorLoadGZip { get; set; }
    public string ErrorLoad7Zip { get; set; }
    public string ErrorLoadPng { get; set; }
    public string ErrorLoadJpg { get; set; }
    public string ErrorLoadSrr { get; set; }
    public string ErrorLoadTorrent { get; set; }
    public string ErrorLoadBinaryZeroes { get; set; }
    public string YoutubeDlNotInstalledDownloadNow { get; set; }
    public string YoutubeDlDownloadedSuccessfully { get; set; }
    public string GeneratingSpectrogramDotDotDot { get; set; }
    public string RemovedXBlankLines { get; set; }
    public string XLinesSelectedOfY { get; set; }
    public string EndTimeMustBeAfterStartTime { get; set; }

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
        LineXTextChangedFromYToZ = "Line {0}: Text changed from \"{1}\" to \"{2}\"";
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
        ReplacedXWithYCountZ = "Replaced \"{0}\" with \"{1}\" ({2} occurrences)";
        ReplacedXWithYInLineZ = "Replaced \"{0}\" with \"{1}\" in line {2}";
        XShotChangedLoaded = "{0} shot changes loaded";
        RuleProfileIsX = "Rule profile is now \"{0}\"";
        XLinesCopiedFromOriginal = "{0} lines copied from original subtitle";
        OneLineCopiedFromOriginal = "One line copied from original subtitle";
        XLinesSwitched = "{0} lines switched";
        OneLineSwitched = "One line switched";
        XLinesMerged = "X lines merged";
        OneLineMerged = "One line merged";
        SpeedIsNowX = "Speed is now \"{0}\"";
        DeleteText = "Delete text";
        DeleteTextAndShiftCellsUp = "Delete text and shift cells up";
        InsertEmptyTextAndShiftCellsDown = "Insert empty text and shift cells down";
        InsertTextFromSubtitleDotDotDot = "Insert text from subtitle...";
        PasteFromClipboardDotDotDot = "Paste from clipboard...";
        TextUp = "Text up";
        TextDown = "Text down";
        InsertedXTextsFromSubtitleY = "Inserted {0} texts from subtitle file \"{1}\"";
        ColumnPaste = "Column paste";
        ChooseColumn = "Choose column";
        TimeCodesOnly = "Time codes only";
        TextOnly = "Text only";
        OverwriteOrShiftCellsDown = "Overwrite/shift cells down";
        OverwriteExistingCells = "Overwrite existing cells";
        ShiftTextCellsDown = "Shift text cells down";
        NoTextInClipboard = "No text in clipboard";
        AudioTrackIsNowX = "Audio track is now \"{0}\"";
        FixedRightToLeftUsingUnicodeControlCharactersX = "Fixed right-to-left using Unicode control characters in {0} lines";
        RemovedUnicodeControlCharactersX = "Removed Unicode control characters from {0} lines";
        ReversedStartAndEndingsForRightToLeftX = "Reversed start and endings for right-to-left in {0} lines";
        ErrorLoadRar = "This file seems to be a compressed 7-Zip file.\n\nSubtitle Edit cannot open compressed files.";
        ErrorLoadZip = "This file seems to be a compressed ZIP file.\n\nSubtitle Edit cannot open compressed files.";
        ErrorLoadGZip = "This file seems to be a compressed GZip file.\n\nSubtitle Edit cannot open compressed files.";
        ErrorLoad7Zip = "This file seems to be a compressed 7-Zip file.\n\nSubtitle Edit cannot open compressed files.";
        ErrorLoadPng = "This file seems to be a PNG image file.\n\nSubtitle Edit cannot open image files.";
        ErrorLoadJpg = "This file seems to be a JPG image file.\n\nSubtitle Edit cannot open image files.";
        ErrorLoadSrr = "This file seems to be a ReScene SRR file.\n\nSubtitle Edit cannot open SRR files.";
        ErrorLoadTorrent = "This file seems to be a BitTorrent file.\n\nSubtitle Edit cannot open torrent files.";
        ErrorLoadBinaryZeroes = "Sorry, this file contains only binary zeroes!\n\nIf you have edited this file with Subtitle Edit you might be able to find a backup via the menu item File -&gt; Restore auto-backup...";
        YoutubeDlNotInstalledDownloadNow = "\"yt-dlp\" is not installed and is required for playing online videos.\n\nDownload now?";
        YoutubeDlDownloadedSuccessfully = "\"yt-dlp\" downloaded successfully.";
        GeneratingSpectrogramDotDotDot = "Generating spectrogram...";   
        RemovedXBlankLines = "Removed {0} blank lines";
        XLinesSelectedOfY = "{0} lines selected of {1}";
        EndTimeMustBeAfterStartTime = "End time must be after start time.";
    }
}