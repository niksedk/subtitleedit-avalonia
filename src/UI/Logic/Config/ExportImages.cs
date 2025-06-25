using SkiaSharp;

namespace Nikse.SubtitleEdit.Logic.Config;

public class ExportImages
{
    public string FontName { get;  set; }
    public int FontSize { get;  set; }
    public int ResolutionWidth { get;  set; }
    public int ResolutionHeight { get;  set; }
    public string Alignment { get;  set; }
    public string BorderStyle { get;  set; }
    public string Profile { get;  set; }
    public int BottomMargin { get;  set; }
    public int BottomMarginUnit { get;  set; }
    public int LeftRightMargin { get;  set; }
    public int LeftRightMarginUnit { get;  set; }
    public decimal FontKerningExtra { get; set; }
    public decimal FrameRate { get; set; }
    public string FontColor { get;  set; }
    public string BorderColor { get;  set; }
    public float BorderWidth { get; set; }
    public int BorderBoxCornerRadius { get; set; }
    public string ShadowColor { get;  set; }
    public float ShadowWidth { get; set; }
    public int ShadowAlpha { get;  set; }
    public bool IsBold { get;  set; }

    public ExportImages()
    {
        FontName = "Arial";
        FontSize = 24;
        ResolutionWidth = 1920;
        ResolutionHeight = 1080;
        Alignment = "Center";
        BorderStyle = "None";
        Profile = "None";
        BottomMargin = 0;
        BottomMarginUnit = 0;
        LeftRightMargin = 0;
        LeftRightMarginUnit = 0;
        FontKerningExtra = 0;
        FrameRate = 24;
        FontColor = SKColors.White.ToString();
        BorderColor = SKColors.Black.ToHex();
        ShadowColor = SKColors.Black.ToHex();
        ShadowWidth = 3;
        ShadowAlpha = 200;
    }
}