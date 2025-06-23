using System.Collections.Generic;
using static Nikse.SubtitleEdit.Logic.FindService;

namespace Nikse.SubtitleEdit.Logic;

public interface IFindService
{
    string SearchText { get; set; } 
    int CurrentIndex { get; set; } 
    bool WholeWord { get; set; }
    FindMode CurrentFindMode { get; set; }

    void Initialize(List<string> items, int currentIndex, bool wholeWord, FindMode findMode);

    int Find(string searchText);

    int FindNext(string searchText, List<string> items);
}