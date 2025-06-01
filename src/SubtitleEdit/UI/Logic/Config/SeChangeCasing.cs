namespace Nikse.SubtitleEdit.Logic.Config;

public class SeChangeCasing
{
    public bool NormalCasing { get; internal set; }
    public bool NormalCasingFixNames { get; internal set; }
    public bool NormalCasingOnlyUpper { get; internal set; }
    public bool FixNamesOnly { get; internal set; }
    public bool AllUppercase { get; internal set; }
    public bool AllLowercase { get; internal set; }

    public SeChangeCasing()
    {
        NormalCasing = true;
    }
}