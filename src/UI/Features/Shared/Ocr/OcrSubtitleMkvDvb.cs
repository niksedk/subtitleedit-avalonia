using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public class OcrSubtitleMkvDvb : IOcrSubtitle
{
    public int Count { get; private set; }
    private readonly MatroskaTrackInfo _matroskaSubtitleInfo;
    private Subtitle _subtitle;
    private List<DvbSubPes> _subtitleImages;

    public OcrSubtitleMkvDvb(MatroskaTrackInfo matroskaSubtitleInfo, Subtitle subtitle, List<DvbSubPes> subtitleImages)
    {
        _matroskaSubtitleInfo = matroskaSubtitleInfo;
        _subtitle = subtitle;
        _subtitleImages = subtitleImages;
        Count = subtitleImages.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        return _subtitleImages[index].GetImageFull();
    }

    public TimeSpan GetStartTime(int index)
    {
        return _subtitle.Paragraphs[index].StartTime.TimeSpan;
    }

    public TimeSpan GetEndTime(int index)
    {
        return _subtitle.Paragraphs[index].EndTime.TimeSpan;
    }

    public List<OcrSubtitleItem> MakeOcrSubtitleItems()
    {
        var ocrSubtitleItems = new List<OcrSubtitleItem>(Count);
        for (var i = 0; i < Count; i++)
        {
            ocrSubtitleItems.Add(new OcrSubtitleItem(this, i));
        }

        return ocrSubtitleItems;
    }
}