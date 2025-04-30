using Nikse.SubtitleEdit.Core.Common;
using SubtitleAlchemist.Logic;

namespace Nikse.SubtitleEdit.Logic;

public interface IMergeManager
{
    Subtitle MergeSelectedLines(Subtitle subtitle, int[] selectedIndices, MergeManager.BreakMode breakMode = MergeManager.BreakMode.Normal);
}