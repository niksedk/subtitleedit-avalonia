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
    [ObservableProperty] private string _text;

    public int Number { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public bool HasFormattedText => FixResult != null;

    private readonly IOcrSubtitle _ocrSubtitle;
    private readonly int _index;
    private SKBitmap? _bitmap;

    private PreProcessingSettings? _preProcessingSettings;
    public PreProcessingSettings? PreProcessingSettings
    {
        get => _preProcessingSettings;
        set
        {
            _preProcessingSettings = value;
            _bitmap = null;
        }
    }

    public SKBitmap GetSkBitmapClean()
    {
        var bitmap = _ocrSubtitle.GetBitmap(_index);
        if (bitmap == null)
        {
            bitmap = new SKBitmap(1, 1);
        }

        return bitmap;
    }

    public SKBitmap GetSkBitmap()
    {
        if (_bitmap == null)
        {
            _bitmap = _ocrSubtitle.GetBitmap(_index);
            if (_bitmap != null && PreProcessingSettings != null)
            {
                _bitmap = PreProcessingSettings.PreProcess(_bitmap);
            }

            if (_bitmap == null)
            {
                _bitmap = new SKBitmap(1, 1);
            }
        }

        return _bitmap;
    }

    public Bitmap GetBitmap()
    {
        return GetSkBitmap().ToAvaloniaBitmap();
    }

    public SKPointI GetPosition()
    {
        return _ocrSubtitle.GetPosition(_index);
    }

    public SKSizeI GetScreenSize()
    {
        return _ocrSubtitle.GetScreenSize(_index);
    }

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

    public TextBlock CreateFormattedText()
    {
        return FixResult?.GetFormattedText() ?? new TextBlock { Text = Text };
    }

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