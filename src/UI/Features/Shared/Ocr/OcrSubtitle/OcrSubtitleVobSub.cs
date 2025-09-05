using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr.OcrSubtitle;

public class OcrSubtitleVobSub : IOcrSubtitle
{
    public int Count { get; private set; }

    private readonly List<VobSubMergedPack> _vobSubMergedPack;

    public OcrSubtitleVobSub(List<VobSubMergedPack> vobSubMergedPack)
    {
        _vobSubMergedPack = vobSubMergedPack;
        Count = vobSubMergedPack.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        return _vobSubMergedPack[index].GetBitmap();
    }

    public TimeSpan GetStartTime(int index)
    {
        return _vobSubMergedPack[index].StartTime;
    }

    public TimeSpan GetEndTime(int index)
    {
        return _vobSubMergedPack[index].EndTime;
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

    public void Delete(int index)
    {
        _vobSubMergedPack.RemoveAt(index);
    }
}