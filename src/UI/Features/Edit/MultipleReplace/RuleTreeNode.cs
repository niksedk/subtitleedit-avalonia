using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public partial class RuleTreeNode : ObservableObject
{
    [ObservableProperty] private string _title;
    public ObservableCollection<RuleTreeNode>? SubNodes { get; }
    public MultipleReplaceRule? Rule { get; set; }

    public RuleTreeNode(string title, MultipleReplaceRule rule)
    {
        Title = title;
        Rule = rule;
    }

    public RuleTreeNode(string title, ObservableCollection<RuleTreeNode> subNodes)
    {
        Title = title;
        SubNodes = subNodes;
    }
}