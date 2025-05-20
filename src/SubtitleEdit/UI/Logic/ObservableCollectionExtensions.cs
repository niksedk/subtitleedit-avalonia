using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Logic;

public static class ObservableCollectionExtensions
{
    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    public static SubtitleLineViewModel? GetOrNull(this ObservableCollection<SubtitleLineViewModel> collection, int index)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        return (index >= 0 && index < collection.Count) ? collection[index] : null;
    }
}