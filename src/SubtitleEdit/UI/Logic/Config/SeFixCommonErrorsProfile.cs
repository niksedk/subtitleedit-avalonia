using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeFixCommonErrorsProfile
{
    public string ProfileName { get; set; } = "Default";
    public List<string> SelectedRules { get; set; } = new();
}