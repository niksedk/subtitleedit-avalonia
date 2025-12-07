using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.Ocr;

public class NOcrLine
{
    public OcrPoint Start { get; set; }
    public OcrPoint End { get; set; }

    public string DisplayName => ToString();

    public NOcrLine()
    {
        Start = new OcrPoint();
        End = new OcrPoint();
    }

    public bool IsEmpty => Start.X == End.X && Start.Y == End.Y;

    public NOcrLine(OcrPoint start, OcrPoint end)
    {
        Start = new OcrPoint(start.X, start.Y);
        End = new OcrPoint(end.X, end.Y);
    }

    public static OcrPointF PointPixelsToPercent(OcrPoint p, int pixelWidth, int pixelHeight)
    {
        return new OcrPointF((float)(p.X * 100.0 / pixelWidth), (float)(p.Y * 100.0 / pixelHeight));
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0},{1} -> {2},{3} ", Start.X, Start.Y, End.X, End.Y);
    }

    public OcrPointF GetStartPercent(int width, int height)
    {
        return PointPixelsToPercent(Start, width, height);
    }

    public OcrPointF GetEnd(int width, int height)
    {
        return PointPixelsToPercent(End, width, height);
    }

    public List<OcrPoint> GetPoints()
    {
        return GetPoints(Start, End);
    }

    public List<OcrPoint> ScaledGetPoints(NOcrChar nOcrChar, int width, int height)
    {
        return GetPoints(GetScaledStart(nOcrChar, width, height), GetScaledEnd(nOcrChar, width, height));
    }

    public static List<OcrPoint> GetPoints(OcrPoint start, OcrPoint end)
    {
        List<OcrPoint> list;
        var x1 = start.X;
        var x2 = end.X;
        var y1 = start.Y;
        var y2 = end.Y;
        if (Math.Abs(start.X - end.X) > Math.Abs(start.Y - end.Y))
        {
            if (x1 > x2)
            {
                x2 = start.X;
                x1 = end.X;
                y2 = start.Y;
                y1 = end.Y;
            }
            var factor = (double)(y2 - y1) / (x2 - x1);
            list = new List<OcrPoint>(x2 - x1 + 1);
            for (var i = x1; i <= x2; i++)
            {
                list.Add(new OcrPoint(i, (int)Math.Round(y1 + factor * (i - x1), MidpointRounding.AwayFromZero)));
            }
        }
        else
        {
            if (y1 > y2)
            {
                x2 = start.X;
                x1 = end.X;
                y2 = start.Y;
                y1 = end.Y;
            }
            var factor = (double)(x2 - x1) / (y2 - y1);
            list = new List<OcrPoint>(y2 - y1 + 1);
            for (var i = y1; i <= y2; i++)
            {
                list.Add(new OcrPoint((int)Math.Round(x1 + factor * (i - y1), MidpointRounding.AwayFromZero), i));
            }
        }

        return list;
    }

    internal OcrPoint GetScaledStart(NOcrChar ocrChar, int width, int height)
    {
        return new OcrPoint((int)Math.Round(Start.X * width / (double)ocrChar.Width, MidpointRounding.AwayFromZero), (int)Math.Round(Start.Y * height / (double)ocrChar.Height, MidpointRounding.AwayFromZero));
    }

    internal OcrPoint GetScaledEnd(NOcrChar ocrChar, int width, int height)
    {
        return new OcrPoint((int)Math.Round(End.X * width / (double)ocrChar.Width, MidpointRounding.AwayFromZero), (int)Math.Round(End.Y * height / (double)ocrChar.Height, MidpointRounding.AwayFromZero));
    }
}