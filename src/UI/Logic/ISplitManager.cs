using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Logic;

public interface ISplitManager
{
    void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle);
    void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds);
    void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds, int textIndex);
}

public class SplitManager : ISplitManager
{
    public void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle)
    {
        var idx = subtitles.IndexOf(subtitle);
        if (idx < 0 || idx >= subtitles.Count)
        {
            return; // Subtitle not found in the collection
        }

        var newSubtitle = new SubtitleLineViewModel(subtitle, true);
        var gap = Se.Settings.General.MinimumMillisecondsBetweenLines / 2.0;
        newSubtitle.SetStartTimeOnly(TimeSpan.FromMilliseconds(subtitle.StartTime.TotalMilliseconds + subtitle.Duration.TotalMilliseconds / 2.0 + gap));
        subtitle.EndTime = TimeSpan.FromMilliseconds(newSubtitle.StartTime.TotalMilliseconds - gap);

        var text = subtitle.Text;
        var lines = text.SplitToLines();
        if (lines.Count == 2)
        {
            newSubtitle.Text = lines[1].Trim();
            subtitle.Text = lines[0].Trim();
        }
        else if (lines.Count > 2)
        {
            var splitIndex = lines.Count / 2;
            subtitle.Text = string.Join(Environment.NewLine, lines, 0, splitIndex).Trim();
            newSubtitle.Text = string.Join(Environment.NewLine, lines, splitIndex, lines.Count - splitIndex).Trim();
        }
        else
        {
            var middleIndex = text.Length / 2;
            var splitIndex = middleIndex;
            while (splitIndex > 0 && !char.IsWhiteSpace(text[splitIndex]) && !char.IsPunctuation(text[splitIndex]))
            {
                splitIndex--;
            }
            subtitle.Text = text.Substring(0, splitIndex).Trim();
            newSubtitle.Text = text.Substring(splitIndex).Trim();
        }

        subtitles.Insert(idx + 1, newSubtitle);
    }

    public void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds)
    {
        
    }

    public void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds, int textIndex)
    {
        
    }
}