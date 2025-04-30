
using System.Collections.Generic;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Logic;

public interface IInsertManager
{
    SubtitleLineViewModel InsertAfter(List<SubtitleLineViewModel> paragraphs, int[] selectedIndices, string text, int minGapBetweenLines);
    SubtitleLineViewModel InsertBefore(List<SubtitleLineViewModel> paragraphs, int[] selectedIndices, string text, int minGapBetweenLines);
}