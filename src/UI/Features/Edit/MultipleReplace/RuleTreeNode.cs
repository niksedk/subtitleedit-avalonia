using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic;
using Projektanker.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public partial class RuleTreeNode : ObservableObject
{
    [ObservableProperty] private string _title;
    public ObservableCollection<RuleTreeNode>? SubNodes { get; }
    public string Find { get; set; } 
    public string ReplaceWith { get; set; } 
    public string Description { get; set; } 
    public bool IsActive { get; set; } = false;
    public bool IsCategory { get; set; } = false;
    public string IconName { get; set; }
    
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