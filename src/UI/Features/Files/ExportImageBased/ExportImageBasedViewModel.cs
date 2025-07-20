using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    [ObservableProperty] private ObservableCollection<string> _borderStyles;
    [ObservableProperty] string? _selectedBorderStyle;
    [ObservableProperty] private ObservableCollection<int> _shadowWidths;
    [ObservableProperty] int? _selectedShadowWidth;
    [ObservableProperty] private bool _isBold;
    [ObservableProperty] private Color _fontColor;
    [ObservableProperty] private Color _borderColor;
    [ObservableProperty] private Color _shadowColor;
    [ObservableProperty] private TimeSpan _startOfProgramme;
    [ObservableProperty] private Bitmap _bitmapPreview;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public DataGrid SubtitleGrid { get; set; }

    private string _subtitleFileName;
    private string _videoFileName;
    private bool _dirty;
    private readonly Timer _timerUpdatePreview;

    public ExportImageBasedViewModel()
    {
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
        BorderStyles = new ObservableCollection<string> { "None", "Solid", "Dashed", "Dotted" };
        SelectedBorderStyle = BorderStyles.FirstOrDefault();
        ShadowWidths = new ObservableCollection<int> { 0, 1, 2, 3, 4, 5 };
        SelectedShadowWidth = 3;
        FontColor = Colors.White;
        BorderColor = Colors.Black;
        ShadowColor = Colors.Black;
        SubtitleGrid = new DataGrid();
        Title = string.Empty;
        BitmapPreview = new SKBitmap(1, 1).ToAvaloniaBitmap();

        _subtitleFileName = string.Empty;
        _videoFileName = string.Empty;

        _timerUpdatePreview = new Timer();
        _timerUpdatePreview.Interval = 250;
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
        _timerUpdatePreview.Start();
    }

    private void TimerUpdatePreviewElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_dirty)
        {
            _dirty = false;
            Dispatcher.UIThread.Post(() => { SubtitleLineChanged(); });
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
    private void Export()
    {
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

        var fontName = SelectedFontFamily ?? "Arial";
        var fontSize = SelectedFontSize ?? 20;

        // Parse text and create text segments with styling
        var segments = ParseTextWithItalics(text);

        // Create fonts
        using var regularTypeface = SKTypeface.FromFamilyName(fontName, SKFontStyle.Normal);
        using var italicTypeface = SKTypeface.FromFamilyName(fontName, SKFontStyle.Italic);
        using var regularFont = new SKFont(regularTypeface, fontSize);
        using var italicFont = new SKFont(italicTypeface, fontSize);
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        // Measure total text dimensions
        float totalWidth = 0;
        float maxHeight = 0;
        var fontMetrics = regularFont.Metrics;
        var textHeight = fontMetrics.Descent - fontMetrics.Ascent;
        maxHeight = Math.Max(maxHeight, textHeight);

        foreach (var segment in segments)
        {
            var currentFont = segment.IsItalic ? italicFont : regularFont;
            totalWidth += currentFont.MeasureText(segment.Text);
        }

        // Calculate bitmap dimensions with padding
        var padding = 10;
        var width = (int)Math.Ceiling(totalWidth) + padding * 2;
        var height = (int)Math.Ceiling(maxHeight) + padding * 2;

        // Ensure minimum size
        width = Math.Max(width, 1);
        height = Math.Max(height, 1);

        // Create bitmap and canvas
        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        // Clear background
        canvas.Clear(SKColors.White);

        // Draw text segments
        float currentX = padding;
        var baselineY = padding - fontMetrics.Ascent;

        for (int i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            var currentFont = segment.IsItalic ? italicFont : regularFont;
            canvas.DrawText(segment.Text, currentX, baselineY, currentFont, paint);
            currentX += currentFont.MeasureText(segment.Text);

            // Add small spacing after italic segments to prevent crowding
            if (segment.IsItalic && i < segments.Count - 1)
            {
                currentX += fontSize * 0.15f; // Add 15% of font size as spacing
            }
        }

        BitmapPreview = bitmap.ToAvaloniaBitmap();
    }

    static List<TextSegment> ParseTextWithItalics(string text)
    {
        var segments = new List<TextSegment>();
        var currentPos = 0;
        var isItalic = false;

        while (currentPos < text.Length)
        {
            var italicStart = text.IndexOf("<i>", currentPos);
            var italicEnd = text.IndexOf("</i>", currentPos);

            if (italicStart == -1 && italicEnd == -1)
            {
                // No more tags, add remaining text
                if (currentPos < text.Length)
                {
                    var remainingText = text.Substring(currentPos);
                    if (!string.IsNullOrEmpty(remainingText))
                    {
                        segments.Add(new TextSegment(remainingText, isItalic));
                    }
                }
                break;
            }

            if (italicStart != -1 && (italicEnd == -1 || italicStart < italicEnd))
            {
                // Found opening tag
                if (italicStart > currentPos)
                {
                    var beforeItalic = text.Substring(currentPos, italicStart - currentPos);
                    if (!string.IsNullOrEmpty(beforeItalic))
                    {
                        segments.Add(new TextSegment(beforeItalic, isItalic));
                    }
                }
                isItalic = true;
                currentPos = italicStart + 3; // Skip "<i>"
            }
            else if (italicEnd != -1)
            {
                // Found closing tag
                if (italicEnd > currentPos)
                {
                    var italicText = text.Substring(currentPos, italicEnd - currentPos);
                    if (!string.IsNullOrEmpty(italicText))
                    {
                        segments.Add(new TextSegment(italicText, isItalic));
                    }
                }
                isItalic = false;
                currentPos = italicEnd + 4; // Skip "</i>"
            }
        }

        // Filter out empty segments
        return segments.Where(s => !string.IsNullOrEmpty(s.Text)).ToList();
    }

    record TextSegment(string Text, bool IsItalic);

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
    }
}