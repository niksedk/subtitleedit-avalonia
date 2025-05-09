using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Logic;

public interface IMergeManager
{
    Subtitle MergeSelectedLines(Subtitle subtitle, int[] selectedIndices, MergeManager.BreakMode breakMode = MergeManager.BreakMode.Normal);
}