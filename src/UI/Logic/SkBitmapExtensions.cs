using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic;

internal static class SkBitmapExtensions
{
    public static SKBitmap ConvertToGrayscale(SKBitmap originalBitmap)
    {
        var grayscaleBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);
        using var canvas = new SKCanvas(grayscaleBitmap);
        using var paint = new SKPaint();
        paint.ColorFilter = SKColorFilter.CreateColorMatrix(new[]
        {
            0.21f, 0.72f, 0.07f, 0, 0,
            0.21f, 0.72f, 0.07f, 0, 0,
            0.21f, 0.72f, 0.07f, 0, 0,
            0,     0,     0,     1, 0
        });
        canvas.DrawBitmap(originalBitmap, 0, 0, paint);

        return grayscaleBitmap;
    }

    public static SKBitmap ConvertToGrayscale(byte[] originalBitmap)
    {
        using var ms = new MemoryStream(originalBitmap);
        var bitmap = SKBitmap.Decode(ms);
        return ConvertToGrayscale(bitmap);
    }

    public static SKBitmap AdjustColors(SKBitmap bitmap, float redIncrease, float greenIncrease, float blueIncrease)
    {
        var grayscaleBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
        using var canvas = new SKCanvas(grayscaleBitmap);
        using var paint = new SKPaint();
        paint.ColorFilter = SKColorFilter.CreateColorMatrix(new[]
        {
            1.0f + redIncrease, 1.0f, 1.0f, 0, 0,
            1.0f, 1.0f + greenIncrease, 1.0f, 0, 0,
            1.0f, 1.0f, 1.0f + blueIncrease, 0, 0,
            0,     0,     0,     1, 0
        });
        canvas.DrawBitmap(bitmap, 0, 0, paint);

        return grayscaleBitmap;
    }

    public static SKBitmap AdjustColors(byte[] originalBitmap, float redIncrease, float greenIncrease, float blueIncrease)
    {
        using var ms = new MemoryStream(originalBitmap);
        var bitmap = SKBitmap.Decode(ms);
        return AdjustColors(bitmap, redIncrease, greenIncrease, blueIncrease);
    }

    public static MemoryStream BitmapToPngStream(SKBitmap bitmap)
    {
        var ms = new MemoryStream();
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        data.SaveTo(ms);

        return ms;
    }

    public static SKBitmap AddBorder(SKBitmap originalBitmap, int borderWidth, SKColor color)
    {
        var borderedBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);
        using var canvas = new SKCanvas(borderedBitmap);
        canvas.DrawBitmap(originalBitmap, borderWidth, borderWidth);

        using var borderPaint = new SKPaint
        {
            Color = color,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = borderWidth
        };
        canvas.DrawRect(0, 0, originalBitmap.Width, originalBitmap.Height, borderPaint);

        return borderedBitmap;
    }

    public static SKBitmap AddBorder(byte[] originalBitmap, int borderWidth, SKColor color)
    {
        using var ms = new MemoryStream(originalBitmap);
        var bitmap = SKBitmap.Decode(ms);
        return AddBorder(bitmap, borderWidth, color);
    }

    public static SKBitmap MakeImageBrighter(SKBitmap bitmap, float brightnessIncrease = 0.25f)
    {
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint();
        var colorMatrix = new[]
        {
            1 + brightnessIncrease, 0, 0, 0, 0,
            0, 1 + brightnessIncrease, 0, 0, 0,
            0, 0, 1 + brightnessIncrease, 0, 0,
            0, 0, 0, 1, 0 // Alpha stays the same
        };

        paint.ColorFilter = SKColorFilter.CreateColorMatrix(colorMatrix);

        canvas.DrawBitmap(bitmap, 0, 0, paint);

        return bitmap;
    }

    public static SKBitmap MakeImageBrighter(byte[] originalBitmap, float brightnessIncrease = 0.25f)
    {
        using var ms = new MemoryStream(originalBitmap);
        var bitmap = SKBitmap.Decode(ms);
        return MakeImageBrighter(bitmap, brightnessIncrease);
    }

    public static SKBitmap CropTransparentColors(this SKBitmap originalBitmap, byte alphaThreshold = 0)
    {
        if (originalBitmap.Width == 0 || originalBitmap.Height == 0)
        {
            return originalBitmap;
        }

        var left = originalBitmap.Width;
        var top = originalBitmap.Height;
        var right = 0;
        var bottom = 0;

        // Find the bounds of non-transparent pixels
        for (var y = 0; y < originalBitmap.Height; y++)
        {
            for (var x = 0; x < originalBitmap.Width; x++)
            {
                var pixel = originalBitmap.GetPixel(x, y);
                if (pixel.Alpha > alphaThreshold)
                {
                    if (x < left)
                    {
                        left = x;
                    }

                    if (x > right)
                    {
                        right = x;
                    }

                    if (y < top)
                    {
                        top = y;
                    }

                    if (y > bottom)
                    {
                        bottom = y;
                    }
                }
            }
        }

        // If no non-transparent pixels found, return a 1x1 transparent bitmap
        if (left > right || top > bottom)
        {
            return new SKBitmap(1, 1);
        }

        var width = right - left + 1;
        var height = bottom - top + 1;

        // Create the cropped bitmap
        var croppedBitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(croppedBitmap);
        var sourceRect = new SKRect(left, top, right + 1, bottom + 1);
        var destRect = new SKRect(0, 0, width, height);
        canvas.DrawBitmap(originalBitmap, sourceRect, destRect);

        return croppedBitmap;
    }

    public static string ToBase64String(this SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data == null ? string.Empty : Convert.ToBase64String(data.ToArray());
    }

    public static byte[] ToPngArray(this SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    public static Bitmap ToAvaloniaBitmap(this SKBitmap skBitmap)
    {
        using var image = SKImage.FromBitmap(skBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream(data.ToArray());

        return new Bitmap(stream);
    }

    public static SKBitmap ToSkBitmap(this Bitmap avaloniaBitmap)
    {
        using var stream = new MemoryStream();
        avaloniaBitmap.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return SKBitmap.Decode(stream);
    }
}