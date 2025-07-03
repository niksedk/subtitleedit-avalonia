using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public sealed class NOcrDrawingCanvasView : Control
{
    public List<NOcrLine> HitPaths { get; } = new();
    public List<NOcrLine> MissPaths { get; } = new();

    public double ZoomFactor
    {
        get => _zoomFactor;
        set
        {
            _zoomFactor = Math.Max(0.01, value);
            if (BackgroundImage is { } bmp)
            {
                Width = bmp.Size.Width * _zoomFactor;
                Height = bmp.Size.Height * _zoomFactor;
            }
            InvalidateVisual();
        }
    }

    public bool NewLinesAreHits { get; set; } = true;

    public Color CanvasColor { get; set; } = Colors.DarkGray;
    public Color HitColor { get; set; } = Colors.Green;
    public Color MissColor { get; set; } = Colors.Red;

    private Bitmap? _backgroundImage;
    public Bitmap? BackgroundImage 
    {
        get => _backgroundImage;
        set
        {
            _backgroundImage = value;
            InvalidateVisual();
        }
    }

    public void SetStrokeWidth(double width)
    {
        _strokeWidth = Math.Max(0.1, width);
        InvalidateVisual();
    }

    public NOcrDrawingCanvasView()
    {
        ClipToBounds = true;
        ZoomFactor = 1.0;
        _currentPath = new NOcrLine();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        _isDrawing = true;
        var pos = e.GetPosition(this);
        _mouseStartX = (int)Math.Round(pos.X / ZoomFactor);
        _mouseStartY = (int)Math.Round(pos.Y / ZoomFactor);

        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_isDrawing) return;

        var pos = e.GetPosition(this);
        var x = (int)Math.Round(pos.X / ZoomFactor);
        var y = (int)Math.Round(pos.Y / ZoomFactor);

        _currentPath = new NOcrLine(new OcrPoint(_mouseStartX, _mouseStartY),
                                    new OcrPoint(x, y));
        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isDrawing && !_currentPath.IsEmpty)
        {
            (NewLinesAreHits ? HitPaths : MissPaths).Add(_currentPath);
            _currentPath = new NOcrLine();
        }
        _isDrawing = false;

        e.Pointer.Capture(null);
        e.Handled = true;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);

        context.FillRectangle(new SolidColorBrush(CanvasColor), bounds);

        if (BackgroundImage is { } bmp)
        {
            var dest = new Rect(0, 0,
                                bmp.Size.Width * ZoomFactor,
                                bmp.Size.Height * ZoomFactor);
            context.DrawImage(bmp, sourceRect: default, destRect: dest);
        }

        DrawPathCollection(context, MissPaths, MissColor);
        DrawPathCollection(context, HitPaths, HitColor);

        if (_isDrawing && !_currentPath.IsEmpty)
            DrawSinglePath(context,
                           _currentPath,
                           NewLinesAreHits ? HitColor : MissColor);
    }

    private void DrawPathCollection(
        DrawingContext ctx,
        IEnumerable<NOcrLine> paths,
        Color colour)
    {
        var pen = GetPen(colour);

        foreach (var p in paths)
            DrawSinglePath(ctx, p, pen);
    }

    private void DrawSinglePath(
        DrawingContext ctx,
        NOcrLine p,
        Color colour) =>
        DrawSinglePath(ctx, p, GetPen(colour));

    private void DrawSinglePath(
        DrawingContext ctx,
        NOcrLine p,
        Pen pen)
    {
        ctx.DrawLine(pen,
            new Point(p.Start.X * ZoomFactor, p.Start.Y * ZoomFactor),
            new Point(p.End.X * ZoomFactor, p.End.Y * ZoomFactor));
    }

    private Pen GetPen(Color colour) =>
        new(new SolidColorBrush(colour), _strokeWidth, lineCap: PenLineCap.Round);

    private NOcrLine _currentPath;
    private bool _isDrawing;
    private int _mouseStartX;
    private int _mouseStartY;
    private double _strokeWidth = 3.0;
    private double _zoomFactor = 1.0;
}
