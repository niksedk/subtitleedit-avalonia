using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;

public class AudioVisualizer : Control
{
    // Properties
    public static readonly StyledProperty<WavePeakData> WavePeaksProperty =
        AvaloniaProperty.Register<AudioVisualizer, WavePeakData>(nameof(WavePeaks));

    public static readonly StyledProperty<double> StartPositionSecondsProperty =
        AvaloniaProperty.Register<AudioVisualizer, double>(nameof(StartPositionSeconds));

    public static readonly StyledProperty<double> ZoomFactorProperty =
        AvaloniaProperty.Register<AudioVisualizer, double>(nameof(ZoomFactor), 1.0);

    public static readonly StyledProperty<double> VerticalZoomFactorProperty =
        AvaloniaProperty.Register<AudioVisualizer, double>(nameof(VerticalZoomFactor), 1.0);

    public static readonly StyledProperty<double> CurrentVideoPositionSecondsProperty =
        AvaloniaProperty.Register<AudioVisualizer, double>(nameof(CurrentVideoPositionSeconds));

    public static readonly StyledProperty<bool> MouseOverProperty =
        AvaloniaProperty.Register<AudioVisualizer, bool>(nameof(MouseOver));

    public static readonly StyledProperty<bool> ShowGridLinesProperty =
        AvaloniaProperty.Register<AudioVisualizer, bool>(nameof(ShowGridLines));

    public static readonly StyledProperty<List<Paragraph>> AllSelectedParagraphsProperty =
        AvaloniaProperty.Register<AudioVisualizer, List<Paragraph>>(nameof(AllSelectedParagraphs));

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

    public bool MouseOver
    {
        get => GetValue(MouseOverProperty);
        set => SetValue(MouseOverProperty, value);
    }

    public bool ShowGridLines
    {
        get => GetValue(ShowGridLinesProperty);
        set => SetValue(ShowGridLinesProperty, value);
    }

    public List<Paragraph> AllSelectedParagraphs
    {
        get => GetValue(AllSelectedParagraphsProperty);
        set => SetValue(AllSelectedParagraphsProperty, value);
    }

    // Pens and brushes
    private readonly Pen _paintWaveform = new Pen(Brushes.LightGreen, 1);
    private readonly Pen _paintPenSelected = new Pen(Brushes.LightPink, 1);
    private readonly Pen _paintGridLines = new Pen(Brushes.DarkGray, 0.2);
    private readonly Pen _paintCurrentPosition = new Pen(Brushes.Cyan, 1);
    private readonly IBrush _mouseOverBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 0));

    static AudioVisualizer()
    {
        AffectsRender<AudioVisualizer>(
            WavePeaksProperty,
            StartPositionSecondsProperty,
            ZoomFactorProperty,
            VerticalZoomFactorProperty,
            CurrentVideoPositionSecondsProperty,
            MouseOverProperty,
            AllSelectedParagraphsProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        using (context.PushClip(new Rect(0, 0, Bounds.Width, Bounds.Height)))
        {
            DrawGridLines(context);
            DrawWaveForm(context);
            DrawParagraphs(context);
            DrawCurrentVideoPosition(context);

            if (MouseOver)
            {
                //context.DrawRectangle(_mouseOverBrush, new Rect(0, 0, Bounds.Width, Bounds.Height));
            }
        }
    }

    private void DrawGridLines(DrawingContext context)
    {
        if (!ShowGridLines)
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

        if (div <= 0) return;

        for (var x = 0; x < Bounds.Width; x++)
        {
            var pos = (StartPositionSeconds + x / div) * WavePeaks.SampleRate;
            var pos0 = (int)pos;
            var pos1 = pos0 + 1;

            if (pos1 >= WavePeaks.Peaks.Count)
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
        // Implement paragraph drawing logic here
        // This would depend on your paragraph data structure
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

    private double CalculateY(double value, double baseHeight, double halfWaveformHeight)
    {
        if (WavePeaks == null)
        {
            return halfWaveformHeight;
        }

        // Normalize the value to the control's height
        var normalizedValue = value / WavePeaks.HighestPeak;
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
}