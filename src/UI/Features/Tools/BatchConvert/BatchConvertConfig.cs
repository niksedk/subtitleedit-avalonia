using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class BatchConvertConfig
{
    public string OutputFolder { get; set; }
    public bool SaveInSourceFolder { get; set; }
    public bool Overwrite { get; set; }
    public string TargetFormatName { get; set; }
    public string TargetEncoding { get; set; }

    public RemoveFormattingSettings RemoveFormatting { get; set; }
    public OffsetTimeCodesSettings OffsetTimeCodes { get; set; }
    public AdjustDurationSettings AdjustDuration { get; set; }
    public ChangeFrameRateSettings ChangeFrameRate { get; set; }
    public ChangeSpeedSettings ChangeSpeed { get; set; }
    public ChangeCasingSettings ChangeCasing { get; set; }
    public FixCommonErrorsSettings2 FixCommonErrors { get; set; }
    public RemoveLineBreaksSettings RemoveLineBreaks { get; set; }
    public DeleteLinesSettings DeleteLines { get; set; }
    public AutoTranslateSettings AutoTranslate { get; set; }
    public RemoveTextForHearingImpairedSettings RemoveTextForHearingImpaired { get; set; }
    public MergeLinesWithSameTimeCodesSettings MergeLinesWithSameTimeCodes { get; set; }
    public MergeLinesWithSameTextsSettings MergeLinesWithSameTexts { get; set; }

    public BatchConvertConfig()
    {
        OutputFolder = string.Empty;
        SaveInSourceFolder = true;
        TargetFormatName = SubRip.NameOfFormat;
        TargetEncoding = TextEncoding.Utf8WithBom;
        RemoveFormatting = new RemoveFormattingSettings();
        OffsetTimeCodes = new OffsetTimeCodesSettings();
        AdjustDuration = new AdjustDurationSettings();
        RemoveLineBreaks = new RemoveLineBreaksSettings();
        ChangeFrameRate = new ChangeFrameRateSettings();
        ChangeSpeed = new ChangeSpeedSettings();
        FixCommonErrors = new FixCommonErrorsSettings2();
        ChangeCasing = new ChangeCasingSettings();
        DeleteLines = new DeleteLinesSettings();
        AutoTranslate = new AutoTranslateSettings();
        RemoveTextForHearingImpaired = new RemoveTextForHearingImpairedSettings();
        MergeLinesWithSameTimeCodes = new MergeLinesWithSameTimeCodesSettings();
        MergeLinesWithSameTexts = new MergeLinesWithSameTextsSettings();
    }

    public class RemoveFormattingSettings
    {
        public bool IsActive { get; set; }
        public bool RemoveAll { get; set; }
        public bool RemoveItalic { get; set; }
        public bool RemoveBold { get; set; }
        public bool RemoveUnderline { get; set; }
        public bool RemoveColor { get; set; }
        public bool RemoveFontName { get; set; }
        public bool RemoveAlignment { get; set; }
    }

    public class OffsetTimeCodesSettings
    {
        public bool IsActive { get; set; }
        public bool Forward { get; set; }
        public long Milliseconds { get; set; }
    }

    public class AdjustDurationSettings
    {
        public bool IsActive { get; set; }
        public AdjustDurationType AdjustmentType { get; set; }
        public double Seconds { get; set; }
        public int Percentage { get; set; }
        public int FixedMilliseconds { get; set; }
        public double OptimalCharsPerSecond { get; set; }
        public double MaxCharsPerSecond { get; set; }

        public AdjustDurationSettings()
        {
            AdjustmentType = AdjustDurationType.Seconds;
            OptimalCharsPerSecond = 15;
            MaxCharsPerSecond = 25;
        }
    }

    public class ChangeFrameRateSettings
    {
        public bool IsActive { get; set; }
        public double FromFrameRate { get; set; }
        public double ToFrameRate { get; set; }
    }

    public class ChangeSpeedSettings
    {
        public bool IsActive { get; set; }
        public double SpeedPercent { get; set; }
    }

    public class ChangeCasingSettings
    {
        public bool IsActive { get; set; }
        public bool NormalCasing { get; set; }
        public bool NormalCasingFixNames { get; set; }
        public bool NormalCasingOnlyUpper { get; set; }
        public bool FixNamesOnly { get; set; }
        public bool AllUppercase { get; set; }
        public bool AllLowercase { get; set; }
    }

    public class FixCommonErrorsSettings2
    {
        public bool IsActive { get; set; }
    }

    public class RemoveLineBreaksSettings
    {
        public bool IsActive { get; set; }
    }

    public class DeleteLinesSettings
    {
        public bool IsActive { get; set; }
        public int DeleteXFirst { get; set; }
        public int DeleteXLast { get; set; }
        public string DeleteContains { get; set; }

        public DeleteLinesSettings()
        {
            DeleteContains = string.Empty;
        }
    }

    public class AutoTranslateSettings
    {
        public bool IsActive { get; set; }
        public TranslationPair SourceLanguage { get; internal set; }
        public TranslationPair TargetLanguage { get; internal set; }
        public IAutoTranslator Translator { get; internal set; }

        public AutoTranslateSettings()
        {
            SourceLanguage = new TranslationPair("English", "en");
            TargetLanguage = new TranslationPair("Spanish", "es");
            Translator = new OllamaTranslate();
        }
    }

    public class RemoveTextForHearingImpairedSettings
    {
        public bool IsActive { get; set; }

        public RemoveTextForHearingImpairedSettings()
        {
        }
    }

    public class MergeLinesWithSameTimeCodesSettings
    {
        public bool IsActive { get; set; }

        public MergeLinesWithSameTimeCodesSettings()
        {
        }
    }

    public class MergeLinesWithSameTextsSettings
    {
        public bool IsActive { get; set; }

        public MergeLinesWithSameTextsSettings()
        {
        }
    }
}

