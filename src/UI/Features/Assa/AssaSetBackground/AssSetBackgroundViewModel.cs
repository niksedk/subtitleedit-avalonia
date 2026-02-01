using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa.AssaSetBackground;

public partial class AssSetBackgroundViewModel : ObservableObject
{
    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    // Padding settings
    [ObservableProperty] private int _paddingLeft = 10;
    [ObservableProperty] private int _paddingRight = 10;
    [ObservableProperty] private int _paddingTop = 5;
    [ObservableProperty] private int _paddingBottom = 5;

    // Style settings
    [ObservableProperty] private Color _boxColor = Color.FromRgb(0, 0, 0);
    [ObservableProperty] private Color _outlineColor = Colors.White;
    [ObservableProperty] private Color _shadowColor = Color.FromRgb(128, 128, 128);
    [ObservableProperty] private int _outlineWidth = 2;
    [ObservableProperty] private int _shadowDistance = 1;
    [ObservableProperty] private int _boxStyleIndex; // 0=square, 1=rounded, 2=circle
    [ObservableProperty] private ObservableCollection<string> _boxStyles = [];
    [ObservableProperty] private int _radius = 10;

    // Fill width
    [ObservableProperty] private bool _fillWidth;
    [ObservableProperty] private int _fillWidthMarginLeft;
    [ObservableProperty] private int _fillWidthMarginRight;

    private Subtitle _subtitle = new();
    private int[]? _selectedIndices;
    private int _videoWidth = 1920;
    private int _videoHeight = 1080;
    private readonly Random _random = new();

    public Subtitle ResultSubtitle => _subtitle;

    public AssSetBackgroundViewModel()
    {
        BoxStyles.Add(Se.Language.Assa.ProgressBarSquareCorners);
        BoxStyles.Add(Se.Language.Assa.ProgressBarRoundedCorners);
        BoxStyles.Add(Se.Language.Assa.BackgroundBoxCircle);
    }

    public void Initialize(Subtitle subtitle, int[]? selectedIndices, int videoWidth, int videoHeight)
    {
        _subtitle = new Subtitle(subtitle, false);
        _selectedIndices = selectedIndices;
        _videoWidth = videoWidth > 0 ? videoWidth : 1920;
        _videoHeight = videoHeight > 0 ? videoHeight : 1080;

        if (string.IsNullOrWhiteSpace(_subtitle.Header))
        {
            _subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
        }
    }

    [RelayCommand]
    private void Ok()
    {
        ApplyBackgroundBoxes();
        OkPressed = true;
        Close();
    }

    private void ApplyBackgroundBoxes()
    {
        if (string.IsNullOrEmpty(_subtitle.Header))
        {
            _subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
        }

        // Generate unique style name
        var boxStyleName = GenerateUniqueStyleName();

        // Add style
        var styleBoxBg = new SsaStyle
        {
            Alignment = "7",
            Name = boxStyleName,
            MarginLeft = 0,
            MarginRight = 0,
            MarginVertical = 0,
            Primary = SkiaSharp.SKColor.Parse($"#{BoxColor.A:X2}{BoxColor.B:X2}{BoxColor.G:X2}{BoxColor.R:X2}"),
            Secondary = SkiaSharp.SKColor.Parse($"#{ShadowColor.A:X2}{ShadowColor.B:X2}{ShadowColor.G:X2}{ShadowColor.R:X2}"),
            Tertiary = SkiaSharp.SKColor.Parse($"#{ShadowColor.A:X2}{ShadowColor.B:X2}{ShadowColor.G:X2}{ShadowColor.R:X2}"),
            Background = SkiaSharp.SKColor.Parse($"#{ShadowColor.A:X2}{ShadowColor.B:X2}{ShadowColor.G:X2}{ShadowColor.R:X2}"),
            Outline = SkiaSharp.SKColor.Parse($"#{OutlineColor.A:X2}{OutlineColor.B:X2}{OutlineColor.G:X2}{OutlineColor.R:X2}"),
            ShadowWidth = ShadowDistance,
            OutlineWidth = OutlineWidth,
        };
        _subtitle.Header = AdvancedSubStationAlpha.UpdateOrAddStyle(_subtitle.Header, styleBoxBg);

        // Get indices to process
        var indices = _selectedIndices ?? Enumerable.Range(0, _subtitle.Paragraphs.Count).ToArray();

        // Generate background boxes for each paragraph
        foreach (var index in indices)
        {
            if (index < 0 || index >= _subtitle.Paragraphs.Count)
            {
                continue;
            }

            var p = _subtitle.Paragraphs[index];
            if (p.IsComment)
            {
                continue;
            }

            // For simplicity, create a basic box that covers the full width if FillWidth is enabled
            var left = FillWidth ? FillWidthMarginLeft : PaddingLeft;
            var right = FillWidth ? (_videoWidth - FillWidthMarginRight) : (_videoWidth - PaddingRight);
            var top = PaddingTop;
            var bottom = _videoHeight - PaddingBottom;

            var boxDrawing = GenerateBackgroundBox(left, right, top, bottom);

            var boxParagraph = new Paragraph(boxDrawing, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds)
            {
                Layer = -1000,
                Extra = boxStyleName,
            };

            _subtitle.InsertParagraphInCorrectTimeOrder(boxParagraph);
        }

        _subtitle.Renumber();
    }

    private string GenerateBackgroundBox(int left, int right, int top, int bottom)
    {
        var width = right - left;
        var height = bottom - top;

        if (BoxStyleIndex == 1 && Radius > 0) // Rounded corners
        {
            var r = Math.Min(Radius, Math.Min(width / 2, height / 2));
            return $"{{\\p1}}m {left + r} {top} l {right - r} {top} b {right} {top} {right} {top + r} {right} {top + r} l {right} {bottom - r} b {right} {bottom} {right - r} {bottom} {right - r} {bottom} l {left + r} {bottom} b {left} {bottom} {left} {bottom - r} {left} {bottom - r} l {left} {top + r} b {left} {top} {left + r} {top} {left + r} {top}{{\\p0}}";
        }
        else if (BoxStyleIndex == 2) // Circle
        {
            var centerX = (left + right) / 2;
            var centerY = (top + bottom) / 2;
            var radiusX = width / 2;
            var radiusY = height / 2;

            // Simplified circle approximation using bezier curves
            var kappa = 0.5522847498;
            var ox = (int)(radiusX * kappa);
            var oy = (int)(radiusY * kappa);

            return $"{{\\p1}}m {centerX} {centerY - radiusY} b {centerX + ox} {centerY - radiusY} {centerX + radiusX} {centerY - oy} {centerX + radiusX} {centerY} b {centerX + radiusX} {centerY + oy} {centerX + ox} {centerY + radiusY} {centerX} {centerY + radiusY} b {centerX - ox} {centerY + radiusY} {centerX - radiusX} {centerY + oy} {centerX - radiusX} {centerY} b {centerX - radiusX} {centerY - oy} {centerX - ox} {centerY - radiusY} {centerX} {centerY - radiusY}{{\\p0}}";
        }

        // Square corners (default)
        return $"{{\\p1}}m {left} {top} l {right} {top} {right} {bottom} {left} {bottom}{{\\p0}}";
    }

    private string GenerateUniqueStyleName()
    {
        var baseName = "SE-box-bg";
        var styleNames = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);

        if (!styleNames.Contains(baseName))
        {
            return baseName;
        }

        var tryCount = 0;
        while (tryCount < 100)
        {
            var name = $"SE-box-bg{_random.Next(1000, 9999)}";
            if (!styleNames.Contains(name))
            {
                return name;
            }
            tryCount++;
        }

        return $"SE-box-bg{Guid.NewGuid():N}";
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}
