using System.Collections.Generic;
using SubtitleAlchemist.Logic;

namespace Nikse.SubtitleEdit.Logic;

public interface IFindService
{
    string SearchText { get; set; } 
    int CurrentIndex { get; set; } 
    bool WholeWord { get; set; }
    FindService.FindMode CurrentFindMode { get; set; }

    int Find(string searchText);

    int FindNext(string searchText, List<string> items);
}