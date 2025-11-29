using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

public class AudioVisualizer : Control
{
    public static readonly StyledProperty<WavePeakData2?> WavePeaksProperty =
        AvaloniaProperty.Register<AudioVisualizer, WavePeakData2?>(nameof(WavePeaks));

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

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<AudioVisualizer, bool>(nameof(IsReadOnly));

    public static readonly StyledProperty<bool> InvertMouseWheelProperty =
        AvaloniaProperty.Register<AudioVisualizer, bool>(nameof(InvertMouseWheel));

    public static readonly StyledProperty<List<SubtitleLineViewModel>> AllSelectedParagraphsProperty =
        AvaloniaProperty.Register<AudioVisualizer, List<SubtitleLineViewModel>>(nameof(AllSelectedParagraphs));

    public static readonly StyledProperty<Color> WaveformColorProperty =
        AvaloniaProperty.Register<AudioVisualizer, Color>(nameof(WaveformColor));

    public static readonly StyledProperty<Color> WaveformSelectedColorProperty =
        AvaloniaProperty.Register<AudioVisualizer, Color>(nameof(WaveformSelectedColor));

    public static readonly StyledProperty<Color> WaveformCursorColorProperty =
        AvaloniaProperty.Register<AudioVisualizer, Color>(nameof(WaveformCursorColor));

    public WavePeakData2? WavePeaks
    {
        get => GetValue(WavePeaksProperty);
        set => SetValue(WavePeaksProperty, value);
    }

    public double StartPositionSeconds
    {
        get => GetValue(StartPositionSecondsProperty);
        set
        {
            var clampedValue = Math.Max(0, Math.Min(value, MaxStartPositionSeconds));
            SetValue(StartPositionSecondsProperty, clampedValue);
        }
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

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
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

    public Color WaveformCursorColor
    {
        get => GetValue(WaveformCursorColorProperty);
        set
        {
            _paintPenCursor = new Pen(new SolidColorBrush(value), 1);
            SetValue(WaveformCursorColorProperty, value);
        }
    }

    public SubtitleLineViewModel? SelectedParagraph { get; set; }

    public double ShotChangeSnapSeconds { get; set; } = 0.05;

    public bool SnapToShotChanges { get; set; } = true;
    public bool FocusOnMouseOver { get; set; } = true;

    private List<double> _shotChanges = new List<double>();

    /// <summary>
    /// Shot changes (seconds)
    /// </summary>
    public List<double> ShotChanges
    {
        get => _shotChanges;
        set
        {
            _shotChanges = value;
        }
    }

    private double MaxStartPositionSeconds
    {
        get
        {
            if (WavePeaks == null || Bounds.Width <= 0 || ZoomFactor <= 0)
            {
                return int.MaxValue;
            }

            // Calculate how many seconds are visible in the current view
            var visibleSeconds = Bounds.Width / (ZoomFactor * WavePeaks.SampleRate);

            // Maximum start position is total length minus visible length
            var maxStart = WavePeaks.LengthInSeconds - visibleSeconds;

            // Don't allow negative values (when zoomed out beyond waveform length)
            return Math.Max(0, maxStart + 1); // +1 to allow some extra space at end
        }
    }

    // Pens and brushes
    private Pen _paintWaveform = new Pen(new SolidColorBrush(Color.FromArgb(150, 144, 238, 144)), 1);
    private Pen _paintPenSelected = new Pen(new SolidColorBrush(Color.FromArgb(210, 254, 10, 10)), 1);
    private Pen _paintPenCursor = new Pen(Brushes.Cyan, 1);
    private readonly Pen _paintGridLines = new Pen(Brushes.DarkGray, 0.2);
    private readonly IBrush _mouseOverBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 0));

    // Paragraph painting
    private readonly IBrush _paintBackground = new SolidColorBrush(Color.FromArgb(90, 70, 70, 70));
    private readonly Pen _paintLeft = new Pen(new SolidColorBrush(Color.FromArgb(60, 0, 255, 0)), 2);
    private readonly Pen _paintRight = new Pen(new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)), 2);
    private readonly IBrush _paintText = Brushes.White;
    private readonly Typeface _typeface = new Typeface(UiUtil.GetDefaultFontName());
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
    private SubtitleLineViewModel? _activeParagraphPrevious;
    private SubtitleLineViewModel? _activeParagraphNext;
    private Point _startPointerPosition;
    private double _originalStartSeconds;
    private double _originalEndSeconds;
    private double _originalPreviousEndSeconds;
    private double _originalNextStartSeconds;
    private long _audioVisualizerLastScroll;
    private long _lastPointerPressed = -1;
    private WaveformDisplayMode _displayMode = WaveformDisplayMode.OnlyWaveform;
    private SpectrogramData2? _spectrogram;

    private enum InteractionMode
    {
        None,
        Moving,
        ResizingLeft,
        ResizingLeftOr,
        ResizingRight,
        ResizingRightOr,
        ResizeLeftAnd,
        ResizeRightAnd,
        New,
    }
    private InteractionMode _interactionMode = InteractionMode.None;

    public readonly double ResizeMargin = 5.0; // Margin for resizing paragraphs
    public bool IsScrolling => _audioVisualizerLastScroll > 0;

    public class PositionEventArgs : EventArgs
    {
        public double PositionInSeconds { get; set; }
    }

    public class ContextEventArgs : EventArgs
    {
        public double PositionInSeconds { get; set; }
        public SubtitleLineViewModel? NewParagraph { get; set; }
    }

    public delegate void PositionEventHandler(object sender, PositionEventArgs e);
    public delegate void ContextEventHandler(object sender, ContextEventArgs e);
    public delegate void ParagraphEventHandler(object sender, ParagraphEventArgs e);
    public event PositionEventHandler? OnVideoPositionChanged;
    public event ContextEventHandler? FlyoutMenuOpening;
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
    public event ParagraphEventHandler? OnDeletePressed;

    public AudioVisualizer()
    {
        AllSelectedParagraphs = new List<SubtitleLineViewModel>();
        Focusable = true;
        IsHitTestVisible = true;
        ClipToBounds = true;
        MenuFlyout = new MenuFlyout();

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

    public void UpdateTheme()
    {
        _paintTimeText = UiUtil.GetTextColor();
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
        else if (e.Key == Key.Delete && !e.KeyModifiers.HasFlag(KeyModifiers.Control) && !e.KeyModifiers.HasFlag(KeyModifiers.Alt) && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            OnDeletePressed?.Invoke(this, new ParagraphEventArgs(0, _activeParagraph));
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
            var newZoomFactor = ZoomFactor + delta / 100.0;

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

        _audioVisualizerLastScroll = DateTime.UtcNow.Ticks; // Update the last scroll time
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
        if (nsp is { Duration.TotalMilliseconds: <= 1 })
        {
            nsp = null;
        }

        if (e.InitialPressMouseButton == MouseButton.Right)
        {
            _isAltDown = false;
            _isCtrlDown = false;
            _isShiftDown = false;
            _interactionMode = InteractionMode.None;
            nsp?.UpdateDuration();
            _audioVisualizerLastScroll = 0;
            e.Handled = true;
            var videoPosition = RelativeXPositionToSeconds((int)e.GetPosition(this).X);
            OnVideoPositionChanged?.Invoke(this, new PositionEventArgs { PositionInSeconds = videoPosition });
            FlyoutMenuOpening?.Invoke(this, new ContextEventArgs { PositionInSeconds = videoPosition, NewParagraph = nsp });
            InvalidateVisual();
            MenuFlyout.ShowAt(this, true);
            return;
        }


        if (nsp is { Duration.TotalMilliseconds: > 1 })
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

        if (_interactionMode is InteractionMode.None or InteractionMode.New)
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
            var ts = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - _lastPointerPressed);
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
        _lastPointerPressed = DateTime.UtcNow.Ticks;
        e.Handled = true;
        var point = e.GetPosition(this);
        _startPointerPosition = point;
        if (IsReadOnly)
        {
            InvalidateVisual();
            return;
        }

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
                _activeParagraphPrevious = p2;
                _interactionMode = InteractionMode.ResizingLeftOr;
            }

            if (_isAltDown)
            {
                p2 = HitTestParagraph(point, displayableParagraphs, idx - 1, 100);
                if (p2 != null)
                {
                    _activeParagraphPrevious = p2;
                    _originalPreviousEndSeconds = p2.EndTime.TotalSeconds;
                    _interactionMode = InteractionMode.ResizeLeftAnd; // move the prev end too
                }
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

            if (_isAltDown)
            {
                p2 = HitTestParagraphRight(point, displayableParagraphs, idx + 1, 100);
                if (p2 != null)
                {
                    _activeParagraphNext = p2;
                    _originalNextStartSeconds = p2.StartTime.TotalSeconds;
                    _interactionMode = InteractionMode.ResizeRightAnd; // move the prev end too
                }
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
        //NewSelectionParagraph = null;
        InvalidateVisual();
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (!FocusOnMouseOver)
        {
            return;
        }

        if (!IsFocused)
        {
            Focus();
        }

        InvalidateVisual();
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (IsReadOnly)
        {
            return;
        }

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

        if (_interactionMode == InteractionMode.ResizingLeftOr && _activeParagraphPrevious != null)
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
                _activeParagraph = _activeParagraphPrevious;
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
                var durationMs = _activeParagraph.Duration.TotalMilliseconds;

                // Clamp so it doesn't overlap previous or next
                if (previous != null && newStart < previous.EndTime.TotalSeconds + 0.001)
                {
                    newStart = previous.EndTime.TotalSeconds + 0.001;
                }

                if (next != null && newEnd > next.StartTime.TotalSeconds - 0.001)
                {
                    newStart = (next.StartTime.TotalSeconds - 0.001) - (_originalEndSeconds - _originalStartSeconds);
                }

                if (newStart < 0)
                {
                    newStart = 0;
                }

                _activeParagraph.StartTime = TimeSpan.FromSeconds(newStart);
                _activeParagraph.EndTime = TimeSpan.FromMilliseconds(newStart * 1000.0 + durationMs);
                break;
            case InteractionMode.ResizeLeftAnd:
                newStart = _originalStartSeconds + deltaSeconds - StartPositionSeconds;
                var newPrevEnd = _originalPreviousEndSeconds + deltaSeconds - StartPositionSeconds;
                if (_activeParagraphPrevious != null)
                {
                    _activeParagraph.SetStartTimeOnly(TimeSpan.FromSeconds(newStart));
                    _activeParagraphPrevious.EndTime = TimeSpan.FromSeconds(newPrevEnd);
                }
                break;
            case InteractionMode.ResizeRightAnd:
                newEnd = _originalEndSeconds + deltaSeconds - StartPositionSeconds;
                var newNextStart = _originalNextStartSeconds + deltaSeconds - StartPositionSeconds;
                if (_activeParagraphNext != null)
                {
                    _activeParagraph.EndTime = TimeSpan.FromSeconds(newEnd);
                    _activeParagraphNext.SetStartTimeOnly(TimeSpan.FromSeconds(newNextStart));
                }
                break;
            case InteractionMode.ResizingLeft:
                newStart = _originalStartSeconds + deltaSeconds - StartPositionSeconds;

                if (newStart < 0)
                {
                    newStart = 0;
                }

                if (SnapToShotChanges)
                {
                    var nearestShotChange = ShotChangesHelper.GetClosestShotChange(_shotChanges, TimeCode.FromSeconds(newStart));
                    if (nearestShotChange != null)
                    {
                        var nearest = (double)nearestShotChange;
                        if (nearest != newStart && Math.Abs(newStart - nearest) < ShotChangeSnapSeconds)
                        {
                            newStart = nearest;
                        }
                    }
                }

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

                if (SnapToShotChanges)
                {
                    var nearestShotChange = ShotChangesHelper.GetClosestShotChange(_shotChanges, TimeCode.FromSeconds(newEnd));
                    if (nearestShotChange != null)
                    {
                        var nearest = (double)nearestShotChange;
                        if (nearest != newEnd && Math.Abs(newEnd - nearest) < ShotChangeSnapSeconds)
                        {
                            var oneFrame = 42;// snap to frame before shot change - TODO: Correct? - TODO: get fps from video
                            newEnd = nearest - oneFrame;
                        }
                    }
                }

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
                if (p == NewSelectionParagraph && NewSelectionParagraph.Duration.TotalMilliseconds < 10)
                {
                    Cursor = new Cursor(StandardCursorType.Arrow);
                }
                else
                {
                    Cursor = new Cursor(StandardCursorType.SizeWestEast);
                }
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

    private SubtitleLineViewModel? HitTestParagraph(Point point, List<SubtitleLineViewModel> subtitles, int index, double resizeMargin)
    {
        if (subtitles == null || index < 0 || index > subtitles.Count - 1)
        {
            return null;
        }

        var p = subtitles[index];

        double left = SecondsToXPosition(p.StartTime.TotalSeconds - StartPositionSeconds);
        double right = SecondsToXPosition(p.EndTime.TotalSeconds - StartPositionSeconds);

        if (point.X >= left - ResizeMargin && point.X <= right + resizeMargin)
        {
            return p;
        }

        return null;
    }

    private SubtitleLineViewModel? HitTestParagraphRight(Point point, List<SubtitleLineViewModel> subtitles, int index, double resizeMargin)
    {
        if (index < 0 || index > subtitles.Count - 1)
        {
            return null;
        }

        var p = subtitles[index];

        double left = SecondsToXPosition(p.StartTime.TotalSeconds - StartPositionSeconds) - resizeMargin;
        double right = SecondsToXPosition(p.StartTime.TotalSeconds - StartPositionSeconds) + resizeMargin;

        if (point.X >= left && point.X <= right)
        {
            return p;
        }

        return null;
    }



    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(Brushes.Transparent, null, new Rect(Bounds.Size));
        using (context.PushClip(new Rect(0, 0, Bounds.Width, Bounds.Height)))
        {
            DrawAllGridLines(context);
            DrawWaveForm(context);
            DrawSpectrogram(context);
            DrawTimeLine(context);
            DrawParagraphs(context);
            DrawShotChanges(context);
            DrawCurrentVideoPosition(context);
            DrawNewParagraph(context);

            if (IsFocused)
            {
                context.DrawRectangle(null, _paintPenSelected, new Rect(0, 0, Bounds.Width, Bounds.Height));
            }
        }
    }

    private void DrawSpectrogram(DrawingContext context)
    {
        if (!HasSpectrogram() || _displayMode == WaveformDisplayMode.OnlyWaveform || _spectrogram == null)
        {
            return;
        }

        var height = Bounds.Height;
        if (_displayMode == WaveformDisplayMode.WaveformAndSpectrogram)
        {
            height = Bounds.Height / 2;
        }

        var width = (int)Math.Round((EndPositionSeconds - StartPositionSeconds) / _spectrogram.SampleDuration);
        if (width <= 0)
        {
            return;
        }

        // Create a combined bitmap using SkiaSharp
        using var skBitmapCombined = new SKBitmap(width, _spectrogram.FftSize / 2);
        using var skCanvas = new SKCanvas(skBitmapCombined);

        var left = (int)Math.Round(StartPositionSeconds / _spectrogram.SampleDuration);
        var offset = 0;
        var imageIndex = left / _spectrogram.ImageWidth;

        while (offset < width && imageIndex < _spectrogram.Images.Count)
        {
            var x = (left + offset) % _spectrogram.ImageWidth;
            var w = Math.Min(_spectrogram.ImageWidth - x, width - offset);

            // Draw part of the spectrogram image
            var sourceRect = new SKRect(x, 0, x + w, skBitmapCombined.Height);
            var destRect = new SKRect(offset, 0, offset + w, skBitmapCombined.Height);
            skCanvas.DrawBitmap(_spectrogram.Images[imageIndex], sourceRect, destRect);

            offset += w;
            imageIndex++;
        }

        // Convert SKBitmap to Avalonia Bitmap and draw it
        var displayHeight = height;
        var avaloniaBitmap = skBitmapCombined.ToAvaloniaBitmap();

        var destRectangle = new Rect(0, Bounds.Height - displayHeight, Bounds.Width, displayHeight);
        context.DrawImage(avaloniaBitmap, destRectangle);
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

        var pen = _paintTimeLine;
        var textBrush = _paintTimeText;

        while (position < Bounds.Width)
        {
            var n = ZoomFactor * WavePeaks.SampleRate;

            if (n > 38 || (int)Math.Round(StartPositionSeconds + seconds) % 5 == 0)
            {
                // Draw major tick lines (seconds)
                context.DrawLine(pen, new Point(position, imageHeight), new Point(position, imageHeight - 10));

                // Draw time text 
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

    private readonly Pen _paintTimeLine = new Pen(Brushes.Gray, 1);
    private IBrush _paintTimeText = UiUtil.GetTextColor();

    private static string GetDisplayTime(double seconds)
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

    public MenuFlyout MenuFlyout { get; set; }

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

        var waveformHeight = Bounds.Height;
        if (_displayMode == WaveformDisplayMode.WaveformAndSpectrogram)
        {
            waveformHeight = Bounds.Height / 2;
        }

        var isSelectedHelper = new IsSelectedHelper(AllSelectedParagraphs, WavePeaks.SampleRate);
        var halfWaveformHeight = waveformHeight / 2;
        var div = WavePeaks.SampleRate * ZoomFactor;
        if (div <= 0)
        {
            return;
        }

        var selectedColorRHigh = (byte)Math.Min(255, WaveformSelectedColor.R + 25);
        var selectedColorGHigh = (byte)Math.Min(255, WaveformSelectedColor.G + 25);
        var selectedColorBHigh = (byte)Math.Min(255, WaveformSelectedColor.B + 25);

        var colorRHigh = (byte)Math.Min(255, WaveformColor.R + 25);
        var colorGHigh = (byte)Math.Min(255, WaveformColor.G + 25);
        var colorBHigh = (byte)Math.Min(255, WaveformColor.B + 25);

        // Create gradient brushes for a more beautiful waveform
        var normalGradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new GradientStop(Color.FromArgb(180+30, WaveformColor.R, WaveformColor.G, WaveformColor.B), 0.0),    // Top: bright blue
                new GradientStop(Color.FromArgb(200+55, colorRHigh, colorGHigh, colorBHigh), 0.5),   // Middle: vibrant blue
                new GradientStop(Color.FromArgb(180+30, WaveformColor.R, WaveformColor.G, WaveformColor.B), 1.0)     // Bottom: bright blue
            }
        };

        var selectedGradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops = new GradientStops
        {
            new GradientStop(Color.FromArgb(200+30, WaveformSelectedColor.R, WaveformSelectedColor.G, WaveformSelectedColor.B), 0.0),    // Top: bright orange
            new GradientStop(Color.FromArgb(220+30, selectedColorRHigh, selectedColorGHigh, selectedColorBHigh), 0.5),   // Middle: vivid orange
            new GradientStop(Color.FromArgb(200+30, WaveformSelectedColor.R, WaveformSelectedColor.G, WaveformSelectedColor.B), 1.0)     // Bottom: bright orange
        }
        };

        // Create pens with the gradient brushes
        var normalPen = new Pen(normalGradient, 1.5);
        var selectedPen = new Pen(selectedGradient, 2.0);

        // Optional: Draw a subtle center line
        var centerLinePen = new Pen(new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)), 1);
        context.DrawLine(centerLinePen, new Point(0, halfWaveformHeight), new Point(Bounds.Width, halfWaveformHeight));

        // Draw waveform with smoothing
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

            var isSelected = isSelectedHelper.IsSelected(pos0);
            var pen = isSelected ? selectedPen : normalPen;

            // Draw the main waveform line
            context.DrawLine(pen, new Point(x, yMax), new Point(x, yMin));

            // Optional: Add a subtle glow effect for selected regions
            if (isSelected)
            {
                var glowPen = new Pen(new SolidColorBrush(Color.FromArgb(60, 255, 200, 100)), 4);
                context.DrawLine(glowPen, new Point(x, yMax), new Point(x, yMin));
            }
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

    private void DrawShotChanges(DrawingContext context)
    {
        var index = 0;
        var currentPositionPos = SecondsToXPosition(CurrentVideoPositionSeconds - StartPositionSeconds);

        var startPositionMilliseconds = StartPositionSeconds * 1000.0;
        var endPositionMilliseconds = RelativeXPositionToSeconds((int)Bounds.Width) * 1000.0; //TODO: Bounds.Width or Width ???
        var paragraphStartList = new List<int>();
        var paragraphEndList = new List<int>();
        foreach (var p in _displayableParagraphs)
        {
            if (p.EndTime.TotalMilliseconds >= startPositionMilliseconds && p.StartTime.TotalMilliseconds <= endPositionMilliseconds)
            {
                paragraphStartList.Add(SecondsToXPosition(p.StartTime.TotalSeconds - StartPositionSeconds));
                paragraphEndList.Add(SecondsToXPosition(p.EndTime.TotalSeconds - StartPositionSeconds));
            }
        }

        while (index < _shotChanges.Count)
        {
            int pos;
            try
            {
                var time = _shotChanges[index++];
                pos = SecondsToXPosition(time - StartPositionSeconds);
            }
            catch
            {
                pos = -1;
            }

            if (pos > 0 && pos < Bounds.Width)
            {
                if (currentPositionPos == pos)
                {
                    // shot change and current pos are the same
                    var pen1 = new Pen(Brushes.AntiqueWhite, 2);
                    context.DrawLine(pen1, new Point(pos, 0), new Point(pos, Bounds.Height));
                    context.DrawLine(_paintPenCursor, new Point(pos, 0), new Point(pos, Bounds.Height));
                }
                else if (paragraphStartList.Contains(pos))
                {
                    var pen1 = new Pen(Brushes.AntiqueWhite, 2);
                    context.DrawLine(pen1, new Point(pos, 0), new Point(pos, Bounds.Height));

                    var brush = new SolidColorBrush(Color.FromArgb(175, 0, 100, 0));
                    var pen2 = new Pen(brush, 2, dashStyle: DashStyle.Dash);
                    context.DrawLine(pen2, new Point(pos, 0), new Point(pos, Bounds.Height));
                }
                else if (paragraphEndList.Contains(pos))
                {
                    var pen1 = new Pen(Brushes.AntiqueWhite, 2);
                    context.DrawLine(pen1, new Point(pos, 0), new Point(pos, Bounds.Height));

                    var brush = new SolidColorBrush(Color.FromArgb(175, 110, 10, 10));
                    var pen2 = new Pen(brush, 2, dashStyle: DashStyle.Dash);
                    context.DrawLine(pen2, new Point(pos, 0), new Point(pos, Bounds.Height));
                }
                else
                {
                    var pen = new Pen(Brushes.AntiqueWhite, 1);
                    context.DrawLine(pen, new Point(pos, 0), new Point(pos, Bounds.Height));
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
            var isOnShotChange = GetShotChangeIndex(CurrentVideoPositionSeconds) >= 0;

            if (isOnShotChange)
            {
                var paintCurrentPositionOverlap = new Pen(Brushes.LightCyan, 1.5)
                {
                    DashStyle = DashStyle.Dash,
                };

                context.DrawLine(paintCurrentPositionOverlap,
                    new Point(currentPositionPos, 0),
                    new Point(currentPositionPos, Bounds.Height));
            }
            else
            {
                context.DrawLine(_paintPenCursor,
                    new Point(currentPositionPos, 0),
                    new Point(currentPositionPos, Bounds.Height));
            }
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
        var normalizedValue = value / WavePeaks.HighestPeak * VerticalZoomFactor;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int SecondsToSampleIndex(double seconds)
    {
        if (WavePeaks == null)
        {
            return 0;
        }

        return (int)Math.Round(seconds * WavePeaks.SampleRate, MidpointRounding.AwayFromZero);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double SampleIndexToSeconds(int index)
    {
        if (WavePeaks == null)
        {
            return 0;
        }

        return (double)index / WavePeaks.SampleRate;
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

    internal int GetShotChangeIndex(double seconds)
    {
        if (ShotChanges == null)
        {
            return -1;
        }

        try
        {
            for (var index = 0; index < ShotChanges.Count; index++)
            {
                var shotChange = ShotChanges[index];
                if (Math.Abs(shotChange - seconds) < 0.04)
                {
                    return index;
                }
            }
        }
        catch
        {
            // ignored
        }

        return -1;
    }

    internal void SetSpectrogram(SpectrogramData2 spectrogram)
    {
        if (_spectrogram != null)
        {
            _spectrogram.Dispose();
            _spectrogram = null;
        }

        if (spectrogram == null)
        {
            return;
        }

        if (spectrogram.IsLoaded)
        {
            InitializeSpectrogramInternal(spectrogram);
        }
        else
        {
            Task.Factory.StartNew(() =>
            {
                spectrogram.Load();
                Dispatcher.UIThread.Post(() =>
                {
                    InitializeSpectrogramInternal(spectrogram);
                });
            });
        }
    }

    public bool HasSpectrogram()
    {
        return _spectrogram != null &&
               _spectrogram.Images != null &&
               _spectrogram.Images.Count > 0;
    }

    private void InitializeSpectrogramInternal(SpectrogramData2 spectrogram)
    {
        if (_spectrogram != null)
        {
            return;
        }

        _spectrogram = spectrogram;
    }

    public double FindDataBelowThreshold(double thresholdPercent, double durationInSeconds)
    {
        if (WavePeaks == null || WavePeaks.Peaks.Count == 0)
        {
            return -1;
        }

        var begin = SecondsToSampleIndex(CurrentVideoPositionSeconds + 1);
        var length = SecondsToSampleIndex(durationInSeconds);
        var threshold = thresholdPercent / 100.0 * WavePeaks.HighestPeak;
        var hitCount = 0;
        for (var i = Math.Max(0, begin); i < WavePeaks.Peaks.Count; i++)
        {
            if (WavePeaks.Peaks[i].Abs <= threshold)
            {
                hitCount++;
            }
            else
            {
                hitCount = 0;
            }

            if (hitCount > length)
            {
                var seconds = RelativeXPositionToSeconds(i - (length / 2));
                if (seconds >= 0)
                {
                    StartPositionSeconds = seconds;
                    if (StartPositionSeconds > 1)
                    {
                        StartPositionSeconds -= 1;
                    }
                }

                return seconds;
            }
        }

        return -1;
    }

    /// <returns>video position in seconds, -1 if not found</returns>
    public double FindDataBelowThresholdBack(double thresholdPercent, double durationInSeconds)
    {
        if (WavePeaks == null || WavePeaks.Peaks.Count == 0)
        {
            return -1;
        }

        var begin = SecondsToSampleIndex(CurrentVideoPositionSeconds - 1);
        var length = SecondsToSampleIndex(durationInSeconds);
        var threshold = thresholdPercent / 100.0 * WavePeaks.HighestPeak;
        var hitCount = 0;
        for (var i = begin; i > 0; i--)
        {
            if (i < WavePeaks.Peaks.Count && WavePeaks.Peaks[i].Abs <= threshold)
            {
                hitCount++;
                if (hitCount > length)
                {
                    var seconds = RelativeXPositionToSeconds(i + length / 2);
                    if (seconds >= 0)
                    {
                        StartPositionSeconds = seconds;
                        if (StartPositionSeconds > 1)
                        {
                            StartPositionSeconds -= 1;
                        }
                        else
                        {
                            StartPositionSeconds = 0;
                        }
                    }

                    return seconds;
                }
            }
            else
            {
                hitCount = 0;
            }
        }

        return -1;
    }

    /// <summary>
    /// Seeks silence in volume
    /// </summary>
    /// <returns>video position in seconds, -1 if not found</returns>
    public double FindDataBelowThresholdBackForStart(double thresholdPercent, double durationInSeconds, double startSeconds)
    {
        if (WavePeaks == null || WavePeaks.Peaks.Count == 0)
        {
            return -1;
        }

        var min = Math.Max(0, SecondsToSampleIndex(startSeconds - 1));
        var maxShort = Math.Min(WavePeaks.Peaks.Count, SecondsToSampleIndex(startSeconds + durationInSeconds + 0.01));
        var max = Math.Min(WavePeaks.Peaks.Count, SecondsToSampleIndex(startSeconds + durationInSeconds + 0.8));
        var length = SecondsToSampleIndex(durationInSeconds);
        var threshold = thresholdPercent / 100.0 * WavePeaks.HighestPeak;

        var minMax = GetMinAndMax(min, max);
        const int lowPeakDifference = 4_000;
        if (minMax.Max - minMax.Min < lowPeakDifference)
        {
            return -1; // all audio about the same
        }

        // look for start silence in the beginning of subtitle
        min = SecondsToSampleIndex(startSeconds);
        var hitCount = 0;
        int index;
        for (index = min; index < max; index++)
        {
            if (index > 0 && index < WavePeaks.Peaks.Count && WavePeaks.Peaks[index].Abs <= threshold)
            {
                hitCount++;
            }
            else
            {
                minMax = GetMinAndMax(min, index);
                var currentMinMax = GetMinAndMax(SecondsToSampleIndex(startSeconds), SecondsToSampleIndex(startSeconds + 0.8));
                if (currentMinMax.Avg > minMax.Avg + 300 || currentMinMax.Avg < 1000 && minMax.Avg < 1000 && Math.Abs(currentMinMax.Avg - minMax.Avg) < 500)
                {
                    break;
                }
                hitCount = length / 2;
            }
        }

        if (hitCount > length)
        {
            minMax = GetMinAndMax(min, index);
            var currentMinMax = GetMinAndMax(SecondsToSampleIndex(startSeconds), SecondsToSampleIndex(startSeconds + 0.8));
            if (currentMinMax.Avg > minMax.Avg + 300 || currentMinMax.Avg < 1000 && minMax.Avg < 1000 && Math.Abs(currentMinMax.Avg - minMax.Avg) < 500)
            {
                return Math.Max(0, SampleIndexToSeconds(index - 1) - 0.01);
            }
        }

        // move start left?
        min = SecondsToSampleIndex(startSeconds - 1);
        hitCount = 0;
        for (index = maxShort; index > min; index--)
        {
            if (index > 0 && index < WavePeaks.Peaks.Count && WavePeaks.Peaks[index].Abs <= threshold)
            {
                hitCount++;
                if (hitCount > length)
                {
                    return Math.Max(0, SampleIndexToSeconds(index + length) - 0.01);
                }
            }
            else
            {
                hitCount = 0;
            }
        }

        return -1;
    }

    private MinMax GetMinAndMax(int startIndex, int endIndex)
    {
        if (WavePeaks == null || WavePeaks.Peaks.Count == 0)
        {
            return new MinMax { Min = 0, Max = 0, Avg = 0 };
        }

        var minPeak = int.MaxValue;
        var maxPeak = int.MinValue;
        double total = 0;
        for (var i = startIndex; i < endIndex; i++)
        {
            var v = WavePeaks.Peaks[i].Abs;
            total += v;
            if (v < minPeak)
            {
                minPeak = v;
            }
            if (v > maxPeak)
            {
                maxPeak = v;
            }
        }

        return new MinMax { Min = minPeak, Max = maxPeak, Avg = total / (endIndex - startIndex) };
    }

    internal void GenerateTimeCodes(Subtitle subtitle, double startFromSeconds, int blockSizeMilliseconds, int minimumVolumePercent, int maximumVolumePercent, int defaultMilliseconds)
    {
        if (WavePeaks == null || WavePeaks.Peaks.Count == 0)
        {
            return;
        }

        var begin = SecondsToSampleIndex(startFromSeconds);

        double average = 0;
        for (int k = begin; k < WavePeaks.Peaks.Count; k++)
        {
            average += WavePeaks.Peaks[k].Abs;
        }

        average /= WavePeaks.Peaks.Count - begin;

        var maxThreshold = (int)(WavePeaks.HighestPeak * (maximumVolumePercent / 100.0));
        var silenceThreshold = (int)(average * (minimumVolumePercent / 100.0));

        int length50Ms = SecondsToSampleIndex(0.050);
        double secondsPerParagraph = defaultMilliseconds / TimeCode.BaseUnit;
        int minBetween = SecondsToSampleIndex(Configuration.Settings.General.MinimumMillisecondsBetweenLines / TimeCode.BaseUnit);
        bool subtitleOn = false;
        int i = begin;
        while (i < WavePeaks.Peaks.Count)
        {
            if (subtitleOn)
            {
                var currentLengthInSeconds = SampleIndexToSeconds(i - begin);
                if (currentLengthInSeconds > 1.0)
                {
                    subtitleOn = EndParagraphDueToLowVolume(subtitle, blockSizeMilliseconds, silenceThreshold, begin, true, i);
                    if (!subtitleOn)
                    {
                        begin = i + minBetween;
                        i = begin;
                    }
                }
                if (subtitleOn && currentLengthInSeconds >= secondsPerParagraph)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        subtitleOn = EndParagraphDueToLowVolume(subtitle, blockSizeMilliseconds, silenceThreshold, begin, true, i + (j * length50Ms));
                        if (!subtitleOn)
                        {
                            i += (j * length50Ms);
                            begin = i + minBetween;
                            i = begin;
                            break;
                        }
                    }

                    if (subtitleOn) // force break
                    {
                        var p = new Paragraph(string.Empty, SampleIndexToSeconds(begin) * TimeCode.BaseUnit, SampleIndexToSeconds(i) * TimeCode.BaseUnit);
                        subtitle.Paragraphs.Add(p);
                        begin = i + minBetween;
                        i = begin;
                    }
                }
            }
            else
            {
                double avgVol = GetAverageVolumeForNextMilliseconds(i, blockSizeMilliseconds);
                if (avgVol > silenceThreshold && avgVol < maxThreshold)
                {
                    subtitleOn = true;
                    begin = i;
                }
            }
            i++;
        }

        subtitle.Renumber();
    }

    private bool EndParagraphDueToLowVolume(Subtitle subtitle, int blockSizeMilliseconds, double silenceThreshold, int begin, bool subtitleOn, int i)
    {
        var avgVol = GetAverageVolumeForNextMilliseconds(i, blockSizeMilliseconds);
        if (avgVol < silenceThreshold)
        {
            var p = new Paragraph(string.Empty, SampleIndexToSeconds(begin) * TimeCode.BaseUnit, SampleIndexToSeconds(i) * TimeCode.BaseUnit);
            subtitle.Paragraphs.Add(p);
            subtitleOn = false;
        }

        return subtitleOn;
    }

    private double GetAverageVolumeForNextMilliseconds(int sampleIndex, int milliseconds)
    {
        if (WavePeaks == null)
        {
            return 0;
        }

        // length cannot be less than 9
        var length = Math.Max(SecondsToSampleIndex(milliseconds / TimeCode.BaseUnit), 9);
        var max = Math.Min(sampleIndex + length, WavePeaks.Peaks.Count);
        var from = Math.Max(sampleIndex, 1);

        if (from >= max)
        {
            return 0;
        }

        double v = 0;
        for (var i = from; i < max; i++)
        {
            v += WavePeaks.Peaks[i].Abs;
        }

        return v / (max - from);
    }

    internal void CenterOnPosition(double position)
    {
        if (WavePeaks == null)
        {
            return;
        }

        var halfWidthInSeconds = (Bounds.Width / 2) / (WavePeaks.SampleRate * ZoomFactor);
        StartPositionSeconds = Math.Max(0, position - halfWidthInSeconds);
    }

    internal void CenterOnPosition(SubtitleLineViewModel line)
    {
        if (WavePeaks == null)
        {
            return;
        }

        var halfWidthInSeconds = (Bounds.Width / 2.0) / (WavePeaks.SampleRate * ZoomFactor) - (line.Duration.TotalSeconds / 2.0);
        StartPositionSeconds = Math.Max(0, line.StartTime.TotalSeconds - halfWidthInSeconds);
    }

    internal void SetDisplayMode(WaveformDisplayMode displayMode)
    {
        _displayMode = displayMode;
    }

    internal WaveformDisplayMode GetDisplayMode()
    {
        return _displayMode;
    }
}