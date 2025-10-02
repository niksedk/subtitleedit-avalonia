using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleMkvBluRay : IOcrSubtitle
{
    public int Count { get; private set; }
    private readonly MatroskaTrackInfo _matroskaSubtitleInfo;
    private readonly List<BluRaySupParser.PcsData> _pcsDataList;

    public OcrSubtitleMkvBluRay(MatroskaTrackInfo matroskaSubtitleInfo, List<BluRaySupParser.PcsData> pcsDataList)
    {
        _matroskaSubtitleInfo = matroskaSubtitleInfo;
        _pcsDataList = pcsDataList;
        Count = _pcsDataList.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        return _pcsDataList[index].GetBitmap();
    }

    public TimeSpan GetStartTime(int index)
    {
        return TimeSpan.FromMilliseconds(_pcsDataList[index].StartTimeCode.TotalMilliseconds);
    }

    public TimeSpan GetEndTime(int index)
    {
        return TimeSpan.FromMilliseconds(_pcsDataList[index].EndTimeCode.TotalMilliseconds);
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
        _pcsDataList.RemoveAt(index);
        Count = _pcsDataList.Count;
    }

    public SKPointI GetPosition(int index)
    {
        return new SKPointI(-1, -1);
    }

    public SKSizeI GetScreenSize(int index)
    {
        return new SKSizeI(-1, -1);
    }
}