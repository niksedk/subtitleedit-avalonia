﻿using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public partial class OcrSubtitleItem : ObservableObject
{
    public SKBitmap GetSkBitmap()
    {
        if (_bitmap == null)
        {
            _bitmap = _ocrSubtitle.GetBitmap(_index);
        }

        return _bitmap;
    }

    public Bitmap GetBitmap()
    {
        return GetSkBitmap().ToAvaloniaBitmap();
    }
    
    public int Number { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }
    public TimeSpan Duration { get; set; }

    [ObservableProperty] private string _text;

    private readonly IOcrSubtitle _ocrSubtitle;
    private readonly int _index;
    private SKBitmap? _bitmap;

    public OcrSubtitleItem(IOcrSubtitle ocrSubtitle, int index)
    {
        _ocrSubtitle = ocrSubtitle;
        _index = index;

        Number = index + 1;
        StartTime = _ocrSubtitle.GetStartTime(index);
        EndTime = _ocrSubtitle.GetEndTime(index);
        Duration = EndTime - StartTime;
        Text = string.Empty;
        _bitmap = null;
    }
}