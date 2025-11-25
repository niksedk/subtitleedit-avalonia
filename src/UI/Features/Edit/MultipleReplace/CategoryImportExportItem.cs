using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class CategoryImportExportItem
{
    public class RuleImportExportItem
    {
        public RuleImportExportItem()
        {
        }

        public RuleImportExportItem(RuleTreeNode node)
        {
            Find = node.Find;
            ReplaceWith = node.ReplaceWith;
            Description = node.Description;
            IsActive = node.IsActive;
            IsCategory = node.IsCategory;
            Type = node.Type.ToString();
        }

        public List<RuleImportExportItem>? SubNodes { get; set; }
        public string Find { get; set; } = string.Empty;
        public string ReplaceWith { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
        public bool IsCategory { get; set; } = false;
        private string Type { get; set; } = string.Empty;
    }

    public List<RuleImportExportItem>? Rules { get; set; }


    public CategoryImportExportItem()
    {
            
    }

    public CategoryImportExportItem(List<RuleTreeNode> ruleNode)
    {
        Rules = new List<RuleImportExportItem>();
        foreach (var node in ruleNode)
        {
            Rules.Add(new RuleImportExportItem(node));
        }
    }
}

