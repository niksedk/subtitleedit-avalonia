using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Collections.Generic;

public class NOcrDrawingCanvasView : Control
{
    public List<NOcrLine> HitPaths { get; set; }
    public List<NOcrLine> MissPaths { get; set; }
    public bool IsControlDown { get; set; }
    public bool IsDrawing { get; private set; }
    public bool NewLinesAreHits { get; set; } = true;
    public IBrush CanvasColor { get; set; } = new SolidColorBrush(Colors.DarkGray);
    public IBrush HitColor { get; set; } = new SolidColorBrush(Colors.Green);
    public IBrush MissColor { get; set; } = new SolidColorBrush(Colors.Red);
    public Bitmap? BackgroundImage { get; set; }

    public float ZoomFactor
    {
        get => _zoomFactor;
        set
        {
            _zoomFactor = value;
            if (BackgroundImage != null)
            {
                Width = BackgroundImage.PixelSize.Width * _zoomFactor;
                Height = BackgroundImage.PixelSize.Height * _zoomFactor;
            }
            InvalidateVisual();
        }
    }

    private NOcrLine _currentPath;
    private int _mouseMoveStartX = -1;
    private int _mouseMoveStartY = -1;
    private float _zoomFactor = 1.0f;
    private float _strokeWidth = 3.0f;
    private bool _abort;    

    public NOcrDrawingCanvasView()
    {
        HitPaths = new List<NOcrLine>();
        MissPaths = new List<NOcrLine>();
        _currentPath = new NOcrLine();

        ClipToBounds = true;

        // Set initial size
        Width = 100;
        Height = 100;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        _abort = false;
        IsDrawing = true;
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

        if (IsDrawing)
        {
            _currentPath = new NOcrLine(new OcrPoint(_mouseMoveStartX, _mouseMoveStartY), new OcrPoint(x, y));
            InvalidateVisual();
        }

        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_abort)
        {
            _abort = false;
            e.Pointer.Capture(null);
            e.Handled = true;
            return;
        }

        IsDrawing = false;
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

        if (IsControlDown)
        {
            IsDrawing = true;
            var pos = e.GetPosition(this);

            _mouseMoveStartX = (int)Math.Round(pos.X / ZoomFactor, MidpointRounding.AwayFromZero);
            _mouseMoveStartY = (int)Math.Round(pos.Y / ZoomFactor, MidpointRounding.AwayFromZero);
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Fill background
        context.FillRectangle(CanvasColor, new Rect(0, 0, Bounds.Width, Bounds.Height));

        // Draw background image if available
        if (BackgroundImage != null)
        {
            var imageRect = new Rect(
                0, 0,
                BackgroundImage.PixelSize.Width * ZoomFactor,
                BackgroundImage.PixelSize.Height * ZoomFactor
            );
            context.DrawImage(BackgroundImage, imageRect);
        }

        // Create pen for drawing lines
        var missPen = new Pen(MissColor, _strokeWidth);
        var hitPen = new Pen(HitColor, _strokeWidth);

        // Draw miss paths
        foreach (var path in MissPaths)
        {
            DrawLine(context, path, missPen);
        }

        // Draw hit paths
        foreach (var path in HitPaths)
        {
            DrawLine(context, path, hitPen);
        }

        // Draw the current path if drawing
        if (IsDrawing && !_currentPath.IsEmpty)
        {
            var currentPen = NewLinesAreHits ? hitPen : missPen;
            DrawLine(context, _currentPath, currentPen);
        }
    }

    private void DrawLine(DrawingContext context, NOcrLine line, IPen pen)
    {
        var startPoint = new Point(line.Start.X * ZoomFactor, line.Start.Y * ZoomFactor);
        var endPoint = new Point(line.End.X * ZoomFactor, line.End.Y * ZoomFactor);

        context.DrawLine(pen, startPoint, endPoint);
    }

    public void SetStrokeWidth(float width)
    {
        _strokeWidth = width;
        InvalidateVisual();
    }

    public void ClearPaths()
    {
        HitPaths.Clear();
        MissPaths.Clear();
        InvalidateVisual();
    }

    public void SetBackgroundImage(Bitmap? bitmap)
    {
        BackgroundImage = bitmap;
        if (bitmap != null)
        {
            Width = bitmap.PixelSize.Width * ZoomFactor;
            Height = bitmap.PixelSize.Height * ZoomFactor;
        }
        InvalidateVisual();
    }

    internal void AbortDraw()
    {
        _abort = true;
        _currentPath = new NOcrLine();
        _mouseMoveStartX = -1;
        _mouseMoveStartY = -1;
        IsDrawing = false;
        InvalidateVisual();
    }
}