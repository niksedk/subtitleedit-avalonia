using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public class ShortcutTreeNode
{
    public ObservableCollection<ShortcutTreeNode>? SubNodes { get; }
    public string Title { get; }
    public ShortCut? ShortCut { get; }
  
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