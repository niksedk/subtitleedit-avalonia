using BenchmarkDotNet.Attributes;
using SkiaSharp;

namespace Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 2, iterationCount: 3)]
public class MakeOneColorBenchmark
{
    private SKBitmap _testBitmap = null!;
    private SKColor _targetColor;

    [Params(32, 64, 128)]
    public int BitmapSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _targetColor = new SKColor(200, 200, 200);
        _testBitmap = CreateTestBitmap(BitmapSize, BitmapSize);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _testBitmap?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public SKBitmap CurrentImplementation()
    {
        return MakeOneColorCurrent(_testBitmap);
    }

    [Benchmark]
    public SKBitmap OptimizedImplementation()
    {
        return MakeOneColorOptimized(_testBitmap);
    }

    private SKBitmap CreateTestBitmap(int width, int height)
    {
        var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        unsafe
        {
            byte* ptr = (byte*)bitmap.GetPixels();
            int stride = bitmap.RowBytes;

            for (int y = 0; y < height; y++)
            {
                uint* row = (uint*)(ptr + y * stride);
                for (int x = 0; x < width; x++)
                {
                    byte intensity = (byte)((x + y) % 256);
                    uint pixel = (255u << 24) | ((uint)intensity << 16) | ((uint)intensity << 8) | intensity;
                    row[x] = pixel;
                }
            }
        }

        return bitmap;
    }

    private SKBitmap MakeOneColorCurrent(SKBitmap bitmap)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;

        var result = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);

                var intensity = (pixel.Red * 0.299 + pixel.Green * 0.587 + pixel.Blue * 0.114) / 255.0;

                var newRed = (byte)(_targetColor.Red * intensity);
                var newGreen = (byte)(_targetColor.Green * intensity);
                var newBlue = (byte)(_targetColor.Blue * intensity);

                var newColor = new SKColor(newRed, newGreen, newBlue, pixel.Alpha);
                result.SetPixel(x, y, newColor);
            }
        }

        return result;
    }

    private SKBitmap MakeOneColorOptimized(SKBitmap bitmap)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        
        var result = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        unsafe
        {
            byte* srcBase = (byte*)bitmap.GetPixels();
            byte* dstBase = (byte*)result.GetPixels();
            int srcStride = bitmap.RowBytes;
            int dstStride = result.RowBytes;

            for (int y = 0; y < height; y++)
            {
                uint* srcRow = (uint*)(srcBase + y * srcStride);
                uint* dstRow = (uint*)(dstBase + y * dstStride);

                for (int x = 0; x < width; x++)
                {
                    uint pixel = srcRow[x];
                    byte b = (byte)(pixel & 0xFF);
                    byte g = (byte)((pixel >> 8) & 0xFF);
                    byte r = (byte)((pixel >> 16) & 0xFF);
                    byte a = (byte)(pixel >> 24);

                    double intensity = (r * 0.299 + g * 0.587 + b * 0.114) / 255.0;

                    byte newR = (byte)(_targetColor.Red * intensity);
                    byte newG = (byte)(_targetColor.Green * intensity);
                    byte newB = (byte)(_targetColor.Blue * intensity);

                    dstRow[x] = (uint)(a << 24) | (uint)(newR << 16) | (uint)(newG << 8) | newB;
                }
            }
        }

        return result;
    }
}
