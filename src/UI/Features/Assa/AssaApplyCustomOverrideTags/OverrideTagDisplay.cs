using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;

public class OverrideTagDisplay
{
    public string Name { get; set; }
    public string Tag { get; set; }
    public OverrideTagDisplay(string name, string tag)
    {
        Name = name;
        Tag = tag;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<OverrideTagDisplay> List()
    {
        return new List<OverrideTagDisplay>
        {
            new OverrideTagDisplay("Font size change", "{\\t(\\fs60)}"),
            new OverrideTagDisplay("Move text from left to right", "{\\move(350,350,1500,350)}Move test"),
            new OverrideTagDisplay("Color from white to red", "{\\1c&HFFFFFF&\\t(\\1c&H0000FF&)}"),
            new OverrideTagDisplay("Rotate X (slow)", "{\\t(\\frx25)}"),
            new OverrideTagDisplay("Rotate X", "{\\t(\\frx360)}"),
            new OverrideTagDisplay("Rotate Y", "{\\t(\\fry360)}"),
            new OverrideTagDisplay("Rotate (tilt)", "{\\t(\\fr5\\fr0)}"),
            new OverrideTagDisplay("Fade", "{\\fad(300,300}"),
            new OverrideTagDisplay("Space increase (slow)", "{\\t(\\fsp4)}"),
        };
    }   
}
