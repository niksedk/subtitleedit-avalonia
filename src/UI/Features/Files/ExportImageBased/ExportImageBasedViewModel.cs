using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public partial class ExportImageBasedViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<string> _fontFamilies;
    [ObservableProperty] string? _selectedFontFamily;
    [ObservableProperty] private ObservableCollection<int> _fontSizes;
    [ObservableProperty] int? _selectedFontSize;
    [ObservableProperty] private ObservableCollection<ResolutionItem> _resolutions;
    [ObservableProperty] ResolutionItem? _selectedResolution;
    [ObservableProperty] private ObservableCollection<int> _topBottomMargins;
    [ObservableProperty] int? _selectedTopBottomMargin;
    [ObservableProperty] private ObservableCollection<int> _leftRightMargins;
    [ObservableProperty] int? _selectedLeftRightMargin;
    [ObservableProperty] private ObservableCollection<double> _outlineWidths;
    [ObservableProperty] double? _selectedOutlineWidth;
    [ObservableProperty] private ObservableCollection<double> _shadowWidths;
    [ObservableProperty] double _selectedShadowWidth;
    [ObservableProperty] private bool _isBold;
    [ObservableProperty] private Color _fontColor;
    [ObservableProperty] private Color _shadowColor;
    [ObservableProperty] private Bitmap _bitmapPreview;
    [ObservableProperty] private Color _outlineColor;
    [ObservableProperty] private double _lineGapPercentage;
    [ObservableProperty] private Color _boxColor;
    [ObservableProperty] private ObservableCollection<double> _boxCornerRadiusList;
    [ObservableProperty] private double _selectedBoxCornerRadius;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public DataGrid SubtitleGrid { get; set; }

    private string _subtitleFileName;
    private string _videoFileName;
    private bool _dirty;
    private readonly Timer _timerUpdatePreview;
    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;

    public ExportImageBasedViewModel(IFileHelper fileHelper, IFolderHelper folderHelper)
    {
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;

        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        FontFamilies = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        SelectedFontFamily = FontFamilies.FirstOrDefault();
        FontSizes = new ObservableCollection<int>(Enumerable.Range(15, 486));
        SelectedFontSize = 20;
        Resolutions = new ObservableCollection<ResolutionItem>(ResolutionItem.GetResolutions());
        SelectedResolution = Resolutions.FirstOrDefault(r => r.Width == 1920 && r.Height == 1080);
        TopBottomMargins = new ObservableCollection<int> { 0, 5, 10, 15, 20, 25, 30 };
        SelectedTopBottomMargin = 10;
        LeftRightMargins = new ObservableCollection<int> { 0, 5, 10, 15, 20, 25, 30 };
        SelectedLeftRightMargin = 10;
        OutlineWidths = new ObservableCollection<double>(Enumerable.Range(0, 16).Select(i => (double)i));
        SelectedOutlineWidth = OutlineWidths.FirstOrDefault();
        ShadowWidths = new ObservableCollection<double>(Enumerable.Range(0, 16).Select(i => (double)i));
        BoxCornerRadiusList = new ObservableCollection<double>(Enumerable.Range(0, 51).Select(i => (double)i));
        SelectedBoxCornerRadius = 0;
        SelectedShadowWidth = 3;
        FontColor = Colors.White;
        ShadowColor = Colors.Black;
        SubtitleGrid = new DataGrid();
        Title = string.Empty;
        BitmapPreview = new SKBitmap(1, 1).ToAvaloniaBitmap();
        OutlineColor = Colors.Black;

        _subtitleFileName = string.Empty;
        _videoFileName = string.Empty;

        _timerUpdatePreview = new Timer();
        _timerUpdatePreview.Interval = 250;
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
    }

    private void TimerUpdatePreviewElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_dirty)
        {
            _dirty = false;
            Dispatcher.UIThread.Post(SubtitleLineChanged);
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private async Task Export()
    {
        IExportHandler exportImageHandler = new ExportHandlerBluRaySup();
        var fileOrFolderName = string.Empty;
        if (exportImageHandler.UseFileName)
        {
            fileOrFolderName = await _fileHelper.PickSaveSubtitleFile(Window!, exportImageHandler.Extension, string.Empty, exportImageHandler.Title);
        }
        else
        {
            fileOrFolderName = await _folderHelper.PickFolderAsync(Window!, exportImageHandler.Title);
        }

        if (string.IsNullOrEmpty(fileOrFolderName))
        {
            return;
        }

        var imageParameters = new List<ImageParameter>();
        for (int i = 0; i < Subtitles.Count; i++)
        {
            SubtitleLineViewModel? subtitle = Subtitles[i];
            var imageParameter = new ImageParameter
            {
                Alignment = ExportAlignment.BottomCenter,
                Index = i,
                Text = subtitle.Text,
                StartTime = subtitle.StartTime,
                EndTime = subtitle.EndTime,
                FontColor = FontColor.ToSKColor(),
                FontName = SelectedFontFamily ?? "Arial",
                FontSize = SelectedFontSize ?? 20,
                IsBold = IsBold,
                OutlineColor = OutlineColor.ToSKColor(),
                OutlineWidth = SelectedOutlineWidth ?? 0,
                ShadowColor = ShadowColor.ToSKColor(),
                ShadowWidth = SelectedShadowWidth,
                BackgroundColor = BoxColor.ToSKColor(),
                BackgroundCornerRadius = SelectedBoxCornerRadius,
                ScreenWidth = SelectedResolution?.Width ?? 1920,
                ScreenHeight = SelectedResolution?.Height ?? 1080,
                BottomTopMargin = SelectedTopBottomMargin ?? 10,
                LeftRightMargin = SelectedLeftRightMargin ?? 10,
            };
            imageParameters.Add(imageParameter);
        }

        for (var i = 0; i < Subtitles.Count; i++)
        {
            var ip = imageParameters[i];
            ip.Bitmap = GenerateBitmap(ip);
            exportImageHandler.CreateParagraph(ip);
        }

        exportImageHandler.WriteHeader(fileOrFolderName, SelectedResolution?.Width ?? 1920, SelectedResolution?.Height ?? 1080);
        for (var i = 0; i < Subtitles.Count; i++)
        {
            var ip = imageParameters[i];
            exportImageHandler.WriteParagraph(ip);
        }
        exportImageHandler.WriteFooter();
    }

    [RelayCommand]
    private void ChangeFontColor()
    {
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }

    public void Initialize(
        ExportImageType exportImageType,
        ObservableCollection<SubtitleLineViewModel> subtitles,
        string? subtitleFileName,
        string? videoFileName)
    {
        Subtitles.Clear();
        Subtitles.AddRange(subtitles);

        if (exportImageType == ExportImageType.BluRaySup)
        {
            Title = Se.Language.File.Export.TitleExportBluRaySup;
        }

        SelectedSubtitle = Subtitles.FirstOrDefault();
    }

    public void SubtitleGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SubtitleLineChanged();
    }

    private void SubtitleLineChanged()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        var text = selected.Text;
        if (string.IsNullOrEmpty(text))
        {
            BitmapPreview = new SKBitmap(1, 1).ToAvaloniaBitmap();
            return;
        }

        var ip = new ImageParameter
        {
            Text = text,
            FontName = SelectedFontFamily ?? "Arial",
            FontSize = SelectedFontSize ?? 20,
            FontColor = FontColor.ToSKColor(),
            OutlineColor = OutlineColor.ToSKColor(),
            OutlineWidth = SelectedOutlineWidth ?? 0,
            ShadowColor = ShadowColor.ToSKColor(),
            ShadowWidth = SelectedShadowWidth,
        };

        BitmapPreview = GenerateBitmap(ip).ToAvaloniaBitmap();
    }

    private SKBitmap GenerateBitmap(ImageParameter ip)
    {
        var fontName = ip.FontName;
        var fontSize = ip.FontSize;
        var fontColor = ip.FontColor;

        var outlineColor = OutlineColor.ToSKColor();
        var outlineWidth = SelectedOutlineWidth ?? 0;

        var shadowColor = ShadowColor.ToSKColor();
        var shadowWidth = SelectedShadowWidth;

        // Parse text and create text segments with styling
        var segments = ParseTextWithStyling(ip.Text, fontColor);

        // Create fonts
        using var regularTypeface = SKTypeface.FromFamilyName(fontName, SKFontStyle.Normal);
        using var boldTypeface = SKTypeface.FromFamilyName(fontName, SKFontStyle.Bold);
        using var italicTypeface = SKTypeface.FromFamilyName(fontName, SKFontStyle.Italic);
        using var boldItalicTypeface = SKTypeface.FromFamilyName(fontName, SKFontStyle.BoldItalic);

        using var regularFont = new SKFont(regularTypeface, fontSize);
        using var boldFont = new SKFont(boldTypeface, fontSize);
        using var italicFont = new SKFont(italicTypeface, fontSize);
        using var boldItalicFont = new SKFont(boldItalicTypeface, fontSize);

        // Split segments into lines and measure dimensions
        var lines = SplitIntoLines(segments);
        var fontMetrics = regularFont.Metrics;
        var lineHeight = fontMetrics.Descent - fontMetrics.Ascent;
        var lineSpacing = lineHeight * 0.17f; //TODO: Move to UI setting!!! -  % of line height 

        float maxWidth = 0;
        foreach (var line in lines)
        {
            float lineWidth = 0;
            foreach (var segment in line)
            {
                var currentFont = GetFont(segment, regularFont, boldFont, italicFont, boldItalicFont);
                lineWidth += currentFont.MeasureText(segment.Text);
            }
            maxWidth = Math.Max(maxWidth, lineWidth);
        }

        // Calculate bitmap dimensions with padding (including outline width and shadow)
        var outlinePadding = (float)Math.Ceiling(outlineWidth);
        var shadowPadding = (float)Math.Ceiling(shadowWidth);
        var padding = 10 + Math.Max(outlinePadding, shadowPadding);
        var width = (int)Math.Ceiling(maxWidth) + (int)(padding * 2) + (int)Math.Ceiling(shadowWidth);
        var totalHeight = (lines.Count * lineHeight) + ((lines.Count - 1) * lineSpacing);
        var height = (int)Math.Ceiling(totalHeight) + (int)(padding * 2) + (int)Math.Ceiling(shadowWidth);

        // Ensure minimum size
        width = Math.Max(width, 1);
        height = Math.Max(height, 1);

        // Create bitmap and canvas
        var bitmap = new SKBitmap(width, height, true);
        using var canvas = new SKCanvas(bitmap);

        // Set up paint
        using var paint = new SKPaint
        {
            Color = BoxColor.ToSKColor(),
            IsAntialias = true,
        };

        // Define the rounded rectangle
        var rect = new SKRect(0, 0, width, height);
        float cornerRadius = (float)SelectedBoxCornerRadius;

        // Draw the rounded rectangle
        canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, paint);


        // Draw text lines
        float currentY = padding - fontMetrics.Ascent;

        foreach (var line in lines)
        {
            float currentX = padding;

            for (int i = 0; i < line.Count; i++)
            {
                var segment = line[i];
                var currentFont = GetFont(segment, regularFont, boldFont, italicFont, boldItalicFont);

                // Draw shadow first (if shadow width > 0)
                if (shadowWidth > 0)
                {
                    // Offset shadow to right and down
                    var shadowOffsetX = currentX + (float)shadowWidth;
                    var shadowOffsetY = currentY + (float)shadowWidth;

                    // If we have an outline, draw shadow of the outline
                    if (outlineWidth > 0)
                    {
                        using var shadowOutlinePaint = new SKPaint
                        {
                            Color = shadowColor,
                            IsAntialias = true,
                            Style = SKPaintStyle.Stroke,
                            StrokeWidth = (float)outlineWidth,
                            StrokeJoin = SKStrokeJoin.Round,
                            StrokeCap = SKStrokeCap.Round
                        };

                        canvas.DrawText(segment.Text, shadowOffsetX, shadowOffsetY, currentFont, shadowOutlinePaint);
                    }

                    // Always draw shadow of the text fill (this creates the shadow behind the text)
                    using var shadowTextPaint = new SKPaint
                    {
                        Color = shadowColor,
                        IsAntialias = true,
                        Style = SKPaintStyle.Fill
                    };

                    canvas.DrawText(segment.Text, shadowOffsetX, shadowOffsetY, currentFont, shadowTextPaint);
                }

                // Draw outline second (if outline width > 0)
                if (outlineWidth > 0)
                {
                    using var outlinePaint = new SKPaint
                    {
                        Color = outlineColor,
                        IsAntialias = true,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = (float)outlineWidth,
                        StrokeJoin = SKStrokeJoin.Round,
                        StrokeCap = SKStrokeCap.Round
                    };

                    canvas.DrawText(segment.Text, currentX, currentY, currentFont, outlinePaint);
                }

                // Draw the main text on top
                using var textPaint = new SKPaint
                {
                    Color = segment.Color,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };

                canvas.DrawText(segment.Text, currentX, currentY, currentFont, textPaint);
                currentX += currentFont.MeasureText(segment.Text);

                // Add small spacing after styled segments to prevent crowding
                if ((segment.IsItalic || segment.IsBold) && i < line.Count - 1)
                {
                    currentX += fontSize * 0.17f; // Add 17% of font size as spacing
                }
            }

            // Move to next line
            currentY += lineHeight + lineSpacing;
        }

        return bitmap;
    }


    private static SKFont GetFont(TextSegment segment, SKFont regular, SKFont bold, SKFont italic, SKFont boldItalic)
    {
        if (segment.IsBold && segment.IsItalic)
        {
            return boldItalic;
        }

        if (segment.IsBold)
        {
            return bold;
        }

        if (segment.IsItalic)
        {
            return italic;
        }

        return regular;
    }

    static List<List<TextSegment>> SplitIntoLines(List<TextSegment> segments)
    {
        var lines = new List<List<TextSegment>>();
        var currentLine = new List<TextSegment>();

        foreach (var segment in segments)
        {
            var text = segment.Text;
            var parts = text.SplitToLines();

            for (int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];

                if (!string.IsNullOrEmpty(part))
                {
                    currentLine.Add(new TextSegment(part, segment.IsItalic, segment.IsBold, segment.Color));
                }

                // Add line break (except for the last part)
                if (i < parts.Count - 1)
                {
                    if (currentLine.Count > 0)
                    {
                        lines.Add(currentLine);
                        currentLine = new List<TextSegment>();
                    }
                    else
                    {
                        // Empty line
                        lines.Add(new List<TextSegment>());
                    }
                }
            }
        }

        // Add the last line if it has content
        if (currentLine.Count > 0)
        {
            lines.Add(currentLine);
        }

        // Ensure we have at least one line
        if (lines.Count == 0)
        {
            lines.Add(new List<TextSegment>());
        }

        return lines;
    }

    static List<TextSegment> ParseTextWithStyling(string text, SKColor defaultFontColor)
    {
        var segments = new List<TextSegment>();
        var currentPos = 0;
        var styleStack = new Stack<TextStyle>();
        var currentStyle = new TextStyle() { Color = defaultFontColor };

        while (currentPos < text.Length)
        {
            var nextTagPos = FindNextTag(text, currentPos);

            if (nextTagPos == -1)
            {
                // No more tags, add remaining text
                if (currentPos < text.Length)
                {
                    var remainingText = text.Substring(currentPos);
                    if (!string.IsNullOrEmpty(remainingText))
                    {
                        segments.Add(new TextSegment(remainingText, currentStyle.IsItalic, currentStyle.IsBold, currentStyle.Color));
                    }
                }
                break;
            }

            // Add text before the tag
            if (nextTagPos > currentPos)
            {
                var beforeTag = text.Substring(currentPos, nextTagPos - currentPos);
                if (!string.IsNullOrEmpty(beforeTag))
                {
                    segments.Add(new TextSegment(beforeTag, currentStyle.IsItalic, currentStyle.IsBold, currentStyle.Color));
                }
            }

            // Process the tag
            var tagInfo = ParseTag(text, nextTagPos, defaultFontColor);
            if (tagInfo != null)
            {
                switch (tagInfo.TagType)
                {
                    case TagType.ItalicOpen:
                        styleStack.Push(currentStyle);
                        currentStyle = currentStyle with { IsItalic = true };
                        break;
                    case TagType.ItalicClose:
                        if (styleStack.Count > 0)
                        {
                            currentStyle = styleStack.Pop();
                        }
                        else
                        {
                            currentStyle = currentStyle with { IsItalic = false };
                        }

                        break;
                    case TagType.BoldOpen:
                        styleStack.Push(currentStyle);
                        currentStyle = currentStyle with { IsBold = true };
                        break;
                    case TagType.BoldClose:
                        if (styleStack.Count > 0)
                        {
                            currentStyle = styleStack.Pop();
                        }
                        else
                        {
                            currentStyle = currentStyle with { IsBold = false };
                        }

                        break;
                    case TagType.FontOpen:
                        styleStack.Push(currentStyle);
                        currentStyle = currentStyle with { Color = tagInfo.Color ?? currentStyle.Color };
                        break;
                    case TagType.FontClose:
                        if (styleStack.Count > 0)
                        {
                            currentStyle = styleStack.Pop();
                        }
                        else
                        {
                            currentStyle = currentStyle with { Color = defaultFontColor };
                        }

                        break;
                }
                currentPos = tagInfo.EndPosition;
            }
            else
            {
                // Invalid tag, treat as regular text
                currentPos++;
            }
        }

        // Filter out empty segments
        return segments.Where(s => !string.IsNullOrEmpty(s.Text)).ToList();
    }

    private static int FindNextTag(string text, int startPos)
    {
        var openBracket = text.IndexOf('<', startPos);
        return openBracket;
    }

    private static TagInfo? ParseTag(string text, int startPos, SKColor defaultFontColor)
    {
        if (startPos >= text.Length || text[startPos] != '<')
        {
            return null;
        }

        var endBracket = text.IndexOf('>', startPos);
        if (endBracket == -1)
        {
            return null;
        }

        var tagContent = text.Substring(startPos + 1, endBracket - startPos - 1);
        var endPosition = endBracket + 1;

        // Check for specific tags
        if (tagContent.Equals("i", StringComparison.OrdinalIgnoreCase))
        {
            return new TagInfo(TagType.ItalicOpen, endPosition);
        }

        if (tagContent.Equals("/i", StringComparison.OrdinalIgnoreCase))
        {
            return new TagInfo(TagType.ItalicClose, endPosition);
        }

        if (tagContent.Equals("b", StringComparison.OrdinalIgnoreCase))
        {
            return new TagInfo(TagType.BoldOpen, endPosition);
        }

        if (tagContent.Equals("/b", StringComparison.OrdinalIgnoreCase))
        {
            return new TagInfo(TagType.BoldClose, endPosition);
        }

        if (tagContent.Equals("/font", StringComparison.OrdinalIgnoreCase))
        {
            return new TagInfo(TagType.FontClose, endPosition);
        }

        // Check for font tag with color attribute
        if (tagContent.StartsWith("font", StringComparison.OrdinalIgnoreCase))
        {
            var color = ParseColorFromFontTag(tagContent, defaultFontColor);
            return new TagInfo(TagType.FontOpen, endPosition, color);
        }

        return null;
    }

    private static SKColor ParseColorFromFontTag(string tagContent, SKColor defaultFontColor)
    {
        // Look for color="#ffffff" pattern
        var colorMatch = System.Text.RegularExpressions.Regex.Match(
            tagContent,
            @"color\s*=\s*[""']([^""']+)[""']",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (colorMatch.Success)
        {
            var colorValue = colorMatch.Groups[1].Value;

            // Handle hex colors
            if (colorValue.StartsWith("#") && colorValue.Length == 7)
            {
                try
                {
                    var hex = colorValue.Substring(1);
                    var r = Convert.ToByte(hex.Substring(0, 2), 16);
                    var g = Convert.ToByte(hex.Substring(2, 2), 16);
                    var b = Convert.ToByte(hex.Substring(4, 2), 16);
                    return new SKColor(r, g, b);
                }
                catch
                {
                    return defaultFontColor;
                }
            }

            // Handle named colors (basic set)
            return colorValue.ToLowerInvariant() switch
            {
                "red" => SKColors.Red,
                "green" => SKColors.Green,
                "blue" => SKColors.Blue,
                "white" => SKColors.White,
                "black" => SKColors.Black,
                "yellow" => SKColors.Yellow,
                "orange" => SKColors.Orange,
                "purple" => SKColors.Purple,
                "pink" => SKColors.Pink,
                "gray" or "grey" => SKColors.Gray,
                _ => defaultFontColor
            };
        }

        return defaultFontColor;
    }

    record TextSegment(string Text, bool IsItalic, bool IsBold, SKColor Color);

    record TextStyle(bool IsItalic = false, bool IsBold = false, SKColor Color = default)
    {
        public SKColor Color { get; init; } = Color == default ? SKColors.Black : Color;
    }

    record TagInfo(TagType TagType, int EndPosition, SKColor? Color = null);

    enum TagType
    {
        ItalicOpen,
        ItalicClose,
        BoldOpen,
        BoldClose,
        FontOpen,
        FontClose
    }

    internal void ComboChanged(object? sender, SelectionChangedEventArgs e)
    {
        _dirty = true;
    }

    internal void CheckBoxChanged(object? sender, RoutedEventArgs e)
    {
        _dirty = true;
    }

    public void OnLoaded()
    {
        _dirty = true;
        _timerUpdatePreview.Start();
    }

    internal void ColorChanged(object? sender, ColorChangedEventArgs e)
    {
        _dirty = true;
    }
}