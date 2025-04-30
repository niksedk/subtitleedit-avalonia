using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Logic
{
    public class InsertManager : IInsertManager
    {
        public SubtitleLineViewModel InsertAfter(List<SubtitleLineViewModel> paragraphs, int[] selectedIndices, string text, int minGapBetweenLines)
        {
            int firstSelectedIndex = 0;
            if (selectedIndices.Length > 0)
            {
                firstSelectedIndex = selectedIndices[0];
            }

            var newParagraph = new Paragraph { Text = text };

            //SetStyleForNewParagraph(newParagraph, firstSelectedIndex);

            var prev = firstSelectedIndex >= 0 ? paragraphs[firstSelectedIndex] : null;
            var next = firstSelectedIndex +1 < paragraphs.Count ? paragraphs[firstSelectedIndex + 1] : null;

            var addMilliseconds = minGapBetweenLines;
            if (addMilliseconds < 1)
            {
                addMilliseconds = 0;
            }

            if (prev != null)
            {
                newParagraph.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + addMilliseconds;
                newParagraph.EndTime.TotalMilliseconds = newParagraph.StartTime.TotalMilliseconds + Configuration.Settings.General.NewEmptyDefaultMs;
                if (next != null && newParagraph.EndTime.TotalMilliseconds > next.StartTime.TotalMilliseconds)
                {
                    newParagraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - minGapBetweenLines;
                }

                if (newParagraph.StartTime.TotalMilliseconds > newParagraph.EndTime.TotalMilliseconds)
                {
                    newParagraph.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + addMilliseconds;
                }

                if (next != null && next.StartTime.IsMaxTime() && prev.EndTime.IsMaxTime())
                {
                    newParagraph.StartTime.TotalMilliseconds = TimeCode.MaxTimeTotalMilliseconds;
                    newParagraph.EndTime.TotalMilliseconds = TimeCode.MaxTimeTotalMilliseconds;
                }
                else if (next != null && next.StartTime.TotalMilliseconds == 0 && prev.EndTime.TotalMilliseconds == 0)
                {
                    newParagraph.StartTime.TotalMilliseconds = 0;
                    newParagraph.EndTime.TotalMilliseconds = 0;
                }
                else if (next == null && prev.EndTime.IsMaxTime())
                {
                    newParagraph.StartTime.TotalMilliseconds = TimeCode.MaxTimeTotalMilliseconds;
                    newParagraph.EndTime.TotalMilliseconds = TimeCode.MaxTimeTotalMilliseconds;
                }
                else if (next == null && prev.EndTime.TotalMilliseconds == 0)
                {
                    newParagraph.StartTime.TotalMilliseconds = 0;
                    newParagraph.EndTime.TotalMilliseconds = 0;
                }
                else if (next != null &&
                         prev.StartTime.TotalMilliseconds == next.StartTime.TotalMilliseconds &&
                         prev.EndTime.TotalMilliseconds == next.EndTime.TotalMilliseconds)
                {
                    newParagraph.StartTime.TotalMilliseconds = prev.StartTime.TotalMilliseconds;
                    newParagraph.EndTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds;
                }
            }
            else if (next != null)
            {
                newParagraph.StartTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 2000;
                newParagraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - addMilliseconds;
            }
            else
            {
                newParagraph.StartTime.TotalMilliseconds = 1000;
                newParagraph.EndTime.TotalMilliseconds = 3000;
                if (newParagraph.DurationTotalMilliseconds < Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                {
                    newParagraph.EndTime.TotalMilliseconds = newParagraph.StartTime.TotalMilliseconds +
                                                             Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
                }
            }

            if (newParagraph.Duration.TotalMilliseconds < 100)
            {
                newParagraph.EndTime.TotalMilliseconds = newParagraph.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
            }

            var dp = new SubtitleLineViewModel(newParagraph);
            var insertIndex = firstSelectedIndex + 1;
            if (insertIndex >= paragraphs.Count)
            {
                paragraphs.Add(dp);
            }
            else
            {
                paragraphs.Insert(firstSelectedIndex, dp);
            }
            return dp;
        }

        public SubtitleLineViewModel InsertBefore(List<SubtitleLineViewModel> paragraphs, int[] selectedIndices, string text, int minGapBetweenLines)
        {
            int firstSelectedIndex = 0;
            if (selectedIndices.Length > 0)
            {
                firstSelectedIndex = selectedIndices[0];
            }

            int addMilliseconds = minGapBetweenLines + 1;
            if (addMilliseconds < 1)
            {
                addMilliseconds = 1;
            }

            var newParagraph = new Paragraph() { Text = text };

            //SetStyleForNewParagraph(newParagraph, firstSelectedIndex);

            var prev = firstSelectedIndex > 0 ? paragraphs[firstSelectedIndex - 1] : null;
            var next = firstSelectedIndex < paragraphs.Count ? paragraphs[firstSelectedIndex] : null;
            if (prev != null && next != null)
            {
                newParagraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - addMilliseconds;
                newParagraph.StartTime.TotalMilliseconds = newParagraph.EndTime.TotalMilliseconds - 2000;
                if (newParagraph.StartTime.TotalMilliseconds <= prev.EndTime.TotalMilliseconds)
                {
                    newParagraph.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                }

                if (newParagraph.DurationTotalMilliseconds < 100)
                {
                    newParagraph.EndTime.TotalMilliseconds += 100;
                }

                if (next.StartTime.IsMaxTime() && prev.EndTime.IsMaxTime())
                {
                    newParagraph.StartTime.TotalMilliseconds = TimeCode.MaxTimeTotalMilliseconds;
                    newParagraph.EndTime.TotalMilliseconds = TimeCode.MaxTimeTotalMilliseconds;
                }
                else if (next.StartTime.TotalMilliseconds == 0 && prev.EndTime.TotalMilliseconds == 0)
                {
                    newParagraph.StartTime.TotalMilliseconds = 0;
                    newParagraph.EndTime.TotalMilliseconds = 0;
                }
                else if (prev.StartTime.TotalMilliseconds == next.StartTime.TotalMilliseconds &&
                         prev.EndTime.TotalMilliseconds == next.EndTime.TotalMilliseconds)
                {
                    newParagraph.StartTime.TotalMilliseconds = prev.StartTime.TotalMilliseconds;
                    newParagraph.EndTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds;
                }
            }
            else if (prev != null)
            {
                newParagraph.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + addMilliseconds;
                newParagraph.EndTime.TotalMilliseconds = newParagraph.StartTime.TotalMilliseconds + Configuration.Settings.General.NewEmptyDefaultMs;
                if (newParagraph.StartTime.TotalMilliseconds > newParagraph.EndTime.TotalMilliseconds)
                {
                    newParagraph.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                }
            }
            else if (next != null)
            {
                newParagraph.StartTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - (2000 + minGapBetweenLines);
                newParagraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - minGapBetweenLines;

                if (next.StartTime.IsMaxTime())
                {
                    newParagraph.StartTime.TotalMilliseconds = TimeCode.MaxTimeTotalMilliseconds;
                    newParagraph.EndTime.TotalMilliseconds = TimeCode.MaxTimeTotalMilliseconds;
                }
                else if (next.StartTime.TotalMilliseconds == 0 && next.EndTime.TotalMilliseconds == 0)
                {
                    newParagraph.StartTime.TotalMilliseconds = 0;
                    newParagraph.EndTime.TotalMilliseconds = 0;
                }
            }
            else
            {
                newParagraph.StartTime.TotalMilliseconds = 1000;
                newParagraph.EndTime.TotalMilliseconds = 3000;
                if (newParagraph.DurationTotalMilliseconds < Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                {
                    newParagraph.EndTime.TotalMilliseconds = newParagraph.StartTime.TotalMilliseconds +
                                                             Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
                }
            }

            var dp = new SubtitleLineViewModel(newParagraph);
            var insertIndex = firstSelectedIndex;
            if (insertIndex >= paragraphs.Count)
            {
                paragraphs.Add(dp);
            }
            else
            {
                paragraphs.Insert(firstSelectedIndex, dp);
            }
            return dp;
        }
    }
}
