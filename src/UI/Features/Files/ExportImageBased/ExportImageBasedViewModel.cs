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
using Nikse.SubtitleEdit.Features.Shared;
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
    [ObservableProperty] int _selectedFontSize;
    [ObservableProperty] private ObservableCollection<ResolutionItem> _resolutions;
    [ObservableProperty] ResolutionItem? _selectedResolution;
    [ObservableProperty] private ObservableCollection<int> _topBottomMargins;
    [ObservableProperty] int _selectedTopBottomMargin;
    [ObservableProperty] private ObservableCollection<int> _leftRightMargins;
    [ObservableProperty] int _selectedLeftRightMargin;
    [ObservableProperty] private ObservableCollection<double> _outlineWidths;
    [ObservableProperty] double _selectedOutlineWidth;
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
    [ObservableProperty] private ObservableCollection<SeExportImagesProfile> _profiles;
    [ObservableProperty] private SeExportImagesProfile? _selectedProfile;
    [ObservableProperty] private string _imageInfo;
    [ObservableProperty] private ObservableCollection<int> _paddingsLeftRight;
    [ObservableProperty] private int _selectedPaddingLeftRight;
    [ObservableProperty] private ObservableCollection<int> _paddingsTopBottom;
    [ObservableProperty] private int _selectedPaddingTopBottom;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public DataGrid SubtitleGrid { get; set; }

    private List<SubtitleLineViewModel>? _selectedSubtitles;
    private string _subtitleFileName;
    private string _videoFileName;
    private bool _dirty;
    private bool _subtitleGridSelectionChangedSkip;
    private readonly Lock _generateLock;
    private bool _isCtrlDown;
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
        SelectedFontSize = 26;
        Resolutions = new ObservableCollection<ResolutionItem>(ResolutionItem.GetResolutions());
        SelectedResolution = Resolutions.FirstOrDefault(r => r.Width == 1920);
        TopBottomMargins = new ObservableCollection<int>(Enumerable.Range(0, 101));
        SelectedTopBottomMargin = 10;
        LeftRightMargins = new ObservableCollection<int>(Enumerable.Range(0, 101));
        SelectedLeftRightMargin = 10;
        OutlineWidths = new ObservableCollection<double>(Enumerable.Range(0, 16).Select(i => (double)i));
        SelectedOutlineWidth = OutlineWidths.First();
        ShadowWidths = new ObservableCollection<double>(Enumerable.Range(0, 16).Select(i => (double)i));
        BoxCornerRadiusList = new ObservableCollection<double>(Enumerable.Range(0, 101).Select(i => (double)i));
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
        Profiles = new ObservableCollection<SeExportImagesProfile>();
        ImageInfo = string.Empty;
        PaddingsLeftRight = new ObservableCollection<int>(Enumerable.Range(0, 101));
        SelectedPaddingLeftRight = 10;
        PaddingsTopBottom = new ObservableCollection<int>(Enumerable.Range(0, 101));
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

        SelectedProfile = profile;

        Profiles.Clear();
        Profiles.AddRange(settings.Profiles);

        LoadProfile(profile);
    }

    private void SaveSettings()
    {
        SaveProfile(SelectedProfile);
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

    private void SaveProfiles()
    {
        Se.Settings.File.ExportImages.Profiles.Clear();
        foreach (var profile in Profiles)
        {
            Se.Settings.File.ExportImages.Profiles.Add(profile);
        }

        Se.Settings.File.ExportImages.LastProfileName = SelectedProfile?.ProfileName ?? "Default";
    }

    private void LoadProfiles()
    {
        Profiles.Clear();
        var profiles = Se.Settings.File.ExportImages.Profiles;
        if (profiles.Count == 0)
        {
            profiles.Add(new SeExportImagesProfile());
        }
        foreach (var profile in profiles)
        {
            Profiles.Add(profile);
        }
    }

    [RelayCommand]
    private async Task ShowProfile()
    {
        SaveProfiles();

        var result = await _windowService.ShowDialogAsync<ImageBasedProfileWindow, ImageBasedProfileViewModel>(Window!, vm =>
        {
            vm.Initialize(Profiles, SelectedProfile);
        });

        if (result.OkPressed)
        {
            LoadProfiles();
            var profile = Profiles.FirstOrDefault(p => p.ProfileName == result.SelectedProfile?.Name);
            SelectedProfile = profile ?? Profiles.FirstOrDefault();
        }
    }

    [RelayCommand]
    private void ToggleLinesItalic()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? new List<SubtitleLineViewModel>();
        if (selectedItems.Any())
        {
            foreach (var item in selectedItems)
            {
                item.Text = item.Text.Contains("<i>")
                    ? item.Text.Replace("<i>", "").Replace("</i>", "")
                    : $"<i>{item.Text}</i>";
            }
        }

        _dirty = true;
    }

    [RelayCommand]
    private void ToggleLinesBold()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? new List<SubtitleLineViewModel>();
        if (selectedItems.Any())
        {
            foreach (var item in selectedItems)
            {
                item.Text = item.Text.Contains("<b>")
                    ? item.Text.Replace("<b>", "").Replace("</b>", "")
                    : $"<b>{item.Text}</b>";
            }
        }

        _dirty = true;
    }

    [RelayCommand]
    private async Task DeleteSelectedLines()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? new List<SubtitleLineViewModel>();
        if (selectedItems == null || !selectedItems.Any())
        {
            return;
        }

        if (Se.Settings.General.PromptDeleteLines)
        {
            var answer = await MessageBox.Show(
                Window!,
                "Delete lines?",
                $"Do you want to delete {selectedItems.Count} lines?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        foreach (var item in selectedItems)
        {
            Subtitles.Remove(item);
        }
        Renumber();

        _dirty = true;
    }

    private void Renumber()
    {
        for (var index = 0; index < Subtitles.Count; index++)
        {
            Subtitles[index].Number = index + 1;
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

    public void SubtitleGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_subtitleGridSelectionChangedSkip)
        {
            return;
        }

        SubtitleGridSelectionChanged();
        SubtitleLineChanged();
    }

    private void SubtitleGridSelectionChanged()
    {
        var selectedItems = SubtitleGrid.SelectedItems;

        if (selectedItems == null)
        {
            SelectedSubtitle = null;
            _selectedSubtitles = null;
            return;
        }

        _selectedSubtitles = selectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count > 1)
        {
            return;
        }

        var item = _selectedSubtitles.FirstOrDefault();
        if (item == null)
        {
            SelectedSubtitle = null;
            return;
        }

        SelectedSubtitle = item;
    }

    private ImageParameter GetImageParameter(int i)
    {
        var subtitle = Subtitles[i];
        var imageParameter = new ImageParameter
        {
            Alignment = SelectedAlignment.Alignment,
            ContentAlignment = SelectedContentAlignment.ContentAlignment,
            PaddingLeftRight = SelectedPaddingLeftRight,
            PaddingTopBottom = SelectedPaddingTopBottom,
            Index = i,
            Text = subtitle.Text,
            StartTime = subtitle.StartTime,
            EndTime = subtitle.EndTime,
            FontColor = FontColor.ToSKColor(),
            FontName = SelectedFontFamily ?? FontFamilies.First(),
            FontSize = SelectedFontSize,
            IsBold = IsBold,
            LineSpacingPercent = SelectedLineSpacing,
            OutlineColor = OutlineColor.ToSKColor(),
            OutlineWidth = SelectedOutlineWidth,
            ShadowColor = ShadowColor.ToSKColor(),
            ShadowWidth = SelectedShadowWidth,
            BackgroundColor = BoxColor.ToSKColor(),
            BackgroundCornerRadius = SelectedBoxCornerRadius,
            ScreenWidth = SelectedResolution?.Width ?? 1920,
            ScreenHeight = SelectedResolution?.Height ?? 1080,
            BottomTopMargin = SelectedTopBottomMargin,
            LeftRightMargin = SelectedLeftRightMargin,
        };

        return imageParameter;
    }

    [RelayCommand]
    private async Task SavePreview()
    {
        if (BitmapPreview == null || SelectedSubtitle == null)
        {
            return;
        }

        var imageIndex = Subtitles.IndexOf(SelectedSubtitle!);
        var fileName = await _fileHelper.PickSaveSubtitleFile(Window!, ".png", $"image{imageIndex}", Se.Language.General.SaveImageAs);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        BitmapPreview.Save(fileName, 100);
    }


    [RelayCommand]
    private async Task ShowPreview()
    {
        if (BitmapPreview == null || SelectedSubtitle == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<ImageBasedPreviewWindow, ImageBasedPreviewViewModel>(Window!, vm =>
        {
            var ip = GetImageParameter(Subtitles.IndexOf(SelectedSubtitle!));
            var bitmap = GenerateBitmap(ip);
            var position = CalculatePosition(ip, bitmap.Width, bitmap.Height);
            vm.Initialize(bitmap, ip.ScreenWidth, ip.ScreenHeight, position.X, position.Y);
        });
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
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

        if (!string.IsNullOrEmpty(videoFileName))
        {
            _ = Task.Run(() =>
            {
                var mediaInfo = FfmpegMediaInfo.Parse(videoFileName);
                if (mediaInfo?.Dimension is { Width: > 0, Height: > 0 })
                {
                    var resolutionItem = new ResolutionItem(string.Empty, mediaInfo.Dimension.Width, mediaInfo.Dimension.Height);

                    Dispatcher.UIThread.Post(() =>
                    {
                        Resolutions.Insert(1, resolutionItem);
                        SelectedResolution = resolutionItem;
                        _dirty = true;
                    });
                }
            });
        }
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
        var position = CalculatePosition(ip, BitmapPreview.Size.Width, BitmapPreview.Size.Height);
        ImageInfo = $"{BitmapPreview.Size.Width}x{BitmapPreview.Size.Height} @ {position.X},{position.Y}";
    }

    private static SKPointI CalculatePosition(ImageParameter ip, double width, double height)
    {
        var x = 0;
        var y = 0;

        if (ip.Alignment == ExportAlignment.TopLeft ||
            ip.Alignment == ExportAlignment.MiddleLeft ||
            ip.Alignment == ExportAlignment.BottomLeft)
        {
            x = (int)ip.LeftRightMargin;
        }
        else if (ip.Alignment == ExportAlignment.TopCenter ||
                 ip.Alignment == ExportAlignment.MiddleCenter ||
                 ip.Alignment == ExportAlignment.BottomCenter)
        {
            x = (int)((ip.ScreenWidth - width) / 2);
        }
        else if (ip.Alignment == ExportAlignment.TopRight ||
                 ip.Alignment == ExportAlignment.MiddleRight ||
                 ip.Alignment == ExportAlignment.BottomRight)
        {
            x = (int)(ip.ScreenWidth - width - ip.LeftRightMargin);
        }

        if (ip.Alignment == ExportAlignment.TopLeft ||
            ip.Alignment == ExportAlignment.TopCenter ||
            ip.Alignment == ExportAlignment.TopRight)
        {
            y = (int)ip.BottomTopMargin;
        }
        else if (ip.Alignment == ExportAlignment.MiddleLeft ||
                 ip.Alignment == ExportAlignment.MiddleCenter ||
                 ip.Alignment == ExportAlignment.MiddleRight)
        {
            y = (int)((ip.ScreenHeight - height) / 2);
        }
        else if (ip.Alignment == ExportAlignment.BottomLeft ||
                 ip.Alignment == ExportAlignment.BottomCenter ||
                 ip.Alignment == ExportAlignment.BottomRight)
        {
            y = (int)(ip.ScreenHeight - height - ip.BottomTopMargin);
        }

        return new SKPointI(x, y);
    }

    private SKBitmap GenerateBitmap(ImageParameter ip)
    {
        var fontName = ip.FontName;
        var fontSize = ip.FontSize;
        var fontColor = ip.FontColor;

        var outlineColor = OutlineColor.ToSKColor();
        var outlineWidth = SelectedOutlineWidth;

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
        var lineSpacing = (float)(baseLineHeight * ip.LineSpacingPercent / 100.0);

        float currentY = 0; // Start measuring from 0

        foreach (var line in lines)
        {
            float lineWidth = 0;
            float lineMinY = float.MaxValue;
            float lineMaxY = float.MinValue;

            foreach (var segment in line)
            {
                var currentFont = GetFont(segment, regularFont, boldFont, italicFont, boldItalicFont);

                // Use SKFont.MeasureText for width measurement
                lineWidth += currentFont.MeasureText(segment.Text);

                // Get actual text bounds for this segment
                var textBounds = new SKRect();
                currentFont.MeasureText(segment.Text, out textBounds);

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
    
    public void ComboResolutionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _dirty = true;
        var res = SelectedResolution;
        if (res == null)
        {
            return;
        }

        _dirty = true;
        if (res is { Width: > 0, Height: > 0 })
        {
            return;
        }

        Dispatcher.UIThread.Post(async void () =>
        {
            try
            {
                var videoFileName = await _fileHelper.PickOpenVideoFile(Window!, Se.Language.General.OpenVideoFileTitle);
                if (string.IsNullOrWhiteSpace(videoFileName))
                {
                    if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is ResolutionItem item)
                    {
                        SelectedResolution = item;
                    }
                    else
                    {
                        SelectedResolution = Resolutions.FirstOrDefault(p => p.Width == 1920);
                    }

                    return;
                }

                _ = Task.Run(() =>
                {
                    var mediaInfo = FfmpegMediaInfo.Parse(videoFileName);
                    if (mediaInfo?.Dimension is { Width: > 0, Height: > 0 })
                    {
                        var resolutionItem = new ResolutionItem(string.Empty, mediaInfo.Dimension.Width, mediaInfo.Dimension.Height);

                        Dispatcher.UIThread.Post(() =>
                        {
                            Resolutions.Insert(1, resolutionItem);
                            SelectedResolution = resolutionItem;
                            _dirty = true;
                        });
                    }
                });
            }
            catch (Exception exception)
            {
                Se.LogError(exception);
            }
        });
    }

    internal void OnClosing()
    {
        SaveSettings();
    }

    internal void ProfileChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.RemovedItems.Count == 1)
        {
            SaveProfile(e.RemovedItems[0]);
        }

        if (e.AddedItems.Count == 1)
        {
            LoadProfile(e.AddedItems[0]);
        }
    }

    private void LoadProfile(object? v)
    {
        if (v is SeExportImagesProfile profile)
        {
            SelectedFontSize = (int)profile.FontSize;
            SelectedResolution = Resolutions.FirstOrDefault(r => r.Width == profile.ScreenWidth);
            SelectedTopBottomMargin = profile.BottomTopMargin;
            SelectedLeftRightMargin = profile.LeftRightMargin;
            SelectedOutlineWidth = profile.OutlineWidth;
            SelectedShadowWidth = profile.ShadowWidth;
            SelectedAlignment = Alignments.FirstOrDefault(p=>p.Alignment == profile.Alignment) ?? Alignments[0];
            SelectedContentAlignment = ContentAlignments.FirstOrDefault(p => p.ContentAlignment == profile.ContentAlignment) ?? ContentAlignments[0];
            IsBold = profile.IsBold;
            SelectedFontFamily = profile.FontName;
            FontColor = profile.FontColor.FromHex().ToAvaloniaColor();
            OutlineColor = profile.OutlineColor.FromHex().ToAvaloniaColor();
            ShadowColor = profile.ShadowColor.FromHex().ToAvaloniaColor();
            BoxColor = profile.BackgroundColor.FromHex().ToAvaloniaColor();
            SelectedBoxCornerRadius = profile.BackgroundCornerRadius;
            SelectedPaddingLeftRight = profile.PaddingLeftRight;
            SelectedPaddingTopBottom = profile.PaddingTopBottom;
            SelectedLineSpacing = profile.LineSpacingPercent;
        }
    }

    private void SaveProfile(object? v)
    {
        if (v is SeExportImagesProfile profile)
        {
            profile.FontSize = SelectedFontSize;
            profile.ScreenWidth = SelectedResolution?.Width ?? 1920;
            profile.ScreenHeight = SelectedResolution?.Height ?? 1080;
            profile.BottomTopMargin = SelectedTopBottomMargin;
            profile.LeftRightMargin = SelectedLeftRightMargin;
            profile.OutlineWidth = SelectedOutlineWidth;
            profile.ShadowWidth = SelectedShadowWidth;
            profile.Alignment = SelectedAlignment.Alignment;
            profile.ContentAlignment = SelectedContentAlignment.ContentAlignment;
            profile.IsBold = IsBold;
            profile.FontName = SelectedFontFamily ?? string.Empty;
            profile.FontColor = FontColor.FromColorToHex(true);
            profile.OutlineColor = OutlineColor.FromColorToHex(true);
            profile.ShadowColor = ShadowColor.FromColorToHex(true);
            profile.BackgroundColor = BoxColor.FromColorToHex(true);
            profile.BackgroundCornerRadius = SelectedBoxCornerRadius;
            profile.PaddingLeftRight = SelectedPaddingLeftRight;
            profile.PaddingTopBottom = SelectedPaddingTopBottom;
            profile.LineSpacingPercent = SelectedLineSpacing;   
        }
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
        else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isCtrlDown = true;
        }
        else if (e.Key == Key.I && _isCtrlDown)
        {
            ToggleLinesItalic();
        }
        else if (e.Key == Key.B && _isCtrlDown)
        {
            ToggleLinesBold();
        }
        else if (e.Key == Key.P && _isCtrlDown)
        {
            Dispatcher.UIThread.Post(async void () =>
            {
                try
                {
                    await ShowPreview();
                }
                catch (Exception exception)
                {
                    Se.LogError(exception);
                }
            });
        }
    }

    internal void OnKeyUp(KeyEventArgs e)
    { 
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isCtrlDown = false;
            e.Handled = true;
        }
    }
}