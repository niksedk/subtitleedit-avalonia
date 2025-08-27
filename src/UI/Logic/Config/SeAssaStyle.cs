using Nikse.SubtitleEdit.Features.Assa;

namespace Nikse.SubtitleEdit.Logic.Config;
public class SeAssaStyle
{
    public SeAssaStyle(StyleDisplay style)
    {
        Name = style.Name;
        FontName = style.FontName;
        FontSize = style.FontSize;
        ColorPrimary = style.ColorPrimary;
        ColorSecondary = style.ColorSecondary;
        ColorOutline = style.ColorOutline;
        ColorShadow = style.ColorShadow;
        OutlineWidth = style.OutlineWidth;
        ShadowWidth = style.ShadowWidth;
        Bold = style.Bold;
        Italic = style.Italic;
        Underline = style.Underline;
        Strikeout = style.Strikeout;
        ScaleX = style.ScaleX;
        ScaleY = style.ScaleY;
        Spacing = style.Spacing;
        Angle = style.Angle;
        Alignment = style.GetAlignment();
        MarginLeft = style.MarginLeft;
        MarginRight = style.MarginRight;
        MarginVertical = style.MarginVertical;
        UseOpaqueBox = style.UseOpaqueBox;
        UseOpaqueBoxPerLine = style.UseOpaqueBoxPerLine;
    }

    public string Name { get; set; } = string.Empty;
    public string FontName { get; set; } = string.Empty;
    public decimal FontSize { get; set; }
    public int UsageCount { get; set; }
    public Avalonia.Media.Color ColorPrimary { get; set; }
    public Avalonia.Media.Color ColorSecondary { get; set; }
    public Avalonia.Media.Color ColorOutline { get; set; }
    public Avalonia.Media.Color ColorShadow { get; set; }
    public decimal OutlineWidth { get; set; }
    public decimal ShadowWidth { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public bool Strikeout { get; set; }
    public decimal ScaleX { get; set; }
    public decimal ScaleY { get; set; }
    public decimal Spacing { get; set; }
    public decimal Angle { get; set; }
    public string Alignment { get; set; } = string.Empty;
    public decimal MarginLeft { get; set; }
    public decimal MarginRight { get; set; }
    public decimal MarginVertical { get; set; }
    public bool UseOpaqueBox { get; set; }
    public bool UseOpaqueBoxPerLine { get; set; }
}


