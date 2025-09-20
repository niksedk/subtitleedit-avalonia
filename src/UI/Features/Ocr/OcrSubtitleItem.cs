using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System;

namespace Nikse.SubtitleEdit.Features.Ocr;

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

    private OcrFixLineResult? _fixResult;
    public OcrFixLineResult? FixResult
    {
        get => _fixResult;
        set
        {
            if (SetProperty(ref _fixResult, value))
            {
                OnPropertyChanged(nameof(HasFormattedText));
            }
        }
    }

    public bool HasFormattedText => FixResult != null;

    public TextBlock CreateFormattedText()
    {
        return FixResult?.GetFormattedText() ?? new TextBlock { Text = Text };
    }

    [ObservableProperty] private string _text;

    private readonly IOcrSubtitle _ocrSubtitle;
    private readonly int _index;
    private SKBitmap? _bitmap;
    private OcrFixLineResult? fixResult;

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