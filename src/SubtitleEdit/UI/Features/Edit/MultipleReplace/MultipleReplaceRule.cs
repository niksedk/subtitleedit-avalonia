﻿namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class MultipleReplaceRule
{
    public string Find { get; set; } 
    public string ReplaceWith { get; set; } 
    public bool Description { get; set; } = false;
    public MultipleReplaceType Type { get; set; }
    
    public MultipleReplaceRule() 
    { 
        Find = string.Empty;
        ReplaceWith = string.Empty;
        Type = MultipleReplaceType.Normal;
    }
}
