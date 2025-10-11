using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

public class ModifySelectionRule
{
    public RuleType RuleType { get; set; }
    public string Name { get; set; }

    public string Text { get; set; }
    public bool HasText { get; set; }
    public bool MatchCase { get; set; }

    public double Number { get; set; }
    public bool HasNumber { get; set; }
    public bool NumberDecimals { get; set; }
    public double NumberMinValue { get; set; }
    public double NumberMaxValue { get; set; }
    public double DefaultValue { get; set; }

    public ModifySelectionRule()
    {
        Name = string.Empty;
        Text = string.Empty;
        HasText = false;
        MatchCase = false;
        Number = 0;
        HasNumber = false;
        NumberDecimals = false;
        NumberMinValue = 0;
        NumberMaxValue = 0;
        DefaultValue = 0;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<ModifySelectionRule> List()
    {
        var l = Se.Language.Edit.ModifySelection;
        var g = Se.Language.General;

        return new List<ModifySelectionRule>
        {
            new() { RuleType = RuleType.Contains,            Name = l.Contains, HasText = true },
            new() { RuleType = RuleType.StartsWith,          Name = l.StartsWith, HasText = true },
            new() { RuleType = RuleType.EndsWith,            Name = l.EndsWith, HasText = true },
            new() { RuleType = RuleType.NotContains,         Name = l.NotContains, HasText = true },
            new() { RuleType = RuleType.AllUppercase,        Name = l.AllUppercase },
            new() { RuleType = RuleType.RegEx,               Name = g.RegularExpression, HasText = true },
            new() { RuleType = RuleType.Odd,                 Name = l.Odd },
            new() { RuleType = RuleType.Even,                Name = l.Even },
            new() { RuleType = RuleType.DurationLessThan,    Name = l.DurationLessThan, HasNumber = true, NumberDecimals = true, NumberMinValue = 0, NumberMaxValue = 60, DefaultValue = 2.0 },
            new() { RuleType = RuleType.DurationGreaterThan, Name = l.DurationGreaterThan, HasNumber = true, NumberDecimals = true, NumberMinValue = 0, NumberMaxValue = 60, DefaultValue = 2.0 },
            new() { RuleType = RuleType.CpsLessThan,         Name = l.CpsLessThan, HasNumber = true, NumberDecimals = true, NumberMinValue = 0, NumberMaxValue = 99, DefaultValue = 15 },
            new() { RuleType = RuleType.CpsGreaterThan,      Name = l.CpsGreaterThan, HasNumber = true, NumberDecimals = true, NumberMinValue = 0, NumberMaxValue = 99, DefaultValue = 20 },
            new() { RuleType = RuleType.LengthLessThan,      Name = l.LengthLessThan, HasNumber = true, NumberMinValue = 0, NumberMaxValue = 200, DefaultValue = 42 },
            new() { RuleType = RuleType.LengthGreaterThan,   Name = l.LengthGreaterThan, HasNumber = true, NumberMinValue = 0, NumberMaxValue = 200, DefaultValue = 42 },
            new() { RuleType = RuleType.ExactlyOneLine,      Name = l.ExactlyOneLine },
            new() { RuleType = RuleType.ExactlyTwoLines,     Name = l.ExactlyTwoLines },
            new() { RuleType = RuleType.MoreThanTwoLines,    Name = l.MoreThanTwoLines },
            new() { RuleType = RuleType.Bookmarked,          Name = l.Bookmarked },
            new() { RuleType = RuleType.BookmarkContains,    Name = l.BookmarkContains, HasText = true },
            new() { RuleType = RuleType.BlankLines,          Name = l.BlankLines },
            new() { RuleType = RuleType.Style,               Name = g.Style, HasText = true },
            new() { RuleType = RuleType.Actor,               Name = g.Actor, HasText = true },

        };
    }
}
