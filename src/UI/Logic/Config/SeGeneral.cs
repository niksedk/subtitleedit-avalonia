using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeGeneral
{
    public string Version { get; set; }
    public string Language { get; set; }
    public int LayoutNumber { get; set; } = 0;
    public bool UseTimeFormatHhMmSsFf { get; set; } = false;
    public double DefaultFrameRate { get; set; }
    public double CurrentFrameRate { get; set; }
    public string DefaultSubtitleFormat { get; set; }
    public string DefaultSaveAsFormat { get; set; }
    public string FavoriteSubtitleFormats { get; set; }
    public string DefaultEncoding { get; set; }
    public bool AutoConvertToUtf8 { get; set; }
    public bool AutoGuessAnsiEncoding { get; set; }
    public int SubtitleLineMaximumPixelWidth { get; set; }
    public int SubtitleLineMaximumLength { get; set; }
    public int MaxNumberOfLines { get; set; }
    public int MaxNumberOfLinesPlusAbort { get; set; }
    public int MergeLinesShorterThan { get; set; }
    public int SubtitleMinimumDisplayMilliseconds { get; set; }
    public int SubtitleMaximumDisplayMilliseconds { get; set; }
    public int MinimumMillisecondsBetweenLines { get; set; }
    public double SubtitleMaximumCharactersPerSeconds { get; set; }
    public double SubtitleOptimalCharactersPerSeconds { get; set; }
    public string CpsLineLengthStrategy { get; set; }
    public double SubtitleMaximumWordsPerMinute { get; set; }
    public int NewEmptyDefaultMs { get; set; }
    public bool PromptDeleteLines { get; set; }
    public bool LockTimeCodes { get; set; }
    public bool RememberPositionAndSize { get; set; }
    public bool PositionIsFullScreen { get; set; }
    public bool PositionIsMaximized { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int PositionWidth { get; set; }
    public int PositionHeight { get; set; }
    public bool AutoBackupOn { get; set; }
    public int AutoBackupIntervalMinutes { get; set; }
    public int AutoBackupDeleteAfterMonths { get; set; }

    public bool ColorDurationTooShort { get; set; }
    public bool ColorDurationTooLong { get; set; }
    public bool ColorTextTooLong { get; set; }
    public bool ColorTextTooWide { get; set; }
    public bool ColorTextTooManyLines { get; set; }
    public bool ColorTimeCodeOverlap { get; set; }
    public bool ColorGapTooShort { get; set; }
    public string ErrorColor { get; set; }

    public string FfmpegPath { get; set; }
    public string LibMpvPath { get; set; }
    public bool AutoOpenVideo { get; set; }
    public DialogType DialogStyle { get; set; }
    public bool ShowColumnEndTime { get; set; }
    public bool ShowColumnGap { get; set; }
    public bool ShowColumnDuration { get; set; }
    public bool ShowColumnActor { get; set; }
    public bool ShowColumnCps { get; set; }
    public bool ShowColumnWpm { get; set; }
    public bool SelectCurrentSubtitleWhilePlaying { get; set; }
    public bool WriteAn2Tag { get; set; }

    public SeGeneral()
    {
        Version = Se.Version;
        Language = "English";
        LayoutNumber = 0;
        UseTimeFormatHhMmSsFf = false;
        DefaultFrameRate = 23.976;
        CurrentFrameRate = DefaultFrameRate;
        SubtitleLineMaximumPixelWidth = 576;
        DefaultSubtitleFormat = new SubRip().FriendlyName;
        DefaultEncoding = TextEncoding.Utf8WithBom;
        AutoConvertToUtf8 = false;
        AutoGuessAnsiEncoding = true;
        SubtitleLineMaximumLength = 43;
        MaxNumberOfLines = 2;
        MaxNumberOfLinesPlusAbort = 1;
        MergeLinesShorterThan = 33;
        SubtitleMinimumDisplayMilliseconds = 1000;
        SubtitleMaximumDisplayMilliseconds = 8 * 1000;
        MinimumMillisecondsBetweenLines = 24;
        SubtitleMaximumCharactersPerSeconds = 25.0;
        SubtitleOptimalCharactersPerSeconds = 15.0;
        SubtitleMaximumWordsPerMinute = 400;
        NewEmptyDefaultMs = 2000;
        PromptDeleteLines = true;
        AutoBackupOn = true;
        AutoBackupIntervalMinutes = 5;
        AutoBackupDeleteAfterMonths = 3;
        DefaultSaveAsFormat = "SubRip";
        FavoriteSubtitleFormats = "SubRip";
        CpsLineLengthStrategy = "";//TODO: Add default value

        ColorDurationTooShort = true;
        ColorDurationTooLong = true;
        ColorTextTooLong = true;
        ColorTextTooWide = true;
        ColorTextTooManyLines = true;
        ColorTimeCodeOverlap = true;
        ColorGapTooShort = true;
        ErrorColor = Color.FromArgb(50, 255, 0, 0).FromColorToHex();

        FfmpegPath = string.Empty;
        LibMpvPath = string.Empty;
        AutoOpenVideo = true;

        DialogStyle = DialogType.DashBothLinesWithSpace;
        ShowColumnEndTime = true;
        ShowColumnGap = false;
        ShowColumnDuration = true;
    }
}