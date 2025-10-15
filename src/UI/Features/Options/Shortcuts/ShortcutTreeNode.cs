using Nikse.SubtitleEdit.Logic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class ShortcutTreeNode : ObservableObject
{
    [ObservableProperty] private string _category;
    [ObservableProperty] private string _title;
    public ObservableCollection<ShortcutTreeNode>? SubNodes { get; }
    public ShortCut? ShortCut { get; set; }

    public ShortcutTreeNode(string category, string title, ShortCut shortcut)
    {
        Category = category;
        Title = title;
        ShortCut = shortcut;
    }

    public ShortcutTreeNode(string category, string title, ObservableCollection<ShortcutTreeNode> subNodes)
    {
        Category = category;
        Title = title;
        SubNodes = subNodes;
    }
}