using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public class ShortcutTreeNode
{
    public ObservableCollection<ShortcutTreeNode>? SubNodes { get; }
    public string Title { get; }
  
    public ShortcutTreeNode(string title)
    {
        Title = title;
    }

    public ShortcutTreeNode(string title, ObservableCollection<ShortcutTreeNode> subNodes)
    {
        Title = title;
        SubNodes = subNodes;
    }
}