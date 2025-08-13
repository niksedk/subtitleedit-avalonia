namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class BurnInEffectItem(string name, BurnInEffectType type, string description)
{

    public string Name { get; set; } = name;
    public BurnInEffectType Type { get; set; } = type;
    public string Description { get; set; } = description;

    public override string ToString()
    {
        return Name;
    }

    public string ApplyEffect(string subtitleText, int screenWidth, int screenHeight, int fontSize)
    {
        if (Type == BurnInEffectType.FadeInOut)
        {
            return $"{{\\fad(250,250}}{subtitleText}";
        }
        else if (Type == BurnInEffectType.SlowFontSizeChange)
        {
            return $"{{\\t(\\fs60)}}{subtitleText}";
        }
        else if (Type == BurnInEffectType.IncreaseFontKerning)
        {
            return $"{{\\t(\\fsp4)}}{subtitleText}";
        }
        else if (Type == BurnInEffectType.FixRightToLeft)
        {
            // TODO: Implement fix right-to-left effect logic here
            return subtitleText;
        }

        return subtitleText; // No effect applied
    }

    public static BurnInEffectItem[] List()
    {
        return
        [
            new BurnInEffectItem("Fade in/out", BurnInEffectType.FadeInOut, "Fades the subtitles in and out."),
            new BurnInEffectItem("Font size change", BurnInEffectType.SlowFontSizeChange, "Slowly changes the font size of the subtitles."),
            new BurnInEffectItem("Increase font kerning", BurnInEffectType.IncreaseFontKerning, "Gradually increases the space between characters in the subtitles."),
            new BurnInEffectItem("Fix right-to-left", BurnInEffectType.FixRightToLeft, "Adjusts the subtitles for right-to-left languages."),   
        ];
    }
}