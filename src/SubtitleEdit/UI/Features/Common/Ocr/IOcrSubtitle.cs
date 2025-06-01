using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Common.Ocr;

public interface IOcrSubtitle
{
    int Count { get; }
    SKBitmap GetBitmap(int index);
    TimeSpan GetStartTime(int index);
    TimeSpan GetEndTime(int index);
    List<OcrSubtitleItem> MakeOcrSubtitleItems();
}