using Nikse.SubtitleEdit.Logic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class ShortcutTreeNode : ObservableObject
{
    [ObservableProperty] private string _title;
    public ObservableCollection<ShortcutTreeNode>? SubNodes { get; }
    public ShortCut? ShortCut { get; set; }

    public ShortcutTreeNode(string title, ShortCut shortcut)
    {
        Title = title;
        ShortCut = shortcut;
    }

    public ShortcutTreeNode(string title, ObservableCollection<ShortcutTreeNode> subNodes)
    {
        Title = title;
        SubNodes = subNodes;
    }
}