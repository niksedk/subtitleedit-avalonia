using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustAlpha;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa.AssaSetPosition;

public partial class AssaSetPositionViewModel : ObservableObject
{
    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }
    public Image? ScreenshotImage { get; set; }
    public Image? ScreenshotOverlayImage { get; set; }
    public Grid? VideoGrid { get; set; }

    [ObservableProperty] private int _sourceWidth = 1920;
    [ObservableProperty] private int _sourceHeight = 1080;
    [ObservableProperty] private int _targetWidth = 1920;
    [ObservableProperty] private int _targetHeight = 1080;
    [ObservableProperty] private int _screenshotX;
    [ObservableProperty] private int _screenshotY;
    [ObservableProperty] private Bitmap _screenshotOverlayText;
    [ObservableProperty] private Bitmap _screenshot;
    [ObservableProperty] private string _screenshotOverlayPosiion;

    private Subtitle _subtitle = new();

    public Subtitle ResultSubtitle => _subtitle;

    public AssaSetPositionViewModel()
    {
        Screenshot = new SKBitmap(1, 1).ToAvaloniaBitmap();
        ScreenshotOverlayText = new SKBitmap(1, 1).ToAvaloniaBitmap();
        ScreenshotOverlayPosiion = string.Empty;
    }

    partial void OnScreenshotXChanged(int value)
    {
        // Only update if UI elements are initialized
        if (VideoGrid != null && ScreenshotOverlayImage != null && ScreenshotImage != null)
        {
            UpdateOverlayPosition();
        }
    }

    partial void OnScreenshotYChanged(int value)
    {
        // Only update if UI elements are initialized
        if (VideoGrid != null && ScreenshotOverlayImage != null && ScreenshotImage != null)
        {
            UpdateOverlayPosition();
        }
    }

    public void Initialize(Subtitle subtitle, SubtitleLineViewModel line, string? videoFileName, int? videoWidth, int? videoHeight)
    {
        _subtitle = new Subtitle(subtitle, false);

        ScreenshotOverlayText = CreateTextImage(subtitle, line);
        //var byes = ScreenshotOverlayText.ToSkBitmap().ToPngArray();
        //System.IO.File.WriteAllBytes(@"C:\temp\overlay.png", byes);

        if (string.IsNullOrEmpty(_subtitle.Header))
        {
            _subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
        }

        // Get source resolution from subtitle header
        var oldPlayResX = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", _subtitle.Header);
        if (int.TryParse(oldPlayResX, out var w) && w > 0)
        {
            SourceWidth = w;
        }

        var oldPlayResY = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", _subtitle.Header);
        if (int.TryParse(oldPlayResY, out var h) && h > 0)
        {
            SourceHeight = h;
        }

        // Set target resolution from video if available
        if (videoWidth.HasValue && videoWidth.Value > 0)
        {
            TargetWidth = videoWidth.Value;
        }
        else
        {
            TargetWidth = SourceWidth;
        }

        if (videoHeight.HasValue && videoHeight.Value > 0)
        {
            TargetHeight = videoHeight.Value;
        }
        else
        {
            TargetHeight = SourceHeight;
        }

        if (TargetWidth <= 0 || TargetHeight <= 0)   
        {
            TargetWidth = 1820;
            TargetHeight = 1080;
        }

        // Initialize position (center horizontally, bottom vertically with some margin)
        if (ScreenshotOverlayText != null)
        {
            ScreenshotX = (int)((TargetWidth - ScreenshotOverlayText.Size.Width) / 2);
            ScreenshotY = (int)(TargetHeight - ScreenshotOverlayText.Size.Height - 50); // 50px margin from bottom
        }

        if (string.IsNullOrEmpty(videoFileName))
        {
            Screenshot = BinaryAdjustAlphaViewModel.CreateCheckeredBackground(TargetHeight, TargetHeight);
            return;
        }

        var fileName = FfmpegGenerator.GetScreenShot(videoFileName, new TimeCode(line.StartTime.TotalMilliseconds).ToDisplayString());
        if (System.IO.File.Exists(fileName))
        {
            try
            {
                Screenshot = new Bitmap(fileName);
            }
            catch
            {
                Screenshot = BinaryAdjustAlphaViewModel.CreateCheckeredBackground(TargetHeight, TargetHeight);
            }
        }
        else
        {
            Screenshot = BinaryAdjustAlphaViewModel.CreateCheckeredBackground(TargetHeight, TargetHeight);
        }

    }

    private Bitmap CreateTextImage(Subtitle subtitle, SubtitleLineViewModel line)
    {
        // Get the style for this line
        var styleName = line.Style;
        if (string.IsNullOrEmpty(styleName))
        {
            styleName = "Default";
        }

        var style = AdvancedSubStationAlpha.GetSsaStyle(styleName, subtitle.Header);
        if (style == null)
        {
            style = new SsaStyle();
        }

        // Remove ASSA tags for simple rendering
        var text = Utilities.RemoveSsaTags(line.Text);
        if (string.IsNullOrEmpty(text))
        {
            return new SKBitmap(1, 1).ToAvaloniaBitmap();
        }

        // Create font
        var fontStyle = SKFontStyle.Normal;
        if (style.Bold && style.Italic)
        {
            fontStyle = SKFontStyle.BoldItalic;
        }
        else if (style.Bold)
        {
            fontStyle = SKFontStyle.Bold;
        }
        else if (style.Italic)
        {
            fontStyle = SKFontStyle.Italic;
        }

        using var typeface = SKTypeface.FromFamilyName(style.FontName, fontStyle);
        using var font = new SKFont(typeface, (float)(style.FontSize * 1.3m));
        
        // Measure text
        var textBounds = new SKRect();
        font.MeasureText(text, out textBounds);
        
        // Calculate dimensions with outline and shadow
        var outlineWidth = (float)style.OutlineWidth;
        var shadowWidth = (float)style.ShadowWidth;
        var padding = 20f;
        
        var width = (int)(textBounds.Width + padding * 2 + outlineWidth * 2 + shadowWidth);
        var height = (int)(textBounds.Height + padding * 2 + outlineWidth * 2 + shadowWidth);
        
        if (width < 1) width = 1;
        if (height < 1) height = 1;
        
        // Create bitmap
        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);
        
        // Calculate text position
        var x = padding + outlineWidth;
        var y = height - padding - outlineWidth;
        
        // Draw shadow if needed
        if (shadowWidth > 0)
        {
            using var shadowPaint = new SKPaint
            {
                Color = style.Background,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
            };
            
            canvas.DrawText(text, x + shadowWidth, y + shadowWidth, font, shadowPaint);
            
            // Draw shadow outline
            if (outlineWidth > 0)
            {
                using var shadowOutlinePaint = new SKPaint
                {
                    Color = style.Background,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = outlineWidth,
                    StrokeJoin = SKStrokeJoin.Round,
                    StrokeCap = SKStrokeCap.Round,
                };
                canvas.DrawText(text, x + shadowWidth, y + shadowWidth, font, shadowOutlinePaint);
            }
        }
        
        // Draw outline
        if (outlineWidth > 0)
        {
            using var outlinePaint = new SKPaint
            {
                Color = style.Outline,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = outlineWidth,
                StrokeJoin = SKStrokeJoin.Round,
                StrokeCap = SKStrokeCap.Round,
            };
            canvas.DrawText(text, x, y, font, outlinePaint);
        }
        
        // Draw main text
        using var textPaint = new SKPaint
        {
            Color = style.Primary,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };
        canvas.DrawText(text, x, y, font, textPaint);
        
        return bitmap.ToAvaloniaBitmap();
    }

    [RelayCommand]
    private async Task Ok()
    {
        OkPressed = true;
        Close();
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

    public void UpdateOverlayPosition()
    {
        if (VideoGrid == null || ScreenshotOverlayImage == null || ScreenshotImage == null)
        {
            return;
        }

        var screenshotImageWidth = ScreenshotImage.Bounds.Width;
        var screenshotImageHeight = ScreenshotImage.Bounds.Height;
        
        if (screenshotImageWidth <= 0 || screenshotImageHeight <= 0)
        {
            return;
        }

        var overlayBitmap = ScreenshotOverlayText;
        if (overlayBitmap == null)
        {
            return;
        }

        // Calculate scale factor based on screenshot image size
        var scaleX = screenshotImageWidth / TargetWidth;
        var scaleY = screenshotImageHeight / TargetHeight;

        // Position and size the overlay
        var overlayWidth = overlayBitmap.Size.Width * scaleX;
        var overlayHeight = overlayBitmap.Size.Height * scaleY;
        var overlayX = ScreenshotX * scaleX;
        var overlayY = ScreenshotY * scaleY;

        // Calculate the offset to center the screenshot image in the VideoGrid
        var gridWidth = VideoGrid.Bounds.Width;
        var gridHeight = VideoGrid.Bounds.Height;
        var offsetX = (gridWidth - screenshotImageWidth) / 2;
        var offsetY = (gridHeight - screenshotImageHeight) / 2;

        ScreenshotOverlayImage.Width = overlayWidth;
        ScreenshotOverlayImage.Height = overlayHeight;
        ScreenshotOverlayImage.Margin = new Thickness(
            offsetX + overlayX,
            offsetY + overlayY,
            0,
            0);

        ScreenshotOverlayPosiion = $"X: {ScreenshotX}, Y: {ScreenshotY}";
    }
}
