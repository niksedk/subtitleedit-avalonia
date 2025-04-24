namespace Nikse.SubtitleEdit.Logic.Config;

public class SeChangeCasing
{
    public bool ToNormalCasing { get; set; }
    public bool FixNames { get; set; }
    public bool OnlyChangeAllUppercaseLines { get; set; }
    public bool FixOnlyNames { get; set; }
    public bool ToUppercase { get; set; }
    public bool ToLowercase { get; set; }
    public string ExtraNames { get; set; }

    public SeChangeCasing()
    {
        ToNormalCasing = true;
        FixNames = true;
        ExtraNames = string.Empty;
    }
}