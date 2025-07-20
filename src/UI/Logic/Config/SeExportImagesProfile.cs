using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using SkiaSharp;
using System;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeExportImagesProfile
{
    public string ProfileName { get; set; } = "Default";
    public ExportAlignment Alignment { get; set; }
    public SKBitmap Bitmap { get; set; }
    public string Text { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Index { get; set; }
    public SKColor FontColor { get; set; }
    public string FontName { get; set; }
    public float FontSize { get; set; }
    public bool IsBold { get; set; }
    public SKColor OutlineColor { get; set; }
    public double OutlineWidth { get; set; }
    public SKColor ShadowColor { get; set; }
    public double ShadowWidth { get; set; }
    public SKColor BackgroundColor { get; set; }
    public double BackgroundCornerRadius { get; set; }
    public byte[] Buffer { get; set; }
    public int ScreenWidth { get; set; }
    public int ScreenHeight { get; set; }
    public int BottomTopMargin { get; set; }
    public int LeftRightMargin { get; set; }
    public SKPointI? OverridePosition { get; set; }
    public string Error { get; set; }
    public bool IsForced { get; set; }
    public bool IsFullFrame { get; set; }
    public double FramesPerSecond { get; set; }

    public SeExportImagesProfile()
    {
        Alignment = ExportAlignment.BottomCenter;
        Bitmap = new SKBitmap(1, 1, true);
        Text = string.Empty;
        FontColor = SKColors.White;
        FontName = string.Empty;
        FontSize = 26;
        OutlineColor = SKColors.Black;
        OutlineWidth = 2;
        ShadowColor = SKColors.Black;
        ShadowWidth = 2;
        BackgroundColor = SKColors.Transparent;
        BackgroundCornerRadius = 0;
        ScreenWidth = 1920;
        ScreenHeight = 1080;
        BottomTopMargin = 10;
        LeftRightMargin = 10;
        Error = string.Empty;
        FramesPerSecond = 25;

    }
}