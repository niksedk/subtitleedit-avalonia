using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

public class AudioVisualizer : Control
{
    public static readonly StyledProperty<WavePeakData?> WavePeaksProperty =
        AvaloniaProperty.Register<AudioVisualizer, WavePeakData?>(nameof(WavePeaks));

    public static readonly StyledProperty<double> StartPositionSecondsProperty =
        AvaloniaProperty.Register<AudioVisualizer, double>(nameof(StartPositionSeconds));

    public static readonly StyledProperty<double> ZoomFactorProperty =
        AvaloniaProperty.Register<AudioVisualizer, double>(nameof(ZoomFactor), 1.0);

    public static readonly StyledProperty<double> VerticalZoomFactorProperty =
        AvaloniaProperty.Register<AudioVisualizer, double>(nameof(VerticalZoomFactor), 1.0);

    public static readonly StyledProperty<double> CurrentVideoPositionSecondsProperty =
        AvaloniaProperty.Register<AudioVisualizer, double>(nameof(CurrentVideoPositionSeconds));

    public static readonly StyledProperty<bool> DrawGridLinesProperty =
        AvaloniaProperty.Register<AudioVisualizer, bool>(nameof(DrawGridLines));

    public static readonly StyledProperty<bool> InvertMouseWheelProperty =
        AvaloniaProperty.Register<AudioVisualizer, bool>(nameof(InvertMouseWheel));

    public static readonly StyledProperty<List<SubtitleLineViewModel>> AllSelectedParagraphsProperty =
        AvaloniaProperty.Register<AudioVisualizer, List<SubtitleLineViewModel>>(nameof(AllSelectedParagraphs));

    public static readonly StyledProperty<Color> WaveformColorProperty =
        AvaloniaProperty.Register<AudioVisualizer, Color>(nameof(WaveformColor));

    public static readonly StyledProperty<Color> WaveformSelectedColorProperty =
        AvaloniaProperty.Register<AudioVisualizer, Color>(nameof(WaveformSelectedColor));

    public WavePeakData? WavePeaks
    {
        get => GetValue(WavePeaksProperty);
        set => SetValue(WavePeaksProperty, value);
    }

    public double StartPositionSeconds
    {
        get => GetValue(StartPositionSecondsProperty);
        set => SetValue(StartPositionSecondsProperty, value);
    }

    public double ZoomFactor
    {
        get => GetValue(ZoomFactorProperty);
        set => SetValue(ZoomFactorProperty, value);
    }

    public double VerticalZoomFactor
    {
        get => GetValue(VerticalZoomFactorProperty);
        set => SetValue(VerticalZoomFactorProperty, value);
    }

    public double CurrentVideoPositionSeconds
    {
        get => GetValue(CurrentVideoPositionSecondsProperty);
        set => SetValue(CurrentVideoPositionSecondsProperty, value);
    }

    public bool InvertMouseWheel
    {
        get => GetValue(InvertMouseWheelProperty);
        set => SetValue(InvertMouseWheelProperty, value);
    }

    public bool DrawGridLines
    {
        get => GetValue(DrawGridLinesProperty);
        set => SetValue(DrawGridLinesProperty, value);
    }

    public List<SubtitleLineViewModel> AllSelectedParagraphs
    {
        get => GetValue(AllSelectedParagraphsProperty);
        set => SetValue(AllSelectedParagraphsProperty, value);
    }

    public Color WaveformColor
    {
        get => GetValue(WaveformColorProperty);
        set
        {
            _paintWaveform = new Pen(new SolidColorBrush(value), 1);
            SetValue(WaveformColorProperty, value);
        }
    }

    public Color WaveformSelectedColor
    {
        get => GetValue(WaveformSelectedColorProperty);
        set
        {
            _paintPenSelected = new Pen(new SolidColorBrush(value), 1);
            SetValue(WaveformSelectedColorProperty, value);
        }
    }

    public SubtitleLineViewModel? SelectedParagraph { get; set; }

    // Pens and brushes
    private Pen _paintWaveform = new Pen(new SolidColorBrush(Color.FromArgb(150, 144, 238, 144)), 1);
    private Pen _paintPenSelected = new Pen(new SolidColorBrush(Color.FromArgb(210, 254, 10, 10)), 1);
    private readonly Pen _paintGridLines = new Pen(Brushes.DarkGray, 0.2);
    private readonly Pen _paintCurrentPosition = new Pen(Brushes.Cyan, 1);
    private readonly IBrush _mouseOverBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 0));

    // Paragraph painting
    private readonly IBrush _paintBackground = new SolidColorBrush(Color.FromArgb(90, 70, 70, 70));
    private readonly Pen _paintLeft = new Pen(new SolidColorBrush(Color.FromArgb(60, 0, 255, 0)), 2);
    private readonly Pen _paintRight = new Pen(new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)), 2);
    private readonly IBrush _paintText = Brushes.White;
    private readonly Typeface _typeface = new Typeface("Arial");
    private readonly double _fontSize = 12;

    private readonly List<SubtitleLineViewModel> _displayableParagraphs = new();
    private bool _isCtrlDown;
    private bool _isAltDown;
    private bool _isShiftDown;
    private long _lastMouseWheelScroll = -1;
    private readonly Lock _lock = new();
    public SubtitleLineViewModel? NewSelectionParagraph { get; set; }
    public double _newSelectionSeconds { get; set; }
    private SubtitleLineViewModel? _activeParagraph;
    private SubtitleLineViewModel? _activeParagraphPrevoius;
    private SubtitleLineViewModel? _activeParagraphNext;
    private Point _startPointerPosition;
    private double _originalStartSeconds;
    private double _originalEndSeconds;
    private long _audioVisualizerLastScroll;
    private long _lastPointerPressed = -1;
    private enum InteractionMode { None, Moving, ResizingLeft, ResizingLeftOr, ResizingRight, ResizingRightOr, New }
    private InteractionMode _interactionMode = InteractionMode.None;

    public readonly double ResizeMargin = 5.0; // Margin for resizing paragraphs
    public bool IsScrolling => _audioVisualizerLastScroll > 0;

    public class PositionEventArgs : EventArgs
    {
        public double PositionInSeconds { get; set; }
    }
    public delegate void PositionEventHandler(object sender, PositionEventArgs e);
    public delegate void ParagraphEventHandler(object sender, ParagraphEventArgs e);
    public event PositionEventHandler? OnVideoPositionChanged;
    public event ParagraphEventHandler? OnToggleSelection;
    public event PositionEventHandler? OnHorizontalScroll;
    public event ParagraphEventHandler? OnParagraphDoubleTapped;
    //public event ParagraphEventHandler? OnPositionSelected;
    //public event ParagraphEventHandler? OnTimeChanged;
    //public event ParagraphEventHandler? OnStartTimeChanged;
    //public event ParagraphEventHandler? OnTimeChangedAndOffsetRest;
    //public event ParagraphEventHandler? OnNewSelectionRightClicked;
    public event ParagraphEventHandler? OnNewSelectionInsert;
    //public event ParagraphEventHandler? OnParagraphRightClicked;
    //public event ParagraphEventHandler? OnNonParagraphRightClicked;
    //public event ParagraphEventHandler? OnSingleClick;
    //public event ParagraphEventHandler? OnStatus;

    public AudioVisualizer()
    {
        AllSelectedParagraphs = new List<SubtitleLineViewModel>();
        Focusable = true;
        IsHitTestVisible = true;
        ClipToBounds = true;

        AffectsRender<AudioVisualizer>(
            WavePeaksProperty,
            StartPositionSecondsProperty,
            ZoomFactorProperty,
            VerticalZoomFactorProperty,
            CurrentVideoPositionSecondsProperty,
            AllSelectedParagraphsProperty);

        PointerMoved += OnPointerMoved;
        PointerEntered += OnPointerEntered;
        PointerExited += OnPointerExited;
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
        PointerWheelChanged += OnPointerWheelChanged;
        DoubleTapped += (sender, e) =>
        {
            if (OnParagraphDoubleTapped != null)
            {
                var point = e.GetPosition(this);
                var p = HitTestParagraph(point);
                if (p != null)
                {
                    var position = RelativeXPositionToSeconds((int)e.GetPosition(this).X);
                    OnParagraphDoubleTapped.Invoke(this, new ParagraphEventArgs(position, p));
                }
            }
        };
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
        LostFocus += (sender, e) =>
        {          
            InvalidateVisual();
        };
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
        {
            _isAltDown = false;
            e.Handled = true;
        }
        else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isCtrlDown = false;
            e.Handled = true;
        }
        else if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
        {
            _isShiftDown = false;
            e.Handled = true;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _interactionMode = InteractionMode.None;
            NewSelectionParagraph = null;
            InvalidateVisual();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            var newP = NewSelectionParagraph;
            if (newP != null && OnNewSelectionInsert != null)
            {
                OnNewSelectionInsert.Invoke(this, new ParagraphEventArgs(newP));
            }

            _interactionMode = InteractionMode.None;
            NewSelectionParagraph = null;
            InvalidateVisual();
            e.Handled = true;
        }
        else if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
        {
            _isAltDown = true;
        }
        else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isCtrlDown = true;
        }
        else if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
        {
            _isShiftDown = true;
        }
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        _lastMouseWheelScroll = DateTime.UtcNow.Ticks;

        var point = e.GetPosition(this);
        var properties = e.GetCurrentPoint(this).Properties;
        var delta = InvertMouseWheel ? -e.Delta.Y : e.Delta.Y;

        if (_isAltDown)
        {
            var newZoomFactor = ZoomFactor + delta / 1000.0;

            if (newZoomFactor < 0.1)
            {
                newZoomFactor = 0.1;
            }

            if (newZoomFactor > 20.0)
            {
                newZoomFactor = 20.0;
            }

            ZoomFactor = newZoomFactor;

            InvalidateVisual();
            return;
        }

        if (_isShiftDown)
        {
            var newZoomFactor = VerticalZoomFactor + delta / 100.0;

            if (newZoomFactor < 0.1)
            {
                newZoomFactor = 0.1;
            }

            if (newZoomFactor > 20.0)
            {
                newZoomFactor = 20.0;
            }

            VerticalZoomFactor = newZoomFactor;

            InvalidateVisual();
            return;
        }

        e.Handled = true;
        var newStart = StartPositionSeconds + delta / 2;
        if (newStart < 0)
        {
            newStart = 0;
        }

        _audioVisualizerLastScroll = Stopwatch.GetTimestamp(); // Update the last scroll time
        StartPositionSeconds = newStart;
        if (OnHorizontalScroll != null)
        {
            OnHorizontalScroll.Invoke(this, new PositionEventArgs { PositionInSeconds = newStart });
        }
        InvalidateVisual();
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var nsp = NewSelectionParagraph;
        if (nsp != null)
        {
            nsp.UpdateDuration();
            if (nsp.Duration.TotalMilliseconds < 10)
            {
                NewSelectionParagraph = null;
            }
            else
            {
                _interactionMode = InteractionMode.None;
                _activeParagraph = null;
                return;
            }
        }

        if (_interactionMode == InteractionMode.None || _interactionMode == InteractionMode.New)
        {
            if (OnVideoPositionChanged != null)
            {
                var videoPosition = RelativeXPositionToSeconds((int)e.GetPosition(this).X);
                _audioVisualizerLastScroll = 0;
                OnVideoPositionChanged.Invoke(this, new PositionEventArgs { PositionInSeconds = videoPosition });
            }
            _activeParagraph = null;
            return;
        }

        if (_interactionMode == InteractionMode.Moving)
        { // click on paragraph, but with no move
            var ts = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - _lastPointerPressed);
            if (ts.TotalMilliseconds < 100 ||
                (
                _activeParagraph != null &&
                Math.Abs(_originalStartSeconds - _activeParagraph.StartTime.TotalSeconds) < 0.01))
            {
                if (_isCtrlDown && _activeParagraph != null && OnToggleSelection != null)
                {
                    var videoPosition = RelativeXPositionToSeconds((int)e.GetPosition(this).X);
                    _audioVisualizerLastScroll = 0;
                    OnToggleSelection.Invoke(this, new ParagraphEventArgs(videoPosition, _activeParagraph));
                }
                else if (OnVideoPositionChanged != null)
                {
                    var videoPosition = RelativeXPositionToSeconds((int)e.GetPosition(this).X);
                    _audioVisualizerLastScroll = 0;
                    OnVideoPositionChanged.Invoke(this, new PositionEventArgs { PositionInSeconds = videoPosition });
                }
                _activeParagraph = null;
                return;
            }
        }

        _interactionMode = InteractionMode.None;
        _activeParagraph = null;

        InvalidateVisual();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _lastPointerPressed = Stopwatch.GetTimestamp();
        e.Handled = true;
        var point = e.GetPosition(this);
        var p = HitTestParagraph(point);
        if (p == null)
        {
            _interactionMode = InteractionMode.New;
            NewSelectionParagraph = new SubtitleLineViewModel();

            var deltaX = point.X; // - _startPointerPosition.X;
            var deltaSeconds = RelativeXPositionToSeconds((int)deltaX);
            _newSelectionSeconds = deltaSeconds;

            NewSelectionParagraph.StartTime = TimeSpan.FromSeconds(deltaSeconds);
            NewSelectionParagraph.EndTime = TimeSpan.FromSeconds(deltaSeconds);
            InvalidateVisual();
            return;
        }

        double left = SecondsToXPosition(p.StartTime.TotalSeconds - StartPositionSeconds);
        double right = SecondsToXPosition(p.EndTime.TotalSeconds - StartPositionSeconds);

        _activeParagraph = p;
        _startPointerPosition = point;
        _originalStartSeconds = p.StartTime.TotalSeconds;
        _originalEndSeconds = p.EndTime.TotalSeconds;

        var displayableParagraphs = _displayableParagraphs;
        if (displayableParagraphs == null || displayableParagraphs.Count == 0)
        {
            return;
        }

        if (Math.Abs(point.X - left) <= ResizeMargin)
        {
            _interactionMode = InteractionMode.ResizingLeft;

            var idx = displayableParagraphs.IndexOf(p);
            var p2 = HitTestParagraph(point, displayableParagraphs, idx - 1);
            if (p2 != null)
            {
                _activeParagraphPrevoius = p2;
                _interactionMode = InteractionMode.ResizingLeftOr;
            }
        }
        else if (Math.Abs(point.X - right) <= ResizeMargin)
        {
            _interactionMode = InteractionMode.ResizingRight;

            var idx = displayableParagraphs.IndexOf(p);
            var p2 = HitTestParagraph(point, displayableParagraphs, idx + 1);
            if (p2 != null)
            {
                _activeParagraphNext = p2;
                _interactionMode = InteractionMode.ResizingRightOr;
            }
        }
        else if (point.X > left && point.X < right)
        {
            _interactionMode = InteractionMode.Moving;
        }
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        base.OnPointerExited(e);
        NewSelectionParagraph = null;
        InvalidateVisual();
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (!IsFocused)
        {
            Focus();
        }

        InvalidateVisual();
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var point = e.GetPosition(this);
        var properties = e.GetCurrentPoint(this).Properties;
        var newP = NewSelectionParagraph;
        if (_interactionMode == InteractionMode.New && newP != null && properties.IsLeftButtonPressed)
        {
            var seconds = RelativeXPositionToSeconds((int)point.X);
            if (seconds > _newSelectionSeconds)
            {
                newP.StartTime = TimeSpan.FromSeconds(_newSelectionSeconds);
                newP.EndTime = TimeSpan.FromSeconds(seconds);
            }
            else
            {
                newP.StartTime = TimeSpan.FromSeconds(seconds);
                newP.EndTime = TimeSpan.FromSeconds(_newSelectionSeconds);
            }

            InvalidateVisual();
            return;
        }

        if (_interactionMode == InteractionMode.None || _activeParagraph == null)
        {
            UpdateCursor(point);
            return;
        }

        var deltaX = point.X - _startPointerPosition.X;
        var deltaSeconds = RelativeXPositionToSeconds((int)deltaX);

        if (_interactionMode == InteractionMode.ResizingLeftOr && _activeParagraphPrevoius != null)
        {
            if (Math.Abs(deltaX) < 3)
            {
                return;
            }

            if (_startPointerPosition.X < point.X)
            {
                _interactionMode = InteractionMode.ResizingLeft;
            }
            else
            {
                _activeParagraph = _activeParagraphPrevoius;
                _originalStartSeconds = _activeParagraph.StartTime.TotalSeconds;
                _originalEndSeconds = _activeParagraph.EndTime.TotalSeconds;
                _interactionMode = InteractionMode.ResizingRight;
            }
        }
        else if (_interactionMode == InteractionMode.ResizingRightOr && _activeParagraphNext != null)
        {
            if (Math.Abs(deltaX) < 3)
            {
                return;
            }

            if (_startPointerPosition.X > point.X)
            {
                _interactionMode = InteractionMode.ResizingRight;
            }
            else
            {
                _activeParagraph = _activeParagraphNext;
                _originalStartSeconds = _activeParagraph.StartTime.TotalSeconds;
                _originalEndSeconds = _activeParagraph.EndTime.TotalSeconds;
                _interactionMode = InteractionMode.ResizingLeft;
            }
        }

        var newStart = _originalStartSeconds;
        var newEnd = _originalEndSeconds;

        var currentIndex = _displayableParagraphs.IndexOf(_activeParagraph);
        var previous = currentIndex > 0 ? _displayableParagraphs[currentIndex - 1] : null;
        var next = currentIndex < _displayableParagraphs.Count - 1 ? _displayableParagraphs[currentIndex + 1] : null;


        switch (_interactionMode)
        {
            case InteractionMode.Moving:
                newStart = _originalStartSeconds + deltaSeconds - StartPositionSeconds;
                newEnd = _originalEndSeconds + deltaSeconds - StartPositionSeconds;

                // Clamp so it doesn't overlap previous or next
                if (previous != null && newStart < previous.EndTime.TotalSeconds + 0.001)
                {
                    newStart = previous.EndTime.TotalSeconds + 0.001;
                    newEnd = newStart + (_originalEndSeconds - _originalStartSeconds);
                }

                if (next != null && newEnd > next.StartTime.TotalSeconds - 0.001)
                {
                    newEnd = next.StartTime.TotalSeconds - 0.001;
                    newStart = newEnd - (_originalEndSeconds - _originalStartSeconds);
                }

                _activeParagraph.StartTime = TimeSpan.FromSeconds(newStart);
                _activeParagraph.EndTime = TimeSpan.FromSeconds(newEnd);
                break;

            case InteractionMode.ResizingLeft:
                newStart = _originalStartSeconds + deltaSeconds - StartPositionSeconds;

                if (previous != null && newStart < previous.EndTime.TotalSeconds)
                {
                    newStart = previous.EndTime.TotalSeconds + 0.001;
                }

                if (newStart < _activeParagraph.EndTime.TotalSeconds - 0.1)
                {
                    _activeParagraph.SetStartTimeOnly(TimeSpan.FromSeconds(newStart));
                }
                break;

            case InteractionMode.ResizingRight:
                newEnd = _originalEndSeconds + deltaSeconds - StartPositionSeconds;

                if (next != null && newEnd > next.StartTime.TotalSeconds)
                {
                    newEnd = next.StartTime.TotalSeconds - 0.001;
                }

                if (newEnd > _activeParagraph.StartTime.TotalSeconds + 0.1)
                {
                    _activeParagraph.EndTime = TimeSpan.FromSeconds(newEnd);
                }
                break;
        }

        InvalidateVisual();
    }


    private void UpdateCursor(Point point)
    {
        var p = HitTestParagraph(point);
        if (p != null)
        {
            double left = SecondsToXPosition(p.StartTime.TotalSeconds - StartPositionSeconds);
            double right = SecondsToXPosition(p.EndTime.TotalSeconds - StartPositionSeconds);

            if (Math.Abs(point.X - left) <= ResizeMargin || Math.Abs(point.X - right) <= ResizeMargin)
            {
                Cursor = new Cursor(StandardCursorType.SizeWestEast);
            }
            else
            {
                Cursor = new Cursor(StandardCursorType.Hand);
            }
        }
        else
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
        }
    }

    private SubtitleLineViewModel? HitTestParagraph(Point point)
    {
        if (_displayableParagraphs == null)
        {
            return null;
        }

        foreach (var p in _displayableParagraphs)
        {
            double left = SecondsToXPosition(p.StartTime.TotalSeconds - StartPositionSeconds);
            double right = SecondsToXPosition(p.EndTime.TotalSeconds - StartPositionSeconds);

            if (point.X >= left - ResizeMargin && point.X <= right + ResizeMargin)
            {
                return p;
            }
        }

        var newSelection = NewSelectionParagraph;
        if (newSelection != null)
        {
            double left = SecondsToXPosition(newSelection.StartTime.TotalSeconds - StartPositionSeconds);
            double right = SecondsToXPosition(newSelection.EndTime.TotalSeconds - StartPositionSeconds);

            if (point.X >= left - ResizeMargin && point.X <= right + ResizeMargin)
            {
                return newSelection;
            }
        }

        return null;
    }

    private SubtitleLineViewModel? HitTestParagraph(Point point, List<SubtitleLineViewModel> subtitles, int index)
    {
        if (subtitles == null || index < 0 || index > subtitles.Count - 1)
        {
            return null;
        }

        var p = subtitles[index];

        double left = SecondsToXPosition(p.StartTime.TotalSeconds - StartPositionSeconds);
        double right = SecondsToXPosition(p.EndTime.TotalSeconds - StartPositionSeconds);

        if (point.X >= left - ResizeMargin && point.X <= right + ResizeMargin)
        {
            return p;
        }

        //var newSelection = NewSelectionParagraph;
        //if (newSelection != null)
        //{
        //    double left = SecondsToXPosition(newSelection.StartTime.TotalSeconds - StartPositionSeconds);
        //    double right = SecondsToXPosition(newSelection.EndTime.TotalSeconds - StartPositionSeconds);

        //    if (point.X >= left - ResizeMargin && point.X <= right + ResizeMargin)
        //    {
        //        return newSelection;
        //    }
        //}

        return null;
    }

    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(Brushes.Transparent, null, new Rect(Bounds.Size));
        using (context.PushClip(new Rect(0, 0, Bounds.Width, Bounds.Height)))
        {
            DrawAllGridLines(context);
            DrawWaveForm(context);
            DrawTimeLine(context);
            DrawParagraphs(context);
            DrawCurrentVideoPosition(context);
            DrawNewParagraph(context);

            if (IsFocused)
            {
                context.DrawRectangle(null, _paintPenSelected, new Rect(0, 0, Bounds.Width, Bounds.Height));
            }
        }
    }

    private void DrawTimeLine(DrawingContext context)
    {
        if (WavePeaks == null || Bounds.Height < 1)
        {
            return;
        }

        var seconds = Math.Ceiling(StartPositionSeconds) - StartPositionSeconds;
        var position = SecondsToXPosition(seconds);
        var imageHeight = Bounds.Height;

        // Create pen and brush for drawing (you'll need to define these as class fields)
        // Assuming you have similar fields like the other drawing methods
        var pen = _paintTimeLine; // You'll need to define this Pen
        var textBrush = _paintTimeText; // You'll need to define this Brush

        while (position < Bounds.Width)
        {
            var n = ZoomFactor * WavePeaks.SampleRate;

            if (n > 38 || (int)Math.Round(StartPositionSeconds + seconds) % 5 == 0)
            {
                // Draw major tick line
                context.DrawLine(pen, new Point(position, imageHeight), new Point(position, imageHeight - 10));

                // Draw time text - try different positions and larger font
                var timeText = GetDisplayTime(StartPositionSeconds + seconds);
                var formattedText = new FormattedText(
                    timeText,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    Typeface.Default, // Use default typeface instead of custom one
                    12, // Increased font size for better visibility
                    textBrush);

                // Try different Y positions - adjust these based on your control height
                var textY = Math.Max(0, imageHeight - formattedText.Height - 2); // Ensure text is within bounds
                context.DrawText(formattedText, new Point(position + 2, textY));
            }

            seconds += 0.5;
            position = SecondsToXPosition(seconds);

            if (n > 64)
            {
                // Draw minor tick line
                context.DrawLine(pen, new Point(position, imageHeight), new Point(position, imageHeight - 5));
            }

            seconds += 0.5;
            position = SecondsToXPosition(seconds);
        }
    }

    // You'll also need to add these fields to your class if they don't exist:
    private readonly Pen _paintTimeLine = new Pen(Brushes.Gray, 1);
    private readonly IBrush _paintTimeText = Brushes.WhiteSmoke;

    private string GetDisplayTime(double seconds)
    {
        var secs = (int)Math.Round(seconds, MidpointRounding.AwayFromZero);
        if (secs < 60)
        {
            return secs.ToString(CultureInfo.InvariantCulture);
        }

        var timeSpan = TimeSpan.FromSeconds(seconds);
        if (timeSpan.TotalHours >= 1)
        {
            return timeSpan.ToString(@"h\:mm\:ss");
        }

        return timeSpan.ToString(@"m\:ss");
    }

    public double EndPositionSeconds
    {
        get
        {
            if (WavePeaks == null)
            {
                return 0;
            }

            return RelativeXPositionToSeconds((int)Bounds.Width);
        }
    }

    private void DrawAllGridLines(DrawingContext context)
    {
        if (!DrawGridLines)
        {
            return;
        }

        var width = Bounds.Width;
        var height = Bounds.Height;

        if (WavePeaks == null)
        {
            for (var i = 0; i < width; i += 10)
            {
                context.DrawLine(_paintGridLines, new Point(i, 0), new Point(i, height));
                context.DrawLine(_paintGridLines, new Point(0, i), new Point(width, i));
            }
        }
        else
        {
            var seconds = Math.Ceiling(StartPositionSeconds) - StartPositionSeconds - 1;
            var xPosition = SecondsToXPosition(seconds);
            var yPosition = 0;
            var yCounter = 0d;
            var interval = ZoomFactor >= 0.4d ?
                0.1d : // a pixel is 0.1 second
                1.0d;  // a pixel is 1.0 second

            while (xPosition < width)
            {
                context.DrawLine(_paintGridLines, new Point(xPosition, 0), new Point(xPosition, height));
                seconds += interval;
                xPosition = SecondsToXPosition(seconds);
            }

            while (yPosition < height)
            {
                context.DrawLine(_paintGridLines, new Point(0, yPosition), new Point(width, yPosition));
                yCounter += interval;
                yPosition = Convert.ToInt32(yCounter * WavePeaks.SampleRate * ZoomFactor);
            }
        }
    }

    private void DrawWaveForm(DrawingContext context)
    {
        if (WavePeaks?.Peaks == null || WavePeaks.Peaks.Count == 0)
        {
            return;
        }

        var showWaveform = true;
        if (!showWaveform)
        {
            return;
        }

        var waveformHeight = Bounds.Height;
        var isSelectedHelper = new IsSelectedHelper(AllSelectedParagraphs, WavePeaks.SampleRate);
        var halfWaveformHeight = waveformHeight / 2;
        var div = WavePeaks.SampleRate * ZoomFactor;

        if (div <= 0)
        {
            return;
        }

        for (var x = 0; x < Bounds.Width; x++)
        {
            var pos = (StartPositionSeconds + x / div) * WavePeaks.SampleRate;
            var pos0 = (int)pos;
            var pos1 = pos0 + 1;

            if (pos1 >= WavePeaks.Peaks.Count || pos0 > WavePeaks.Peaks.Count)
            {
                break;
            }

            var pos1Weight = pos - pos0;
            var pos0Weight = 1.0 - pos1Weight;
            var peak0 = WavePeaks.Peaks[pos0];
            var peak1 = WavePeaks.Peaks[pos1];
            var max = peak0.Max * pos0Weight + peak1.Max * pos1Weight;
            var min = peak0.Min * pos0Weight + peak1.Min * pos1Weight;

            var yMax = CalculateY(max, 0, halfWaveformHeight);
            var yMin = CalculateY(min, 0, halfWaveformHeight);

            // Ensure yMin is below yMax and both are within bounds
            if (yMin < yMax)
            {
                (yMin, yMax) = (yMax, yMin);
            }

            // Make sure there's at least a 1 pixel line
            if (Math.Abs(yMax - yMin) < 1)
            {
                yMin = yMax + 1;
            }

            var pen = isSelectedHelper.IsSelected(pos0) ? _paintPenSelected : _paintWaveform;
            context.DrawLine(pen, new Point(x, yMax), new Point(x, yMin));
        }
    }

    private void DrawParagraphs(DrawingContext context)
    {
        if (_displayableParagraphs == null)
        {
            return;
        }

        var startPositionMilliseconds = StartPositionSeconds * 1000.0;
        var endPositionMilliseconds = RelativeXPositionToSeconds((int)Bounds.Width) * 1000.0;
        var paragraphStartList = new List<int>();
        var paragraphEndList = new List<int>();
        var paragraphs = _displayableParagraphs;

        foreach (var p in paragraphs)
        {
            if (p.EndTime.TotalMilliseconds >= startPositionMilliseconds && p.StartTime.TotalMilliseconds <= endPositionMilliseconds)
            {
                paragraphStartList.Add(SecondsToXPosition(p.StartTime.TotalSeconds - StartPositionSeconds));
                paragraphEndList.Add(SecondsToXPosition(p.EndTime.TotalSeconds - StartPositionSeconds));
                DrawParagraph(p, context);
            }
        }
    }

    private void DrawParagraph(SubtitleLineViewModel paragraph, DrawingContext context)
    {
        var currentRegionLeft = SecondsToXPosition(paragraph.StartTime.TotalSeconds - StartPositionSeconds);
        var currentRegionRight = SecondsToXPosition(paragraph.EndTime.TotalSeconds - StartPositionSeconds);
        var currentRegionWidth = currentRegionRight - currentRegionLeft;

        if (currentRegionWidth <= 5 || Bounds.Height < 1)
        {
            return;
        }

        var height = Bounds.Height;

        // Draw background rectangle
        context.FillRectangle(_paintBackground, new Rect(currentRegionLeft, 0, currentRegionWidth, height));

        // Draw left and right borders
        context.DrawLine(_paintLeft, new Point(currentRegionLeft, 0), new Point(currentRegionLeft, height));
        context.DrawLine(_paintRight, new Point(currentRegionRight - 1, 0), new Point(currentRegionRight - 1, height));

        // Draw clipped text
        var text = HtmlUtil.RemoveHtmlTags(paragraph.Text, true);
        if (text.Length > 200)
        {
            text = text.Substring(0, 100).TrimEnd() + "...";
        }

        var textBounds = new Rect(currentRegionLeft + 1, 0, currentRegionWidth - 3, height);

        using (context.PushClip(textBounds))
        {
            var arr = text.SplitToLines();
            if (Configuration.Settings.VideoControls.WaveformUnwrapText)
            {
                text = string.Join("  ", arr);
                var formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    _typeface, _fontSize, _paintText);
                context.DrawText(formattedText, new Point(currentRegionLeft + 3, 14));
            }
            else
            {
                double addY = 0;
                foreach (var line in arr)
                {
                    var formattedText = new FormattedText(line, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        _typeface, _fontSize, _paintText);
                    context.DrawText(formattedText, new Point(currentRegionLeft + 3, 14 + addY));
                    addY += formattedText.Height;
                }
            }
        }
    }

    private void DrawCurrentVideoPosition(DrawingContext context)
    {
        if (CurrentVideoPositionSeconds <= 0)
        {
            return;
        }

        var currentPositionPos = SecondsToXPosition(CurrentVideoPositionSeconds - StartPositionSeconds);
        if (currentPositionPos > 0 && currentPositionPos < Bounds.Width)
        {
            context.DrawLine(_paintCurrentPosition,
                new Point(currentPositionPos, 0),
                new Point(currentPositionPos, Bounds.Height));
        }
    }

    private void DrawNewParagraph(DrawingContext context)
    {
        if (NewSelectionParagraph == null)
        {
            return;
        }

        double currentRegionLeft = SecondsToXPosition(NewSelectionParagraph.StartTime.TotalSeconds - StartPositionSeconds);
        double currentRegionRight = SecondsToXPosition(NewSelectionParagraph.EndTime.TotalSeconds - StartPositionSeconds);
        var currentRegionWidth = currentRegionRight - currentRegionLeft;

        if (currentRegionRight >= 0 && currentRegionLeft <= Bounds.Width)
        {
            var rect = new Rect(currentRegionLeft, 0, currentRegionWidth, Bounds.Height);
            context.FillRectangle(_paintBackground, rect);
        }
    }

    private double CalculateY(double value, double baseHeight, double halfWaveformHeight)
    {
        if (WavePeaks == null)
        {
            return halfWaveformHeight;
        }

        // Normalize the value to the control's height
        var normalizedValue = value / WavePeaks.HighestPeak / VerticalZoomFactor;
        var yOffset = normalizedValue * halfWaveformHeight;

        // Ensure Y stays within bounds
        var y = halfWaveformHeight - yOffset;
        return Math.Max(0, Math.Min(Bounds.Height, y));
    }

    private double RelativeXPositionToSeconds(int x)
    {
        if (WavePeaks == null)
        {
            return 0;
        }
        return StartPositionSeconds + (double)x / WavePeaks.SampleRate / ZoomFactor;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private int SecondsToXPosition(double seconds)
    {
        if (WavePeaks == null)
        {
            return 0;
        }

        return (int)Math.Round(seconds * WavePeaks.SampleRate * ZoomFactor, MidpointRounding.AwayFromZero);
    }

    public void SetPosition(double startPositionSeconds, ObservableCollection<SubtitleLineViewModel> subtitle, double currentVideoPositionSeconds, int subtitleIndex, List<SubtitleLineViewModel> selectedIndexes)
    {
        if (TimeSpan.FromTicks(DateTime.UtcNow.Ticks - _lastMouseWheelScroll).TotalSeconds > 0.25)
        { // don't set start position when scrolling with mouse wheel as it will make a bad (jumping back) forward scrolling
            StartPositionSeconds = startPositionSeconds;
        }
        CurrentVideoPositionSeconds = currentVideoPositionSeconds;
        LoadParagraphs(subtitle, subtitleIndex, selectedIndexes);
    }

    private void LoadParagraphs(ObservableCollection<SubtitleLineViewModel> subtitle, int primarySelectedIndex, List<SubtitleLineViewModel> selectedIndexes)
    {
        lock (_lock)
        {
            _displayableParagraphs.Clear();
            SelectedParagraph = null;
            AllSelectedParagraphs.Clear();

            if (WavePeaks == null)
            {
                return;
            }

            const double additionalSeconds = 15.0; // Helps when scrolling
            var startThresholdMilliseconds = (StartPositionSeconds - additionalSeconds) * TimeCode.BaseUnit;
            var endThresholdMilliseconds = (EndPositionSeconds + additionalSeconds) * TimeCode.BaseUnit;
            var displayableParagraphs = new List<SubtitleLineViewModel>();
            for (var i = 0; i < subtitle.Count; i++)
            {
                var p = subtitle[i];

                if (p.StartTime.TotalMilliseconds >= TimeCode.MaxTimeTotalMilliseconds)
                {
                    continue;
                }

                //_subtitle.Paragraphs.Add(p);
                if (p.EndTime.TotalMilliseconds >= startThresholdMilliseconds && p.StartTime.TotalMilliseconds <= endThresholdMilliseconds)
                {
                    displayableParagraphs.Add(p);
                    if (displayableParagraphs.Count > 99)
                    {
                        break;
                    }
                }
            }

            displayableParagraphs = displayableParagraphs.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
            var lastStartTime = -1d;
            foreach (var p in displayableParagraphs)
            {
                if (displayableParagraphs.Count > 30 &&
                    (p.Duration.TotalMilliseconds < 0.01 || p.StartTime.TotalMilliseconds - lastStartTime < 90))
                {
                    continue;
                }

                _displayableParagraphs.Add(p);
                lastStartTime = p.StartTime.TotalMilliseconds;
            }

            var primaryParagraph = subtitle.GetOrNull(primarySelectedIndex);
            if (primaryParagraph != null && !primaryParagraph.StartTime.IsMaxTime())
            {
                SelectedParagraph = primaryParagraph;
                AllSelectedParagraphs.Add(primaryParagraph);
            }

            foreach (var index in selectedIndexes)
            {
                var p = subtitle.FirstOrDefault(p => p == index);
                if (p != null && !p.StartTime.IsMaxTime())
                {
                    AllSelectedParagraphs.Add(p);
                }
            }
        }
    }
}