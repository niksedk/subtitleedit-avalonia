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
    [ObservableProperty] private TimeSpan _startTime;
    [ObservableProperty] private TimeSpan _endTime;
    [ObservableProperty] private TimeSpan _duration;
    [ObservableProperty] private int _screenWidth;
    [ObservableProperty] private int _screenHeight;
    
    private bool _isUpdating;

    public BinarySubtitleItem(OcrSubtitleItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        Number = item.Number;
        IsForced = false; // OcrSubtitleItem does not expose forced flag; default to false
        Text = item.Text;

        // Store times as TimeSpan for use with TimeCodeUpDown and SecondsUpDown controls
        _startTime = item.StartTime;
        _endTime = item.EndTime;
        _duration = item.Duration;

        var screenSize = item.GetScreenSize();
        _screenWidth = screenSize.Width;
        _screenHeight = screenSize.Height;
        
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
    public string Text { get; set; }
    public SKSizeI ScreenSize => new SKSizeI(ScreenWidth, ScreenHeight);

    // When StartTime changes, update EndTime to maintain Duration
    partial void OnStartTimeChanged(TimeSpan value)
    {
        if (_isUpdating) return;
        
        _isUpdating = true;
        EndTime = value + _duration;
        _isUpdating = false;
    }

    // When Duration changes, update EndTime to maintain StartTime
    partial void OnDurationChanged(TimeSpan value)
    {
        if (_isUpdating) return;
        
        _isUpdating = true;
        EndTime = _startTime + value;
        _isUpdating = false;
    }

    // When EndTime changes, update Duration to maintain StartTime
    partial void OnEndTimeChanged(TimeSpan value)
    {
        if (_isUpdating) return;
        
        _isUpdating = true;
        Duration = value - _startTime;
        _isUpdating = false;
    }
}