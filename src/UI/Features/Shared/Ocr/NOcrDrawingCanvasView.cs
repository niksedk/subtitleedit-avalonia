using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Nikse.SubtitleEdit.Logic.Ocr;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public class NOcrDrawingCanvasView : Control
{
    public List<NOcrLine> HitPaths { get; set; }
    public List<NOcrLine> MissPaths { get; set; }

    public float ZoomFactor
    {
        get => _zoomFactor;
        set
        {
            _zoomFactor = value;
            Width = BackgroundImage.Width * _zoomFactor;
            Height = BackgroundImage.Height * _zoomFactor;
            InvalidateVisual();
        }
    }

    private NOcrLine _currentPath;
    private bool _isDrawing = false;
    private int _mouseMoveStartX = -1;
    private int _mouseMoveStartY = -1;

    private readonly SKPaint _drawingPaint = new()
    {
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 3,
        IsAntialias = true
    };

    private float _zoomFactor = 1.0f;

    public bool NewLinesAreHits { get; set; } = true;

    public SKColor CanvasColor { get; set; } = SKColors.DarkGray;
    public SKColor HitColor { get; set; } = SKColors.Green;
    public SKColor MissColor { get; set; } = SKColors.Red;
    public SKBitmap BackgroundImage { get; set; }

    public NOcrDrawingCanvasView()
    {
        HitPaths = new List<NOcrLine>();
        MissPaths = new List<NOcrLine>();
        _currentPath = new NOcrLine();
        BackgroundImage = new SKBitmap(1, 1);
        ZoomFactor = 1;

        ClipToBounds = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        _isDrawing = true;
        var pos = e.GetPosition(this);

        _mouseMoveStartX = (int)Math.Round(pos.X / ZoomFactor, MidpointRounding.AwayFromZero);
        _mouseMoveStartY = (int)Math.Round(pos.Y / ZoomFactor, MidpointRounding.AwayFromZero);

        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var pos = e.GetPosition(this);
        var x = (int)Math.Round(pos.X / ZoomFactor, MidpointRounding.AwayFromZero);
        var y = (int)Math.Round(pos.Y / ZoomFactor, MidpointRounding.AwayFromZero);

        if (_isDrawing)
        {
            _currentPath = new NOcrLine(new OcrPoint(_mouseMoveStartX, _mouseMoveStartY), new OcrPoint(x, y));
            InvalidateVisual();
        }

        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        _isDrawing = false;
        if (!_currentPath.IsEmpty)
        {
            if (NewLinesAreHits)
            {
                HitPaths.Add(_currentPath);
            }
            else
            {
                MissPaths.Add(_currentPath);
            }
            _currentPath = new NOcrLine();
        }

        e.Pointer.Capture(null);
        e.Handled = true;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var customDrawOp = new SkiaDrawOperation(
            new Rect(0, 0, Bounds.Width, Bounds.Height),
            this);

        context.Custom(customDrawOp);
    }

    private class SkiaDrawOperation : ICustomDrawOperation
    {
        private readonly NOcrDrawingCanvasView _owner;

        public SkiaDrawOperation(Rect bounds, NOcrDrawingCanvasView owner)
        {
            Bounds = bounds;
            _owner = owner;
        }

        public Rect Bounds { get; }
        public bool HitTest(Point p) => false;
        public bool Equals(ICustomDrawOperation? other) => false;

        public void Dispose() { }

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature is null)
                return;

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            canvas.Clear(_owner.CanvasColor);
            canvas.DrawBitmap(_owner.BackgroundImage,
                new SKRect(0, 0, _owner.BackgroundImage.Width * _owner.ZoomFactor, _owner.BackgroundImage.Height * _owner.ZoomFactor));

            _owner._drawingPaint.Color = _owner.MissColor;
            foreach (var path in _owner.MissPaths)
            {
                _owner.DrawNOcrLine(path, canvas);
            }

            _owner._drawingPaint.Color = _owner.HitColor;
            foreach (var path in _owner.HitPaths)
            {
                _owner.DrawNOcrLine(path, canvas);
            }

            // Draw the current path if drawing
            _owner._drawingPaint.Color = _owner.NewLinesAreHits ? _owner.HitColor : _owner.MissColor;
            if (_owner._isDrawing && !_owner._currentPath.IsEmpty)
            {
                _owner.DrawNOcrLine(_owner._currentPath, canvas);
            }
        }
    }

    private void DrawNOcrLine(NOcrLine path, SKCanvas canvas)
    {
        var skPath = new SKPath();
        skPath.MoveTo(path.Start.X * ZoomFactor, path.Start.Y * ZoomFactor);
        skPath.LineTo(path.End.X * ZoomFactor, path.End.Y * ZoomFactor);
        canvas.DrawPath(skPath, _drawingPaint);
    }

    public void SetStrokeWidth(float width)
    {
        _drawingPaint.StrokeWidth = width;
    }
}