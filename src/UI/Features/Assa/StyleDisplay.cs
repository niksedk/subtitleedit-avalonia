using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Assa;

public class StyleDisplay
{
    public string Name { get; set; } = string.Empty;
    public string FontName { get; set; } = string.Empty;    
    public decimal FontSize { get; set; }
    public int UsageCount { get; set; }
    public Color ColorPrimary { get; set; }
    public Color ColorSecondary { get; set; }
    public Color ColorOutline { get; set; }
    public Color ColorShadow { get; set; }
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
