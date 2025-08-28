using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public static class DurationsBridgeGaps2
    {
        public static int BridgeGaps(ObservableCollection<SubtitleLineViewModel> subtitles, int minMsBetweenLines, int percentageForPrevious, double maxMs, List<int> fixedIndexes, Dictionary<string, string> dic, bool useFrames)
        {
            int fixedCount = 0;
            if (minMsBetweenLines > maxMs)
            {
                string message = $"{nameof(DurationsBridgeGaps)}: {nameof(minMsBetweenLines)} cannot be larger than {nameof(maxMs)}!";
                Se.LogError(message);
                return 0;
            }

            int count = subtitles.Count - 1;
            for (int i = 0; i < count; i++)
            {
                var cur = subtitles[i];
                var next = subtitles[i + 1];

                double currentGap = next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds;

                // there shouldn't be adjustment if current gaps is shorter or equal than minimum gap or greater than maximum gaps
                if (currentGap <= minMsBetweenLines || currentGap > maxMs)
                {
                    continue;
                }

                // Calculate how much of the gap to allocate to the previous subtitle
                double gapToBridge = currentGap - minMsBetweenLines;
                double previousExtension = (gapToBridge * percentageForPrevious) / 100.0;
                double nextReduction = gapToBridge - previousExtension;

                // Extend the current subtitle by the calculated amount
                cur.EndTime = TimeSpan.FromMilliseconds(cur.EndTime.TotalMilliseconds + previousExtension);

                // Move the next subtitle's start time to maintain the minimum gap
                next.StartTime = TimeSpan.FromMilliseconds(cur.EndTime.TotalMilliseconds + minMsBetweenLines);

                if (fixedIndexes != null)
                {
                    fixedIndexes.Add(i);
                    fixedIndexes.Add(i + 1);
                }
                fixedCount++;

                double newGap = next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds;
                if (useFrames)
                {
                    dic?.Add(cur.Id.ToString(), $"{SubtitleFormat.MillisecondsToFrames(currentGap)} => {SubtitleFormat.MillisecondsToFrames(newGap)}");
                }
                else
                {
                    dic?.Add(cur.Id.ToString(), $"{currentGap / TimeCode.BaseUnit:0.000} => {newGap / TimeCode.BaseUnit:0.000}");
                }
            }

            return fixedCount;
        }
    }
}
