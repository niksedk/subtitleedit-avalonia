using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.MergeShortLines;

public class MergeShortLinesResult
{
    public List<SubtitleLineViewModel> MergedSubtitles { get; }
    public List<MergeShortLinesItem> Fixes { get; }
    public int MergeCount { get; }

    public MergeShortLinesResult(List<SubtitleLineViewModel> mergedSubtitles, List<MergeShortLinesItem> fixes, int mergeCount)
    {
        MergedSubtitles = mergedSubtitles;
        Fixes = fixes;
        MergeCount = mergeCount;
    }
}

public static class MergeShortLinesHelper
{
    public static MergeShortLinesResult Merge(
        List<SubtitleLineViewModel> subtitles,
        List<double> shotChanges,
        int singleLineMaxLength,
        int maxNumberOfLines,
        int gapThresholdMs,
        int unbreakLinesShorterThan)
    {
        var fixes = new List<MergeShortLinesItem>();
        var mergeCount = 0;
        var maxCharactersPerSubtitle = maxNumberOfLines * singleLineMaxLength;

        var result = new List<SubtitleLineViewModel>(subtitles.Count);

        for (var index = 0; index < subtitles.Count; index++)
        {
            var baseVm = subtitles[index];
            var current = new SubtitleLineViewModel(baseVm);

            var j = index + 1;
            while (j < subtitles.Count)
            {
                var next = subtitles[j];

                // stop if there is a shot change between current and next
                var hasShotChangeBetween = shotChanges != null && shotChanges.Any(s =>
                    s > current.EndTime.TotalSeconds && s < next.StartTime.TotalSeconds);
                if (hasShotChangeBetween)
                {
                    break;
                }

                // Check temporal proximity
                var gapMs = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;
                if (gapMs > gapThresholdMs)
                {
                    break;
                }

                // Check combined plain length limit
                var combinedPlain = HtmlUtil.RemoveHtmlTags((current.Text ?? string.Empty) + " " + (next.Text ?? string.Empty), true)
                    .Replace("\r\n", " ").Replace('\n', ' ').Trim();
                if (combinedPlain.Length > maxCharactersPerSubtitle)
                {
                    break;
                }

                // Try to wrap combined text within singleLineMaxLength and maxNumberOfLines
                var language = string.IsNullOrWhiteSpace(baseVm.Language) ? "en" : baseVm.Language;
                var combinedRaw = (current.Text ?? string.Empty).TrimEnd() + " " + (next.Text ?? string.Empty).TrimStart();
                var wrapped = Utilities.AutoBreakLine(combinedRaw, singleLineMaxLength, unbreakLinesShorterThan, language);
                var lines = wrapped.SplitToLines();
                if (lines.Count > maxNumberOfLines)
                {
                    break;
                }

                // Check that each line doesn't exceed singleLineMaxLength
                var anyLineTooLong = lines.Any(line => HtmlUtil.RemoveHtmlTags(line, true).Length > singleLineMaxLength);
                if (anyLineTooLong)
                {
                    break;
                }

                // Merge
                current.Text = wrapped;
                current.EndTime = next.EndTime;
                current.UpdateDuration();
                mergeCount++;

                // fix item for this merge step
                var fix = new MergeShortLinesItem(
                    "Merge short lines",
                    index + 1,
                    $"Merged line {j + 1} into {index + 1}",
                    new SubtitleLineViewModel(current));
                fixes.Add(fix);

                j++;
            }

            result.Add(current);
            // Skip the lines we merged into current
            index = j - 1;
        }

        return new MergeShortLinesResult(result, fixes, mergeCount);
    }
}
