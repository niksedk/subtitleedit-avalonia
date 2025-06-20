using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public partial class RuleTreeNode : ObservableObject
{
    [ObservableProperty] private string _title;
    public ObservableCollection<RuleTreeNode>? SubNodes { get; }
    [ObservableProperty] private string _find;
    [ObservableProperty] private string _replaceWith;
    [ObservableProperty] private string _description;
    [ObservableProperty] private bool _isActive = false;
    [ObservableProperty] private bool _isCategory = false;
    [ObservableProperty] private string _iconName;

    public MultipleReplaceType Type { get; set; }

    public RuleTreeNode(string title, MultipleReplaceRule rule)
    {
        Title = title;
        Find = rule.Find;
        ReplaceWith = rule.ReplaceWith;
        Description = rule.Description;
        IsActive = rule.Active;
        IsCategory = false;
        Type = rule.Type;
        IconName = string.Empty;
        UpdateIconName();
    }

    private void UpdateIconName()
    {
        if (Type == MultipleReplaceType.RegularExpression)
        {
            IconName = IconNames.MdiRegex;
        }
        else if (Type == MultipleReplaceType.CaseInsensitive)
        {
            IconName = IconNames.MdiFindReplace;
        }
        else if (Type == MultipleReplaceType.CaseSensitive)
        {
            IconName =  IconNames.MdiCaseSensitiveAlt;
        }
    }

    public RuleTreeNode(string title, ObservableCollection<RuleTreeNode> subNodes, bool active)
    {
        Title = title;
        SubNodes = subNodes;
        Find = string.Empty;
        ReplaceWith = string.Empty;
        Description = string.Empty;
        IconName = string.Empty;
        IsActive = active;
        IsCategory = true;
    }
}