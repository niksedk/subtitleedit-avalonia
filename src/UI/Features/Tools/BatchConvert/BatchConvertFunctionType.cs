namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public enum BatchConvertFunctionType
{
    RemoveFormatting,
    OffsetTimeCodes,
    AdjustDisplayDuration,
    DeleteLines,
    ChangeFrameRate,
    ChangeSpeed,
    ChangeCasing,
    FixCommonErrors,
    MultipleReplace,
    AutoTranslate,
    RemoveTextForHearingImpaired,
    MergeLinesWithSameTimeCodes,
    MergeLinesWithSameText,
    FixRightToLeft,
}