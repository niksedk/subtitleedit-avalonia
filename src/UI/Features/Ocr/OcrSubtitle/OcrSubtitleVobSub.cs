using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleVobSub : IOcrSubtitle
{
    public int Count { get; private set; }

    private readonly List<VobSubMergedPack> _vobSubMergedPack;
    private List<SKColor>? _palette;

    public OcrSubtitleVobSub(List<VobSubMergedPack> vobSubMergedPack, List<SKColor>? palette = null)
    {
        _vobSubMergedPack = vobSubMergedPack;
        _palette = palette;
        Count = _vobSubMergedPack.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        if (_palette != null)
        {
            _vobSubMergedPack[index].Palette = _palette;
        }

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
        Count = _vobSubMergedPack.Count;
    }
}