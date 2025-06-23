using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

public partial class FindService : IFindService
{
    public string SearchText { get; set; } = string.Empty;
    public int CurrentIndex { get; set; } = -1;
    public bool WholeWord { get; set; }
    public FindMode CurrentFindMode { get; set; } = FindMode.CaseInsensitive;

    private List<string> _items = new List<string>();

    public FindService()
    {
    }

    public FindService(List<string> items, int currentIndex, bool wholeWord, FindMode findMode)
    {
        _items = items;
        CurrentIndex = currentIndex;
        WholeWord = wholeWord;
        CurrentFindMode = findMode;
    }

    public int Find(string searchText)
    {
        SearchText = searchText;
        CurrentIndex = FindInList(searchText, 0);
        return CurrentIndex;
    }

    public int FindNext(string searchText, List<string> items)
    {
        SearchText = searchText;
        _items = items;

        if (CurrentIndex == -1)
        {
            return -1;
        }

        var startIndex = (CurrentIndex + 1) % _items.Count;
        CurrentIndex = FindInList(searchText, startIndex);
        return CurrentIndex;
    }

    private int FindInList(string searchText, int startIndex)
    {
        for (var i = 0; i < _items.Count; i++)
        {
            var index = (startIndex + i) % _items.Count;
            if (MatchesSearchCriteria(_items[index], searchText))
            {
                return index;
            }
        }

        return -1;
    }

    private bool MatchesSearchCriteria(string item, string searchText)
    {
        switch (CurrentFindMode)
        {
            case FindMode.CaseSensitive:
                return WholeWord 
                    ? Regex.IsMatch(item, $@"\b{Regex.Escape(searchText)}\b", RegexOptions.IgnoreCase) 
                    : item.Contains(searchText, StringComparison.OrdinalIgnoreCase);

            case FindMode.CaseInsensitive:
                return WholeWord 
                    ? Regex.IsMatch(item, $@"\b{Regex.Escape(searchText)}\b") 
                    : item.Contains(searchText, StringComparison.Ordinal);

            case FindMode.RegularExpression:
                try
                {
                    return Regex.IsMatch(item, searchText);
                }
                catch (ArgumentException)
                {
                    // Invalid regex pattern
                    return false;
                }

            default:
                return false;
        }
    }
}
