using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class BatchConverter : IBatchConverter, IFixCallbacks
{
    private BatchConvertConfig _config;
    private List<SubtitleFormat> _subtitleFormats;

    public SubtitleFormat Format { get; set; } = new SubRip();

    public Encoding Encoding { get; set; } = Encoding.UTF8;

    public string Language { get; set; } = "en";

    public BatchConverter()
    {
        _config = new BatchConvertConfig();
        _subtitleFormats = new List<SubtitleFormat>();
    }

    public void Initialize(BatchConvertConfig config)
    {
        _config = config;
        _subtitleFormats = SubtitleFormat.AllSubtitleFormats.ToList();
    }

    public async Task Convert(BatchConvertItem item, CancellationToken cancellationToken)
    {
        if (_subtitleFormats.Count == 0)
        {
            throw new InvalidOperationException("Initialize not called?");
        }

        foreach (var format in _subtitleFormats)
        {
            if (format.Name == _config.TargetFormatName && item.Subtitle != null)
            {
                item.Status = Se.Language.General.ConvertingDotDotDot;
                ;
                try
                {
                    var processedSubtitle = await RunConvertFunctions(item, cancellationToken);
                    var converted = format.ToText(processedSubtitle, _config.TargetEncoding);
                    var path = MakeOutputFileName(item, format);
                    await File.WriteAllTextAsync(path, converted);
                    item.Status = Se.Language.General.Converted;
                }
                catch (Exception exception)
                {
                    item.Status = string.Format(Se.Language.General.ErrorX, exception.Message);
                }

                break;
            }
        }
    }

    private async Task<Subtitle> RunConvertFunctions(BatchConvertItem item, CancellationToken cancellationToken)
    {
        var s = new Subtitle(item.Subtitle, false);
        Language = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(s) ?? "en";
        s = AdjustDisplayDuration(s);
        s = await AutoTranslate(s, cancellationToken);
        s = ChangeCasing(s, Language);
        s = ChangeFrameRate(s);
        s = ChangeSpeed(s);
        s = DeleteLines(s);
        s = FixCommonErrors(s);
        s = MergeLinesWithSameText(s);
        s = MergeLinesWithSameTimeCodes(s);
        s = OffsetTimeCodes(s);
        s = RemoveFormatting(s);
        s = RemoveLineBreaks(s);
        s = RemoveTextForHearingImpaired(s);
        return s;
    }

    private Subtitle RemoveFormatting(Subtitle subtitle)
    {
        if (!_config.RemoveFormatting.IsActive)
        {
            return subtitle;
        }

        foreach (var p in subtitle.Paragraphs)
        {
            if (_config.RemoveFormatting.RemoveAll)
            {
                p.Text = HtmlUtil.RemoveHtmlTags(p.Text, true);
            }
            else
            {
                if (_config.RemoveFormatting.RemoveItalic)
                {
                    p.Text = HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagItalic);
                    p.Text = p.Text
                        .Replace("{\\i}", string.Empty)
                        .Replace("{\\i0}", string.Empty)
                        .Replace("{\\i1}", string.Empty);
                }

                if (_config.RemoveFormatting.RemoveBold)
                {
                    p.Text = HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagBold);
                    p.Text = p.Text
                        .Replace("{\\b}", string.Empty)
                        .Replace("{\\b0}", string.Empty)
                        .Replace("{\\b1}", string.Empty);
                }

                if (_config.RemoveFormatting.RemoveUnderline)
                {
                    p.Text = HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagUnderline);
                    p.Text = p.Text
                        .Replace("{\\u}", string.Empty)
                        .Replace("{\\u0}", string.Empty)
                        .Replace("{\\u1}", string.Empty);
                }

                if (_config.RemoveFormatting.RemoveColor)
                {
                    p.Text = HtmlUtil.RemoveColorTags(p.Text);
                    if (p.Text.Contains("\\c") || p.Text.Contains("\\1c"))
                    {
                        p.Text = HtmlUtil.RemoveAssaColor(p.Text);
                    }
                }

                if (_config.RemoveFormatting.RemoveFontName)
                {
                    p.Text = HtmlUtil.RemoveFontName(p.Text);
                }

                if (_config.RemoveFormatting.RemoveAlignment)
                {
                    if (p.Text.Contains('{'))
                    {
                        p.Text = HtmlUtil.RemoveAssAlignmentTags(p.Text);
                    }
                }
            }
        }

        return subtitle;
    }

    private Subtitle OffsetTimeCodes(Subtitle subtitle)
    {
        if (!_config.OffsetTimeCodes.IsActive)
        {
            return subtitle;
        }

        var totalMilliseconds = _config.OffsetTimeCodes.Milliseconds;
        if (!_config.OffsetTimeCodes.Forward)
        {
            totalMilliseconds *= -1;
        }

        subtitle.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(totalMilliseconds));
        return subtitle;
    }

    private Subtitle ChangeFrameRate(Subtitle subtitle)
    {
        if (!_config.ChangeFrameRate.IsActive)
        {
            return subtitle;
        }

        subtitle.ChangeFrameRate(_config.ChangeFrameRate.FromFrameRate, _config.ChangeFrameRate.ToFrameRate);

        return subtitle;
    }

    private Subtitle ChangeSpeed(Subtitle subtitle)
    {
        if (!_config.ChangeSpeed.IsActive)
        {
            return subtitle;
        }

        foreach (var p in subtitle.Paragraphs)
        {
            p.StartTime.TotalMilliseconds = TimeSpan.FromMilliseconds(p.StartTime.TotalMilliseconds * (100.0 / _config.ChangeSpeed.SpeedPercent)).TotalMilliseconds;
            p.EndTime.TotalMilliseconds = TimeSpan.FromMilliseconds(p.EndTime.TotalMilliseconds * (100.0 / _config.ChangeSpeed.SpeedPercent)).TotalMilliseconds;
        }

        return subtitle;
    }


    private Subtitle ChangeCasing(Subtitle subtitle, string language)
    {
        if (!_config.ChangeCasing.IsActive)
        {
            return subtitle;
        }

        var fixCasing = new FixCasing(language)
        {
            FixNormal = _config.ChangeCasing.NormalCasing,
            FixNormalOnlyAllUppercase = _config.ChangeCasing.NormalCasingOnlyUpper,
            FixMakeUppercase = _config.ChangeCasing.AllUppercase,
            FixMakeLowercase = _config.ChangeCasing.AllLowercase,
            FixMakeProperCase = false,
            FixProperCaseOnlyAllUppercase = false,
            Format = subtitle.OriginalFormat,
        };
        fixCasing.Fix(subtitle);

        return subtitle;
    }

    private Subtitle FixCommonErrors(Subtitle subtitle)
    {
        if (!_config.FixCommonErrors.IsActive || _config.FixCommonErrors.Profile == null)
        {
            return subtitle;
        }

        foreach (var fix in _config.FixCommonErrors.Profile.FixRules)
        {
            if (fix.IsSelected)
            {
                var fixCommonError = fix.GetFixCommonErrorFunction();
                fixCommonError.Fix(subtitle, this);
            }
        }

        return subtitle;
    }

    private Subtitle RemoveLineBreaks(Subtitle subtitle)
    {
        if (!_config.OffsetTimeCodes.IsActive)
        {
            return subtitle;
        }

        foreach (var paragraph in subtitle.Paragraphs)
        {
            paragraph.Text = Utilities.UnbreakLine(paragraph.Text);
        }

        return subtitle;
    }

    private Subtitle DeleteLines(Subtitle subtitle)
    {
        if (!_config.DeleteLines.IsActive)
        {
            return subtitle;
        }

        var c = _config.DeleteLines;
        if (c.DeleteXFirst == 0 && c.DeleteXLast == 0 && string.IsNullOrWhiteSpace(c.DeleteContains))
        {
            return subtitle;
        }

        var paragraphs = subtitle.Paragraphs.Skip(c.DeleteXFirst).ToList();
        paragraphs = paragraphs.Take(paragraphs.Count - c.DeleteXLast).ToList();
        if (!string.IsNullOrWhiteSpace(c.DeleteContains))
        {
            paragraphs = paragraphs.Where(p => !p.Text.Contains(c.DeleteContains)).ToList();
        }

        subtitle.Paragraphs.Clear();
        subtitle.Paragraphs.AddRange(paragraphs);
        subtitle.Renumber();

        return subtitle;
    }

    private Subtitle AdjustDisplayDuration(Subtitle subtitle)
    {
        if (!_config.AdjustDuration.IsActive)
        {
            return subtitle;
        }

        var shotChanges = new List<double>();
        var c = _config.AdjustDuration;
        if (c.AdjustmentType == AdjustDurationType.Percent)
        {
            subtitle.AdjustDisplayTimeUsingPercent(c.Percentage, null, shotChanges, true);
        }
        else if (c.AdjustmentType == AdjustDurationType.Recalculate)
        {
            subtitle.RecalculateDisplayTimes(c.MaxCharsPerSecond, null, c.OptimalCharsPerSecond, true, shotChanges, true);
        }
        else if (c.AdjustmentType == AdjustDurationType.Fixed)
        {
            subtitle.SetFixedDuration(null, c.FixedMilliseconds * 1000.0);
        }
        else if (c.AdjustmentType == AdjustDurationType.Seconds)
        {
            subtitle.AdjustDisplayTimeUsingSeconds(c.Seconds, null, shotChanges, true);
        }

        return subtitle;
    }

    private async Task<Subtitle> AutoTranslate(Subtitle subtitle, CancellationToken cancellationToken)
    {
        if (!_config.AutoTranslate.IsActive)
        {
            return subtitle;
        }

        Configuration.Settings.Tools.OllamaPrompt = Se.Settings.AutoTranslate.OllamaPrompt;
        Configuration.Settings.Tools.OllamaApiUrl = Se.Settings.AutoTranslate.OllamaUrl;
        Configuration.Settings.Tools.OllamaModel = Se.Settings.AutoTranslate.OllamaModel;
        var doAutoTranslate = new DoAutoTranslate();
        var translatedSubtitle = await doAutoTranslate.DoTranslate(subtitle, _config.AutoTranslate.SourceLanguage, _config.AutoTranslate.TargetLanguage,
            _config.AutoTranslate.Translator, default);

        for (var i = 0; i < subtitle.Paragraphs.Count && i < translatedSubtitle.Count; i++)
        {
            subtitle.Paragraphs[i].Text = translatedSubtitle[i].TranslatedText;
        }

        return subtitle;
    }

    private Subtitle RemoveTextForHearingImpaired(Subtitle subtitle)
    {
        if (!_config.RemoveTextForHearingImpaired.IsActive)
        {
            return subtitle;
        }

        return subtitle;
    }

    private Subtitle MergeLinesWithSameTimeCodes(Subtitle subtitle)
    {
        if (!_config.MergeLinesWithSameTimeCodes.IsActive)
        {
            return subtitle;
        }

        return subtitle;
    }


    private Subtitle MergeLinesWithSameText(Subtitle subtitle)
    {
        if (!_config.MergeLinesWithSameTexts.IsActive)
        {
            return subtitle;
        }

        var mergedIndexes = new List<int>();
        var removed = new HashSet<int>();
        var maxMsBetween = _config.MergeLinesWithSameTexts.MaxMillisecondsBetweenLines;
        var fixIncrementing = _config.MergeLinesWithSameTexts.IncludeIncrementingLines;
        var numberOfMerges = 0;
        Paragraph? p = null;
        var lineNumbers = new List<int>();
        for (var i = 1; i < subtitle.Paragraphs.Count; i++)
        {
            if (removed.Contains(i - 1))
            {
                continue;
            }

            p = subtitle.Paragraphs[i - 1];

            for (var j = i; j < subtitle.Paragraphs.Count; j++)
            {
                if (removed.Contains(j))
                {
                    continue;
                }

                var next= subtitle.Paragraphs[j];
                var incrementText = string.Empty;
                if ((MergeLinesSameTextUtils.QualifiesForMerge(p, next, maxMsBetween) ||
                     fixIncrementing && MergeLinesSameTextUtils.QualifiesForMergeIncrement(p, next, maxMsBetween, out incrementText)))
                {
                    p.Text = next.Text;
                    p.EndTime.TotalMilliseconds = next.EndTime.TotalMilliseconds;
                    if (!string.IsNullOrEmpty(incrementText))
                    {
                        p.Text = incrementText;
                    }

                    if (lineNumbers.Count > 0)
                    {
                        lineNumbers.Add(next.Number);
                    }
                    else
                    {
                        lineNumbers.Add(p.Number);
                        lineNumbers.Add(next.Number);
                    }

                    removed.Add(j);
                    numberOfMerges++;
                    if (!mergedIndexes.Contains(j))
                    {
                        mergedIndexes.Add(j);
                    }

                    if (!mergedIndexes.Contains(i - 1))
                    {
                        mergedIndexes.Add(i - 1);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        return subtitle;
    }

    private string MakeOutputFileName(BatchConvertItem item, SubtitleFormat format)
    {
        var outputFolder = _config.SaveInSourceFolder || string.IsNullOrEmpty(_config.OutputFolder)
            ? Path.GetDirectoryName(item.FileName)
            : _config.OutputFolder;
        if (string.IsNullOrEmpty(outputFolder))
        {
            throw new InvalidOperationException("Output folder is not set");
        }

        var fileName = Path.GetFileNameWithoutExtension(item.FileName);
        var targetExtension = format.Extension;
        var outputFileName = Path.Combine(outputFolder, fileName + targetExtension);
        if (!File.Exists(outputFileName))
        {
            return outputFileName;
        }

        if (_config.Overwrite)
        {
            File.Delete(outputFileName);
        }
        else
        {
            var counter = 1;
            do
            {
                outputFileName = Path.Combine(outputFolder, fileName + $"_{counter}" + targetExtension);
                counter++;
            } while (File.Exists(outputFileName));
        }

        return outputFileName;
    }

    public bool AllowFix(Paragraph p, string action)
    {
        return true;
    }

    public void AddFixToListView(Paragraph p, string action, string before, string after)
    {
    }

    public void AddFixToListView(Paragraph p, string action, string before, string after, bool isChecked)
    {
    }

    public void LogStatus(string sender, string message)
    {
    }

    public void LogStatus(string sender, string message, bool isImportant)
    {
    }

    public void UpdateFixStatus(int fixes, string message)
    {
    }

    public bool IsName(string candidate)
    {
        return false; //TODO: fix name checking
    }

    public HashSet<string> GetAbbreviations()
    {
        return new HashSet<string>(); //TODO: fix abbreviation checking
    }

    public void AddToTotalErrors(int count)
    {
    }

    public void AddToDeleteIndices(int index)
    {
    }
}