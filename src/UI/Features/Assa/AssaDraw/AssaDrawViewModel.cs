using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

public partial class AssaDrawViewModel : ObservableObject
{
    public Window? Window { get; set; }
    public AssaDrawCanvas? Canvas { get; set; }

    public bool OkPressed { get; private set; }

    /// <summary>
    /// The generated ASSA drawing code result.
    /// </summary>
    public string AssaDrawingCode { get; private set; } = string.Empty;

    [ObservableProperty] private List<DrawShape> _shapes = [];
    [ObservableProperty] private DrawShape? _activeShape;
    [ObservableProperty] private DrawCoordinate? _activePoint;
    [ObservableProperty] private DrawingTool _currentTool = DrawingTool.Line;
    [ObservableProperty] private int _canvasWidth = 1920;
    [ObservableProperty] private int _canvasHeight = 1080;
    [ObservableProperty] private string _positionText = "Position: 0, 0";
    [ObservableProperty] private string _zoomText = "Zoom: 100%";
    [ObservableProperty] private float _pointX;
    [ObservableProperty] private float _pointY;
    [ObservableProperty] private bool _isPointSelected;
    [ObservableProperty] private bool _showGrid = true;
    [ObservableProperty] private ObservableCollection<ShapeTreeItem> _shapeTreeItems = [];

    private float _currentX = float.MinValue;
    private float _currentY = float.MinValue;
    private readonly Regex _regexStart = new(@"\{[^{]*\\p1[^}]*\}");
    private readonly Regex _regexEnd = new(@"\{[^{]*\\p0[^}]*\}");

    public AssaDrawViewModel()
    {
    }

    public void Initialize(string? existingDrawCode = null)
    {
        if (!string.IsNullOrEmpty(existingDrawCode))
        {
            ImportAssaDrawingFromText(existingDrawCode, 0, Colors.White, false);
        }

        RefreshTreeView();
    }

    public void SetCanvas(AssaDrawCanvas canvas)
    {
        Canvas = canvas;
        Canvas.Shapes = Shapes;
        Canvas.CanvasWidth = CanvasWidth;
        Canvas.CanvasHeight = CanvasHeight;
        Canvas.CurrentTool = CurrentTool;

        Canvas.CanvasClicked += OnCanvasClicked;
        Canvas.CanvasMouseMoved += OnCanvasMouseMoved;
        Canvas.PointDragged += OnPointDragged;
        Canvas.ZoomChanged += OnZoomChanged;
    }

    private void OnZoomChanged(object? sender, float zoomFactor)
    {
        ZoomText = $"Zoom: {zoomFactor * 100:0}%";
    }

    private void OnCanvasClicked(object? sender, CanvasClickEventArgs e)
    {
        if (!e.IsLeftButton)
        {
            return;
        }

        var x = e.X;
        var y = e.Y;

        if (DrawSettings.SnapToGrid)
        {
            x = MathF.Round(x / DrawSettings.GridSize) * DrawSettings.GridSize;
            y = MathF.Round(y / DrawSettings.GridSize) * DrawSettings.GridSize;
        }

        ActivePoint = null;
        IsPointSelected = false;

        // Continue drawing on existing shape
        if (ActiveShape != null && ActiveShape.Points.Count > 0 && !Shapes.Contains(ActiveShape))
        {
            AddPointToActiveShape(x, y);
            Canvas?.InvalidateVisual();
            return;
        }

        // Start new shape
        StartNewShape(x, y);
        Canvas?.InvalidateVisual();
    }

    private void AddPointToActiveShape(float x, float y)
    {
        if (ActiveShape == null)
        {
            return;
        }

        switch (CurrentTool)
        {
            case DrawingTool.Line:
                ActiveShape.AddPoint(DrawCoordinateType.Line, x, y, DrawSettings.PointColor);
                break;

            case DrawingTool.Bezier:
                // Add two control points and endpoint
                var lastPoint = ActiveShape.Points[^1];
                var oneThirdX = (x - lastPoint.X) / 3f;
                var oneThirdY = (y - lastPoint.Y) / 3f;

                ActiveShape.AddPoint(DrawCoordinateType.BezierCurveSupport1,
                    lastPoint.X + oneThirdX, lastPoint.Y + oneThirdY, DrawSettings.PointHelperColor);
                ActiveShape.AddPoint(DrawCoordinateType.BezierCurveSupport2,
                    lastPoint.X + oneThirdX * 2, lastPoint.Y + oneThirdY * 2, DrawSettings.PointHelperColor);
                ActiveShape.AddPoint(DrawCoordinateType.BezierCurve, x, y, DrawSettings.PointColor);
                break;

            case DrawingTool.Circle:
                // Complete the circle with second click
                if (ActiveShape.Points.Count == 1)
                {
                    var start = ActiveShape.Points[0];
                    var radius = Math.Max(Math.Abs(x - start.X), Math.Abs(y - start.Y));
                    if (radius > 1)
                    {
                        ActiveShape = CircleBezier.MakeCircle(start.X, start.Y, radius, ActiveShape.Layer, ActiveShape.ForeColor);
                        Shapes.Add(ActiveShape);
                        RefreshTreeView();
                        ActiveShape = null;
                        _currentX = float.MinValue;
                        _currentY = float.MinValue;
                        if (Canvas != null)
                        {
                            Canvas.Shapes = Shapes;
                            Canvas.ActiveShape = null;
                            Canvas.CurrentX = float.MinValue;
                            Canvas.CurrentY = float.MinValue;
                        }
                    }
                }
                break;

            case DrawingTool.Rectangle:
                // Complete the rectangle with second click
                if (ActiveShape.Points.Count == 1)
                {
                    var start = ActiveShape.Points[0];
                    ActiveShape = MakeRectangle(start.X, start.Y, x - start.X, y - start.Y, ActiveShape.Layer, ActiveShape.ForeColor);
                    Shapes.Add(ActiveShape);
                    RefreshTreeView();
                    ActiveShape = null;
                    _currentX = float.MinValue;
                    _currentY = float.MinValue;
                    if (Canvas != null)
                    {
                        Canvas.Shapes = Shapes;
                        Canvas.ActiveShape = null;
                        Canvas.CurrentX = float.MinValue;
                        Canvas.CurrentY = float.MinValue;
                    }
                }
                break;
        }
    }

    private void StartNewShape(float x, float y)
    {
        ActiveShape = new DrawShape();

        switch (CurrentTool)
        {
            case DrawingTool.Line:
                ActiveShape.AddPoint(DrawCoordinateType.Line, x, y, DrawSettings.PointColor);
                break;

            case DrawingTool.Bezier:
                ActiveShape.AddPoint(DrawCoordinateType.BezierCurve, x, y, DrawSettings.PointColor);
                break;

            case DrawingTool.Circle:
            case DrawingTool.Rectangle:
                ActiveShape.AddPoint(DrawCoordinateType.Line, x, y, DrawSettings.PointColor);
                break;
        }

        if (Canvas != null)
        {
            Canvas.ActiveShape = ActiveShape;
        }
    }

    private void OnCanvasMouseMoved(object? sender, CanvasMouseEventArgs e)
    {
        _currentX = e.X;
        _currentY = e.Y;
        PositionText = $"Position: {e.X:0}, {e.Y:0}";

        if (Canvas != null)
        {
            Canvas.CurrentX = _currentX;
            Canvas.CurrentY = _currentY;
            Canvas.InvalidateVisual();
        }
    }

    private void OnPointDragged(object? sender, DrawCoordinate point)
    {
        PointX = point.X;
        PointY = point.Y;
        RefreshTreeView();
    }

    [RelayCommand]
    private void SelectTool() => SetTool(DrawingTool.Select);

    [RelayCommand]
    private void LineTool() => SetTool(DrawingTool.Line);

    [RelayCommand]
    private void BezierTool() => SetTool(DrawingTool.Bezier);

    [RelayCommand]
    private void RectangleTool() => SetTool(DrawingTool.Rectangle);

    [RelayCommand]
    private void CircleTool() => SetTool(DrawingTool.Circle);

    private void SetTool(DrawingTool tool)
    {
        CurrentTool = tool;
        if (Canvas != null)
        {
            Canvas.CurrentTool = tool;
        }
    }

    [RelayCommand]
    private void CloseShape()
    {
        if (ActiveShape == null || ActiveShape.Points.Count < 3)
        {
            return;
        }

        // Handle circle/rectangle special cases
        if (CurrentTool == DrawingTool.Circle && _currentX > float.MinValue)
        {
            var start = ActiveShape.Points[0];
            var radius = Math.Max(Math.Abs(_currentX - start.X), Math.Abs(_currentY - start.Y));
            if (radius > 1)
            {
                ActiveShape = CircleBezier.MakeCircle(start.X, start.Y, radius, ActiveShape.Layer, ActiveShape.ForeColor);
            }
        }
        else if (CurrentTool == DrawingTool.Rectangle && _currentX > float.MinValue)
        {
            var start = ActiveShape.Points[0];
            ActiveShape = MakeRectangle(start.X, start.Y, _currentX - start.X, _currentY - start.Y,
                ActiveShape.Layer, ActiveShape.ForeColor);
        }

        if (!Shapes.Contains(ActiveShape))
        {
            Shapes.Add(ActiveShape);
        }

        RefreshTreeView();
        ActiveShape = null;
        _currentX = float.MinValue;
        _currentY = float.MinValue;

        if (Canvas != null)
        {
            Canvas.ActiveShape = null;
            Canvas.CurrentX = float.MinValue;
            Canvas.CurrentY = float.MinValue;
            Canvas.Shapes = Shapes;
            Canvas.InvalidateVisual();
        }
    }

    private static DrawShape MakeRectangle(float x, float y, float width, float height, int layer, Color color)
    {
        var shape = new DrawShape { ForeColor = color, Layer = layer };
        shape.AddPoint(DrawCoordinateType.Line, x, y, DrawSettings.PointColor);
        shape.AddPoint(DrawCoordinateType.Line, x + width, y, DrawSettings.PointColor);
        shape.AddPoint(DrawCoordinateType.Line, x + width, y + height, DrawSettings.PointColor);
        shape.AddPoint(DrawCoordinateType.Line, x, y + height, DrawSettings.PointColor);
        return shape;
    }

    [RelayCommand]
    private void DeleteShape()
    {
        if (ActiveShape != null)
        {
            Shapes.Remove(ActiveShape);
            ActiveShape = null;
            RefreshTreeView();
            Canvas?.InvalidateVisual();
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        Shapes.Clear();
        ActiveShape = null;
        ActivePoint = null;
        _currentX = float.MinValue;
        _currentY = float.MinValue;
        RefreshTreeView();

        if (Canvas != null)
        {
            Canvas.Shapes = Shapes;
            Canvas.ActiveShape = null;
            Canvas.ActivePoint = null;
            Canvas.InvalidateVisual();
        }
    }

    [RelayCommand]
    private void ZoomIn()
    {
        Canvas?.ZoomIn();
        UpdateZoomText();
    }

    [RelayCommand]
    private void ZoomOut()
    {
        Canvas?.ZoomOut();
        UpdateZoomText();
    }

    [RelayCommand]
    private void ResetView()
    {
        Canvas?.ResetView();
        UpdateZoomText();
    }

    private void UpdateZoomText()
    {
        if (Canvas != null)
        {
            ZoomText = $"Zoom: {Canvas.ZoomFactor * 100:0}%";
        }
    }

    [RelayCommand]
    private void ToggleGrid()
    {
        ShowGrid = !ShowGrid;
        DrawSettings.ShowGrid = ShowGrid;
        Canvas?.InvalidateVisual();
    }

    [RelayCommand]
    private async void CopyToClipboard()
    {
        var code = GenerateAssaCode();
        if (!string.IsNullOrEmpty(code) && Window?.Clipboard != null)
        {
            await Window.Clipboard.SetTextAsync(code);
        }
    }

    private string GenerateAssaCode()
    {
        if (Shapes.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var shape in Shapes.Where(s => !s.IsEraser && !s.Hidden))
        {
            sb.Append(shape.ToAssa());
            sb.Append(' ');
        }

        var drawCode = sb.ToString().Trim();
        if (string.IsNullOrEmpty(drawCode))
        {
            return string.Empty;
        }

        return $"{{\\p1}}{drawCode}{{\\p0}}";
    }

    [RelayCommand]
    private void Ok()
    {
        AssaDrawingCode = GenerateAssaCode();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    private void RefreshTreeView()
    {
        ShapeTreeItems.Clear();

        var layers = Shapes.GroupBy(s => s.Layer).OrderBy(g => g.Key);
        foreach (var layer in layers)
        {
            var layerItem = new ShapeTreeItem
            {
                Name = $"Layer {layer.Key}",
                IsLayer = true,
                Layer = layer.Key
            };

            foreach (var shape in layer)
            {
                var shapeItem = new ShapeTreeItem
                {
                    Name = $"Shape ({(shape.IsEraser ? "erase" : "draw")})",
                    Shape = shape
                };

                foreach (var point in shape.Points)
                {
                    shapeItem.Children.Add(new ShapeTreeItem
                    {
                        Name = point.GetText(point.X, point.Y),
                        Point = point
                    });
                }

                layerItem.Children.Add(shapeItem);
            }

            ShapeTreeItems.Add(layerItem);
        }
    }

    private void ImportAssaDrawingFromText(string text, int layer, Color color, bool isEraser)
    {
        text = _regexStart.Replace(text, string.Empty);
        text = _regexEnd.Replace(text, string.Empty);
        var arr = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        var i = 0;
        var bezierCount = 0;
        var state = DrawCoordinateType.None;
        DrawCoordinate? moveCoordinate = null;
        DrawShape? drawShape = null;

        while (i < arr.Length)
        {
            var v = arr[i];

            if (v == "m" && i < arr.Length - 2 &&
                float.TryParse(arr[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var mX) &&
                float.TryParse(arr[i + 2], NumberStyles.Float, CultureInfo.InvariantCulture, out var mY))
            {
                bezierCount = 0;
                moveCoordinate = new DrawCoordinate(null, DrawCoordinateType.Move, mX, mY, DrawSettings.PointColor);
                state = DrawCoordinateType.Move;
                i += 2;
            }
            else if (v == "l")
            {
                state = DrawCoordinateType.Line;
                bezierCount = 0;
                if (moveCoordinate != null)
                {
                    drawShape = new DrawShape { Layer = layer, ForeColor = color, IsEraser = isEraser };
                    drawShape.AddPoint(DrawCoordinateType.Line, moveCoordinate.X, moveCoordinate.Y, DrawSettings.PointColor);
                    moveCoordinate = null;
                    Shapes.Add(drawShape);
                }
            }
            else if (v == "b")
            {
                state = DrawCoordinateType.BezierCurve;
                if (moveCoordinate != null)
                {
                    drawShape = new DrawShape { Layer = layer, ForeColor = color, IsEraser = isEraser };
                    drawShape.AddPoint(DrawCoordinateType.BezierCurve, moveCoordinate.X, moveCoordinate.Y, DrawSettings.PointColor);
                    moveCoordinate = null;
                    Shapes.Add(drawShape);
                }
                bezierCount = 1;
            }
            else if (state == DrawCoordinateType.Line && drawShape != null && i < arr.Length - 1 &&
                float.TryParse(arr[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var lX) &&
                float.TryParse(arr[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var lY))
            {
                drawShape.AddPoint(DrawCoordinateType.Line, lX, lY, DrawSettings.PointColor);
                i++;
            }
            else if (state == DrawCoordinateType.BezierCurve && drawShape != null && i < arr.Length - 1 &&
                float.TryParse(arr[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var bX) &&
                float.TryParse(arr[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var bY))
            {
                bezierCount++;
                if (bezierCount > 3)
                {
                    bezierCount = 1;
                }

                var pointType = bezierCount switch
                {
                    2 => DrawCoordinateType.BezierCurveSupport1,
                    3 => DrawCoordinateType.BezierCurveSupport2,
                    _ => DrawCoordinateType.BezierCurve
                };

                var pointColor = bezierCount is 2 or 3 ? DrawSettings.PointHelperColor : DrawSettings.PointColor;
                drawShape.AddPoint(pointType, bX, bY, pointColor);
                i++;
            }

            i++;
        }
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (ActiveShape != null && !Shapes.Contains(ActiveShape))
            {
                // Cancel current drawing
                ActiveShape = null;
                _currentX = float.MinValue;
                _currentY = float.MinValue;
                if (Canvas != null)
                {
                    Canvas.ActiveShape = null;
                    Canvas.CurrentX = float.MinValue;
                    Canvas.CurrentY = float.MinValue;
                    Canvas.InvalidateVisual();
                }
                e.Handled = true;
                return;
            }

            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Enter)
        {
            CloseShape();
            e.Handled = true;
        }
        else if (e.Key == Key.Delete && ActiveShape != null)
        {
            DeleteShape();
            e.Handled = true;
        }
        else if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            switch (e.Key)
            {
                case Key.D0:
                case Key.NumPad0:
                    ResetView();
                    e.Handled = true;
                    break;
                case Key.OemPlus:
                case Key.Add:
                    ZoomIn();
                    e.Handled = true;
                    break;
                case Key.OemMinus:
                case Key.Subtract:
                    ZoomOut();
                    e.Handled = true;
                    break;
                case Key.C:
                    CopyToClipboard();
                    e.Handled = true;
                    break;
                case Key.N:
                    ClearAll();
                    e.Handled = true;
                    break;
            }
        }
        else if (e.Key == Key.F4)
        {
            LineTool();
            e.Handled = true;
        }
        else if (e.Key == Key.F5)
        {
            BezierTool();
            e.Handled = true;
        }
        else if (e.Key == Key.F6)
        {
            RectangleTool();
            e.Handled = true;
        }
        else if (e.Key == Key.F7)
        {
            CircleTool();
            e.Handled = true;
        }
        else if (e.Key == Key.F8)
        {
            CloseShape();
            e.Handled = true;
        }
        else if (e.Key == Key.G && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            ToggleGrid();
            e.Handled = true;
        }
    }

    partial void OnPointXChanged(float value)
    {
        if (ActivePoint != null)
        {
            ActivePoint.X = value;
            RefreshTreeView();
            Canvas?.InvalidateVisual();
        }
    }

    partial void OnPointYChanged(float value)
    {
        if (ActivePoint != null)
        {
            ActivePoint.Y = value;
            RefreshTreeView();
            Canvas?.InvalidateVisual();
        }
    }
}

/// <summary>
/// Represents an item in the shape tree view.
/// </summary>
public partial class ShapeTreeItem : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private ObservableCollection<ShapeTreeItem> _children = [];

    public bool IsLayer { get; set; }
    public int Layer { get; set; }
    public DrawShape? Shape { get; set; }
    public DrawCoordinate? Point { get; set; }

    public override string ToString() => Name;
}