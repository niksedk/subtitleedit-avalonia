using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Core.Common;
using System;
using SkiaSharp;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit;

public partial class BinarySubtitleItem : ObservableObject
{
    [ObservableProperty] private int _x;
    [ObservableProperty] private int _y;

    public BinarySubtitleItem(OcrSubtitleItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        Number = item.Number;
        IsForced = false; // OcrSubtitleItem does not expose forced flag; default to false
        Text = item.Text;

        // Store times as TimeSpan for use with TimeCodeUpDown and SecondsUpDown controls
        StartTime = item.StartTime;
        EndTime = item.EndTime;
        Duration = item.Duration;

        ScreenSize = item.GetScreenSize();
        
        // Get bitmap (cropped to remove transparent borders)
        try
        {
            Bitmap = item.GetBitmapCropped();
        }
        catch
        {
            Bitmap = null;
        }

        // Position (x,y) from OcrSubtitleItem
        try
        {
            var pos = item.GetPosition();
            _x = pos.X;
            _y = pos.Y;
        }
        catch
        {
            _x = 0;
            _y = 0;
        }
    }
    
    public int Number { get; set; }
    public bool IsForced { get; set; }
    public Bitmap? Bitmap { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string Text { get; set; }
    public SKSizeI ScreenSize { get; set; }
}