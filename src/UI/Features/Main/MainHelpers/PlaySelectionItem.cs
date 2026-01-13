using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Main.MainHelpers;

public class PlaySelectionItem
{
    public double EndSeconds { get; set; } = -1;
    public int Index { get; set; } = -1;
    public bool Loop { get; set; }
    public List<SubtitleLineViewModel> Subtitles { get; set; } = new List<SubtitleLineViewModel>();

    public PlaySelectionItem(List<SubtitleLineViewModel> subtitles, TimeSpan endTime, bool loop)
    {
        Subtitles = subtitles;
        EndSeconds = endTime.TotalSeconds;
        Index = 0;
        Loop = loop;
    }

    public SubtitleLineViewModel? GetNextSubtitle()
    {
        if (Index < Subtitles.Count-1)
        {
            Index++;
            var s = Subtitles[Index];
            EndSeconds = s.EndTime.TotalSeconds;
            return s;
        }
        else
        {
            if (Loop)
            {
                Index = 0;
                var s = Subtitles[Index];
                EndSeconds = s.EndTime.TotalSeconds;
                return s;
            }

            return null;
        }
    }
}
