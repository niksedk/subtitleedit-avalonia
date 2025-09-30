using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class PreProcessingSettings
{
    public bool CropTransparentColors { get; set; }
    public bool InverseColors { get; set; }
    public bool Binarize { get; set; }
    public bool RemoveBorders { get; set; }
    public int BorderSize { get; set; } = 2;

    public SKBitmap PreProcess(SKBitmap bitmap)
    {
        var bmp = bitmap;

        if (CropTransparentColors)
        {
            bmp = CropTransparent(bmp);
        }

        if (RemoveBorders)
        {
            bmp = RemoveBorder(bmp);

            if (CropTransparentColors)
            {
                bmp = CropTransparent(bmp);
            }
        }

        if (InverseColors)
        {
            bmp = InvertColors(bmp);
        }

        if (Binarize)
        {
            bmp = BinarizeOtsu(bmp);
        }

        return bmp;
    }

    private SKBitmap CropTransparent(SKBitmap bitmap)
    {
        var left = bitmap.Width;
        var top = bitmap.Height;
        var right = 0;
        var bottom = 0;

        // Find the bounding box of non-transparent pixels
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                if (pixel.Alpha > 0) // Non-transparent pixel
                {
                    if (x < left) left = x;
                    if (x > right) right = x;
                    if (y < top) top = y;
                    if (y > bottom) bottom = y;
                }
            }
        }

        // If no non-transparent pixels found, return original
        if (left > right || top > bottom)
        {
            return bitmap;
        }

        // Calculate cropped dimensions
        int width = right - left + 1;
        int height = bottom - top + 1;

        // Create new cropped bitmap
        var cropped = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(cropped))
        {
            var srcRect = new SKRect(left, top, right + 1, bottom + 1);
            var destRect = new SKRect(0, 0, width, height);
            canvas.DrawBitmap(bitmap, srcRect, destRect);
        }

        return cropped;
    }

    private SKBitmap InvertColors(SKBitmap bitmap)
    {
        var inverted = new SKBitmap(bitmap.Width, bitmap.Height);

        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);

                // Invert RGB channels, preserve alpha
                var invertedPixel = new SKColor(
                    (byte)(255 - pixel.Red),
                    (byte)(255 - pixel.Green),
                    (byte)(255 - pixel.Blue),
                    pixel.Alpha
                );

                inverted.SetPixel(x, y, invertedPixel);
            }
        }

        return inverted;
    }

    private SKBitmap RemoveBorder(SKBitmap bitmap)
    {
        if (BorderSize <= 0 || BorderSize * 2 >= bitmap.Width || BorderSize * 2 >= bitmap.Height)
        {
            return bitmap;
        }

        var width = bitmap.Width - (BorderSize * 2);
        var height = bitmap.Height - (BorderSize * 2);

        var cropped = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(cropped))
        {
            var srcRect = new SKRect(BorderSize, BorderSize, bitmap.Width - BorderSize, bitmap.Height - BorderSize);
            var destRect = new SKRect(0, 0, width, height);
            canvas.DrawBitmap(bitmap, srcRect, destRect);
        }

        return cropped;
    }

    private SKBitmap BinarizeOtsu(SKBitmap bitmap)
    {
        // Convert to grayscale first
        var grayscale = new SKBitmap(bitmap.Width, bitmap.Height);
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                byte gray = (byte)(0.299 * pixel.Red + 0.587 * pixel.Green + 0.114 * pixel.Blue);
                grayscale.SetPixel(x, y, new SKColor(gray, gray, gray, pixel.Alpha));
            }
        }

        // Calculate histogram
        var histogram = new int[256];
        for (var y = 0; y < grayscale.Height; y++)
        {
            for (var x = 0; x < grayscale.Width; x++)
            {
                var pixel = grayscale.GetPixel(x, y);
                histogram[pixel.Red]++;
            }
        }

        // Calculate Otsu's threshold
        int total = grayscale.Width * grayscale.Height;
        float sum = 0;
        for (var i = 0; i < 256; i++)
        {
            sum += i * histogram[i];
        }

        float sumB = 0;
        int wB = 0;
        int wF = 0;
        float maxVariance = 0;
        int threshold = 0;

        for (var i = 0; i < 256; i++)
        {
            wB += histogram[i];
            if (wB == 0) continue;

            wF = total - wB;
            if (wF == 0) break;

            sumB += i * histogram[i];
            float mB = sumB / wB;
            float mF = (sum - sumB) / wF;

            float variance = wB * wF * (mB - mF) * (mB - mF);

            if (variance > maxVariance)
            {
                maxVariance = variance;
                threshold = i;
            }
        }

        // Apply threshold
        var binarized = new SKBitmap(grayscale.Width, grayscale.Height);
        for (var y = 0; y < grayscale.Height; y++)
        {
            for (var x = 0; x < grayscale.Width; x++)
            {
                var pixel = grayscale.GetPixel(x, y);
                byte value = pixel.Red >= threshold ? (byte)255 : (byte)0;
                binarized.SetPixel(x, y, new SKColor(value, value, value, pixel.Alpha));
            }
        }

        return binarized;
    }
}