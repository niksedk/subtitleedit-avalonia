using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// Represents a single shape in an ASSA drawing consisting of multiple coordinates.
/// </summary>
public class DrawShape
{
    public List<DrawCoordinate> Points { get; set; } = [];
    public Color ForeColor { get; set; } = Colors.White;
    public Color OutlineColor { get; set; } = Colors.Black;
    public int OutlineWidth { get; set; }
    public int Layer { get; set; }
    public bool IsEraser { get; set; }
    public bool Hidden { get; set; }
    public bool Expanded { get; set; } = true;

    public DrawShape()
    {
    }

    public DrawShape(DrawShape other)
    {
        ForeColor = other.ForeColor;
        OutlineColor = other.OutlineColor;
        OutlineWidth = other.OutlineWidth;
        Layer = other.Layer;
        IsEraser = other.IsEraser;
        Hidden = other.Hidden;
        Expanded = other.Expanded;

        foreach (var point in other.Points)
        {
            var newPoint = point.Clone();
            newPoint.DrawShape = this;
            Points.Add(newPoint);
        }
    }

    public void AddPoint(DrawCoordinateType drawType, float x, float y, Color pointColor)
    {
        Points.Add(new DrawCoordinate(this, drawType, x, y, pointColor));
    }

    /// <summary>
    /// Converts the shape to ASSA drawing commands.
    /// </summary>
    public string ToAssa()
    {
        if (Points.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        var first = Points[0];

        // Start with move command
        sb.Append($"m {first.X:0.##} {first.Y:0.##} ");

        var i = 1;
        while (i < Points.Count)
        {
            var point = Points[i];

            if (point.DrawType == DrawCoordinateType.Line)
            {
                sb.Append($"l {point.X:0.##} {point.Y:0.##} ");
            }
            else if (point.DrawType == DrawCoordinateType.BezierCurveSupport1 && i + 2 < Points.Count)
            {
                var support2 = Points[i + 1];
                var endPoint = Points[i + 2];
                sb.Append($"b {point.X:0.##} {point.Y:0.##} {support2.X:0.##} {support2.Y:0.##} {endPoint.X:0.##} {endPoint.Y:0.##} ");
                i += 2;
            }
            else if (point.DrawType == DrawCoordinateType.BezierCurve)
            {
                sb.Append($"l {point.X:0.##} {point.Y:0.##} ");
            }

            i++;
        }

        return sb.ToString().Trim();
    }

    public DrawShape Clone()
    {
        return new DrawShape(this);
    }
}
