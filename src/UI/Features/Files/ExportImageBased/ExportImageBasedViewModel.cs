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
using System.Threading;
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
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private ObservableCollection<ExportAlignmentDisplay> _alignments;
    [ObservableProperty] ExportAlignmentDisplay _selectedAlignment;
    [ObservableProperty] private ObservableCollection<ExportContentAlignmentDisplay> _contentAlignments;
    [ObservableProperty] ExportContentAlignmentDisplay _selectedContentAlignment;
    [ObservableProperty] private ObservableCollection<int> _lineSpacings;
    [ObservableProperty] int _selectedLineSpacing;
    [ObservableProperty] private ObservableCollection<string> _profiles;
    [ObservableProperty] private string? _selectedProfile;
    [ObservableProperty] private string _imageInfo;
    [ObservableProperty] private ObservableCollection<int> _paddingsLeftRight;
    [ObservableProperty] private int _selectedPaddingLeftRight;
    [ObservableProperty] private ObservableCollection<int> _paddingsTopBottom;
    [ObservableProperty] private int _selectedPaddingTopBottom;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public DataGrid SubtitleGrid { get; set; }

    private string _subtitleFileName;
    private string _videoFileName;
    private bool _dirty;
    private readonly Lock _generateLock;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly System.Timers.Timer _timerUpdatePreview;
    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;
    private readonly IWindowService _windowService;

    public ExportImageBasedViewModel(IFileHelper fileHelper, IFolderHelper folderHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;
        _windowService = windowService;

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
        Alignments = new ObservableCollection<ExportAlignmentDisplay>(ExportAlignmentDisplay.GetAlignments());
        SelectedAlignment = Alignments[0];
        ContentAlignments = new ObservableCollection<ExportContentAlignmentDisplay>(ExportContentAlignmentDisplay.GetAlignments());
        SelectedContentAlignment = ContentAlignments[0];
        LineSpacings = new ObservableCollection<int>(Enumerable.Range(-50, 151));
        SelectedLineSpacing = 0;
        SubtitleGrid = new DataGrid();
        Title = string.Empty;
        BitmapPreview = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        OutlineColor = Colors.Black;
        ProgressText = string.Empty;
        ProgressValue = 0;
        Profiles = new ObservableCollection<string>();
        ImageInfo = string.Empty;
        PaddingsLeftRight = new ObservableCollection<int> { 0, 1, 2, 3, 4, 5, 10, 15, 20, 25, 30 };
        SelectedPaddingLeftRight = 10;
        PaddingsTopBottom = new ObservableCollection<int> { 0, 1, 2, 3, 4, 5, 10, 15, 20, 25, 30 };
        SelectedPaddingTopBottom = 10;

        _subtitleFileName = string.Empty;
        _videoFileName = string.Empty;
        _generateLock = new Lock();
        _cancellationTokenSource = new CancellationTokenSource();

        _timerUpdatePreview = new System.Timers.Timer();
        _timerUpdatePreview.Interval = 250;
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = Se.Settings.File.ExportImages;
        if (settings.Profiles.Count == 0)
        {
            settings.Profiles.Add(new SeExportImagesProfile());
        }

        var profile = settings.Profiles.FirstOrDefault() ?? new SeExportImagesProfile();
        if (!string.IsNullOrEmpty(Se.Settings.File.ExportImages.LastProfileName))
        {
            var lastProfile = settings.Profiles.FirstOrDefault(p => p.ProfileName == settings.LastProfileName);
            if (lastProfile != null)
            {
                profile = lastProfile;
            }
        }

        SelectedProfile = profile.ProfileName;

        Profiles.Clear();
        Profiles.AddRange(settings.Profiles.Select(p => p.ProfileName));

        if (!string.IsNullOrEmpty(profile.FontName))
        {
            var fontFamily = FontFamilies.FirstOrDefault(ff => ff == profile.FontName);
            if (!string.IsNullOrEmpty(fontFamily))
            {
                SelectedFontFamily = fontFamily;
            }
        }

        SelectedFontSize = (int)profile.FontSize;
        SelectedResolution = Resolutions.FirstOrDefault(r => r.Width == profile.ScreenWidth);
        SelectedTopBottomMargin = profile.BottomTopMargin;
        SelectedLeftRightMargin = profile.LeftRightMargin;
        SelectedOutlineWidth = profile.OutlineWidth;
        SelectedShadowWidth = profile.ShadowWidth;
        IsBold = profile.IsBold;
        FontColor = profile.FontColor.FromHex().ToAvaloniaColor();
        ShadowColor = profile.ShadowColor.FromHex().ToAvaloniaColor();
        OutlineColor = profile.OutlineColor.FromHex().ToAvaloniaColor();
        BoxColor = profile.BackgroundColor.FromHex().ToAvaloniaColor();
        SelectedBoxCornerRadius = profile.BackgroundCornerRadius;
    }

    private void SaveSettings()
    {
        var profile = Se.Settings.File.ExportImages.Profiles.FirstOrDefault() ?? new SeExportImagesProfile();

        profile.FontName = SelectedFontFamily ?? FontFamilies.First();
        profile.FontSize = SelectedFontSize ?? 26;
        profile.ScreenWidth = SelectedResolution?.Width ?? 1920;
        profile.ScreenHeight = SelectedResolution?.Height ?? 1080;
        profile.BottomTopMargin = SelectedTopBottomMargin ?? 10;
        profile.LeftRightMargin = SelectedLeftRightMargin ?? 10;
        profile.OutlineWidth = SelectedOutlineWidth ?? 3;
        profile.ShadowWidth = SelectedShadowWidth;
        profile.IsBold = IsBold;
        profile.FontColor = FontColor.ToSKColor().ToHex(true);
        profile.ShadowColor = ShadowColor.ToSKColor().ToHex(true);
        profile.OutlineColor = OutlineColor.ToSKColor().ToHex(true);
        profile.BackgroundColor = BoxColor.ToSKColor().ToHex(true);
        profile.BackgroundCornerRadius = SelectedBoxCornerRadius;

        Se.SaveSettings();
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
    private void Cancel()
    {
        if (IsGenerating)
        {
            _cancellationTokenSource.Cancel();
            IsGenerating = false;
            return;
        }

        Close();
    }

    [RelayCommand]
    private async Task ShowProfile()
    {
        //   SaveProfiles();

        var result = await _windowService.ShowDialogAsync<ImageBasedProfileWindow, ImageBasedProfileViewModel>(Window!,
            vm =>
            {
                vm.Initialize(Profiles, SelectedProfile);
            });

        if (result.OkPressed)
        {
            //LoadProfiles();
            // var profile = Profiles.FirstOrDefault(p => p.ProfileName == result.SelectedProfile?.Name);
            // SelectedProfile = profile ?? Profiles.FirstOrDefault();
        }
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

        IsGenerating = true;
        var imageParameters = new List<ImageParameter>();
        for (var i = 0; i < Subtitles.Count; i++)
        {
            var imageParameter = GetImageParameter(i);
            imageParameters.Add(imageParameter);
        }

        int total = Subtitles.Count;
        int completed = 0;
        var progressLock = new object();
        await Task.Run(() =>
        {
            Parallel.For(0, total, i =>
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }

                var ip = imageParameters[i];
                ip.Bitmap = GenerateBitmap(ip);
                exportImageHandler.CreateParagraph(ip);

                lock (_generateLock)
                {
                    completed++;
                    var percent = completed * 100.0 / total;

                    Dispatcher.UIThread.Post(() =>
                    {
                        ProgressValue = percent;
                        ProgressText = string.Format(Se.Language.General.GeneratingImageXofY, completed, total);
                    });
                }
            });

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            ProgressValue = 100;
            ProgressText = Se.Language.General.SavingDotDotDot;

            exportImageHandler.WriteHeader(fileOrFolderName, SelectedResolution?.Width ?? 1920, SelectedResolution?.Height ?? 1080);
            for (var i = 0; i < Subtitles.Count; i++)
            {
                var ip = imageParameters[i];
                exportImageHandler.WriteParagraph(ip);
            }
            exportImageHandler.WriteFooter();
            IsGenerating = false;
        });
    }

    private ImageParameter GetImageParameter(int i)
    {
        SubtitleLineViewModel? subtitle = Subtitles[i];
        var imageParameter = new ImageParameter
        {
            Alignment = ExportAlignment.BottomCenter,
            ContentAlignment = SelectedContentAlignment.ContentAlignment,
            PaddingLeftRight = SelectedPaddingLeftRight,
            PaddingTopBottom = SelectedPaddingTopBottom,
            Index = i,
            Text = subtitle.Text,
            StartTime = subtitle.StartTime,
            EndTime = subtitle.EndTime,
            FontColor = FontColor.ToSKColor(),
            FontName = SelectedFontFamily ?? FontFamilies.First(),
            FontSize = SelectedFontSize ?? 26,
            IsBold = IsBold,
            LineSpacingPercent = SelectedLineSpacing,
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

        return imageParameter;
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
            BitmapPreview = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
            return;
        }

        var idx = Subtitles.IndexOf(selected);
        var ip = GetImageParameter(idx);
        BitmapPreview = GenerateBitmap(ip).ToAvaloniaBitmap();
        ImageInfo = $"{BitmapPreview.Size.Width}x{BitmapPreview.Size.Height}";
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

        // IMPROVED: Measure actual text bounds instead of using font metrics
        float maxWidth = 0;
        float minY = float.MaxValue; // Highest point of any text
        float maxY = float.MinValue; // Lowest point of any text

        // Calculate line spacing once
        var baseLineHeight = Math.Abs(fontMetrics.Ascent) + Math.Abs(fontMetrics.Descent);
        var lineSpacing = (float)(baseLineHeight * ip.LineSpacingPercent / 100.0) - baseLineHeight;

        float currentY = 0; // Start measuring from 0

        foreach (var line in lines)
        {
            float lineWidth = 0;
            float lineMinY = float.MaxValue;
            float lineMaxY = float.MinValue;

            foreach (var segment in line)
            {
                var currentFont = GetFont(segment, regularFont, boldFont, italicFont, boldItalicFont);
                lineWidth += currentFont.MeasureText(segment.Text);

                // Get actual text bounds for this segment using paint
                using var measurePaint = new SKPaint();
                measurePaint.Typeface = currentFont.Typeface;
                measurePaint.TextSize = currentFont.Size;

                var textBounds = new SKRect();
                measurePaint.MeasureText(segment.Text, ref textBounds);

                // Adjust bounds to current Y position
                var segmentMinY = currentY + textBounds.Top;
                var segmentMaxY = currentY + textBounds.Bottom;

                lineMinY = Math.Min(lineMinY, segmentMinY);
                lineMaxY = Math.Max(lineMaxY, segmentMaxY);
            }

            maxWidth = Math.Max(maxWidth, lineWidth);
            minY = Math.Min(minY, lineMinY);
            maxY = Math.Max(maxY, lineMaxY);

            // Move to next line position
            currentY += baseLineHeight + lineSpacing;
        }

        // Calculate actual text height from measured bounds
        var actualTextHeight = lines.Count > 0 ? maxY - minY : 0;

        // IMPROVED: Calculate precise content area and effects padding
        var effectsPadding = (float)Math.Max(outlineWidth, shadowWidth);
        var paddingLeftRight = (float)ip.PaddingLeftRight;
        var paddingTopBottom = (float)ip.PaddingTopBottom;

        // Content area (text + specified padding)
        var contentWidth = maxWidth + (paddingLeftRight * 2);
        var contentHeight = actualTextHeight + (paddingTopBottom * 2);

        // Total bitmap size (content + effects padding)
        var width = (int)Math.Ceiling(contentWidth + (effectsPadding * 2));
        var height = (int)Math.Ceiling(contentHeight + (effectsPadding * 2));

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

        // IMPROVED: Define the rounded rectangle with proper bounds
        // The background box should cover the content area, positioned to account for effects
        var boxRect = new SKRect(
            effectsPadding,
            effectsPadding,
            width - effectsPadding,
            height - effectsPadding
        );
        float cornerRadius = (float)SelectedBoxCornerRadius;

        // Draw the rounded rectangle
        canvas.DrawRoundRect(boxRect, cornerRadius, cornerRadius, paint);

        // IMPROVED: Calculate precise text positioning
        // Start position accounts for effects padding + specified padding
        var textStartX = effectsPadding + paddingLeftRight;
        // Position text baseline to account for the measured minY offset
        var textStartY = effectsPadding + paddingTopBottom - minY;

        currentY = 0; // Reset for actual drawing

        foreach (var line in lines)
        {
            float currentX = textStartX;

            // Calculate line width for alignment
            var lineWidth = line.Sum(segment => GetFont(segment, regularFont, boldFont, italicFont, boldItalicFont).MeasureText(segment.Text));

            // Adjust X position based on content alignment
            if (ip.ContentAlignment == ExportContentAlignment.Center)
            {
                // Center within the content area (not the entire bitmap)
                var contentAreaWidth = contentWidth - (paddingLeftRight * 2);
                currentX += (contentAreaWidth - lineWidth) / 2;
            }
            else if (ip.ContentAlignment == ExportContentAlignment.Right)
            {
                // Align right within the content area
                var contentAreaWidth = contentWidth - (paddingLeftRight * 2);
                currentX += contentAreaWidth - lineWidth;
            }

            for (int i = 0; i < line.Count; i++)
            {
                var segment = line[i];
                var currentFont = GetFont(segment, regularFont, boldFont, italicFont, boldItalicFont);

                // Draw shadow first (if shadow width > 0)
                if (shadowWidth > 0)
                {
                    var shadowOffsetX = currentX + (float)shadowWidth;
                    var shadowOffsetY = textStartY + currentY + (float)shadowWidth;

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

                    canvas.DrawText(segment.Text, currentX, textStartY + currentY, currentFont, outlinePaint);
                }

                // Draw the main text on top
                using var textPaint = new SKPaint
                {
                    Color = segment.Color,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };

                canvas.DrawText(segment.Text, currentX, textStartY + currentY, currentFont, textPaint);
                currentX += currentFont.MeasureText(segment.Text);

                // Add small spacing after styled segments to prevent crowding
                if ((segment.IsItalic || segment.IsBold) && i < line.Count - 1)
                {
                    currentX += fontSize * 0.17f;
                }
            }

            // Move to next line
            currentY += baseLineHeight + lineSpacing;
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

    internal void OnClosing()
    {
        SaveSettings();
    }
}