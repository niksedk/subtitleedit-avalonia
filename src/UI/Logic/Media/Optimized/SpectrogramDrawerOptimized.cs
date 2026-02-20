using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Runtime.CompilerServices;

namespace Nikse.SubtitleEdit.Logic.Media.Optimized;

public class SpectrogramDrawerOptimized
{
    private const double RaisedCosineWindowScale = 0.5;
    private const int MagnitudeIndexRange = 256;
    
    private readonly int _nfft;
    private readonly MagnitudeToIndexMapper _mapper;
    private readonly RealFFT _fft;
    private readonly SKColor[] _palette;
    private readonly double[] _segment;
    private readonly double[] _window;
    private readonly double[] _magnitude1;
    private readonly double[] _magnitude2;
    
    public SpectrogramDrawerOptimized(int nfft)
    {
        _nfft = nfft;
        _mapper = new MagnitudeToIndexMapper(100.0, MagnitudeIndexRange - 1);
        _fft = new RealFFT(nfft);
        _palette = GeneratePalette();
        _segment = new double[nfft];
        _window = CreateRaisedCosineWindow(nfft);
        _magnitude1 = new double[nfft / 2];
        _magnitude2 = new double[nfft / 2];
        
        double scaleCorrection = 1.0 / (RaisedCosineWindowScale * _fft.ForwardScaleFactor);
        for (int i = 0; i < _window.Length; i++)
        {
            _window[i] *= scaleCorrection;
        }
    }
    
    public SKBitmap Draw(double[] samples)
    {
        int width = samples.Length / _nfft;
        int height = _nfft / 2;
        
        // Validate dimensions
        if (width <= 0 || height <= 0)
        {
            return new SKBitmap(1, 1, SKColorType.Rgba8888, SKAlphaType.Premul);
        }
        
        var bmp = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        
        // Use SKPixmap for safer pixel access that validates stride
        using var pixmap = bmp.PeekPixels();
        if (pixmap == null)
        {
            return bmp;
        }
        
        // Get pixel span for direct writing - validated by SKPixmap
        var pixelSpan = bmp.GetPixelSpan();
        int stride = bmp.RowBytes;
        int bytesPerPixel = 4; // RGBA8888
        
        // Validate stride matches expected format
        if (stride < width * bytesPerPixel)
        {
            // Fallback to canvas-based drawing if stride is unexpected
            DrawWithCanvas(bmp, samples, width, height);
            return bmp;
        }
        
        DrawDirect(pixelSpan, stride, samples, width, height);
        
        bmp.NotifyPixelsChanged();
        return bmp;
    }
    
    /// <summary>
    /// Direct pixel writing - fastest path when stride is validated.
    /// </summary>
    private void DrawDirect(Span<byte> pixels, int stride, double[] samples, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            // Calculate segment offsets with bounds checking
            int offset1 = (x * _nfft) - (x > 0 ? _nfft / 4 : 0);
            int offset2 = (x * _nfft) + (x < width - 1 ? _nfft / 4 : 0);
            
            // Ensure offsets are within bounds
            offset1 = Math.Max(0, Math.Min(offset1, samples.Length - _nfft));
            offset2 = Math.Max(0, Math.Min(offset2, samples.Length - _nfft));
            
            ProcessSegment(samples, offset1, _magnitude1);
            ProcessSegment(samples, offset2, _magnitude2);
            
            for (int y = 0; y < height; y++)
            {
                int colorIndex = _mapper.Map((_magnitude1[y] + _magnitude2[y]) / 2.0);
                SKColor color = _palette[colorIndex];
                
                int pixelY = height - y - 1;
                int pixelOffset = (pixelY * stride) + (x * 4);
                
                // Bounds check for safety
                if (pixelOffset + 3 < pixels.Length)
                {
                    pixels[pixelOffset] = color.Red;
                    pixels[pixelOffset + 1] = color.Green;
                    pixels[pixelOffset + 2] = color.Blue;
                    pixels[pixelOffset + 3] = color.Alpha;
                }
            }
        }
    }
    
    /// <summary>
    /// Canvas-based fallback for non-standard formats.
    /// </summary>
    private void DrawWithCanvas(SKBitmap bmp, double[] samples, int width, int height)
    {
        using var canvas = new SKCanvas(bmp);
        using var paint = new SKPaint();
        
        for (int x = 0; x < width; x++)
        {
            int offset1 = (x * _nfft) - (x > 0 ? _nfft / 4 : 0);
            int offset2 = (x * _nfft) + (x < width - 1 ? _nfft / 4 : 0);
            
            offset1 = Math.Max(0, Math.Min(offset1, samples.Length - _nfft));
            offset2 = Math.Max(0, Math.Min(offset2, samples.Length - _nfft));
            
            ProcessSegment(samples, offset1, _magnitude1);
            ProcessSegment(samples, offset2, _magnitude2);
            
            for (int y = 0; y < height; y++)
            {
                int colorIndex = _mapper.Map((_magnitude1[y] + _magnitude2[y]) / 2.0);
                paint.Color = _palette[colorIndex];
                canvas.DrawPoint(x, height - y - 1, paint);
            }
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessSegment(double[] samples, int offset, double[] magnitude)
    {
        for (int i = 0; i < _nfft; i++)
        {
            _segment[i] = samples[offset + i] * _window[i];
        }
        
        _fft.ComputeForward(_segment);
        MagnitudeSpectrum(_segment, magnitude);
    }
    
    private static double[] CreateRaisedCosineWindow(int n)
    {
        double twoPiOverN = Math.PI * 2.0 / n;
        double[] dst = new double[n];
        for (int i = 0; i < n; i++)
        {
            dst[i] = 0.5 * (1.0 - Math.Cos(twoPiOverN * i));
        }
        return dst;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MagnitudeSpectrum(double[] segment, double[] magnitude)
    {
        magnitude[0] = Math.Sqrt(SquareSum(segment[0], segment[1]));
        for (int i = 2; i < segment.Length; i += 2)
        {
            magnitude[i / 2] = Math.Sqrt(SquareSum(segment[i], segment[i + 1]) * 2.0);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double SquareSum(double a, double b)
    {
        return a * a + b * b;
    }
    
    private static SKColor[] GeneratePalette()
    {
        var palette = new SKColor[MagnitudeIndexRange];
        if (Se.Settings.Waveform.SpectrogramStyle == SeSpectrogramStyle.ClassicViridis.ToString())
        {
            for (int colorIndex = 0; colorIndex < MagnitudeIndexRange; colorIndex++)
            {
                palette[colorIndex] = PaletteValueViridis(colorIndex, MagnitudeIndexRange);
            }
        }
        else if (Se.Settings.Waveform.SpectrogramStyle == SeSpectrogramStyle.ClassicPlasma.ToString())
        {
            for (int colorIndex = 0; colorIndex < MagnitudeIndexRange; colorIndex++)
            {
                palette[colorIndex] = PaletteValuePlasma(colorIndex, MagnitudeIndexRange);
            }
        }
        else
        {
            for (int colorIndex = 0; colorIndex < MagnitudeIndexRange; colorIndex++)
            {
                palette[colorIndex] = PaletteValue(colorIndex, MagnitudeIndexRange);
            }
        }
        return palette;
    }
    
    private static SKColor PaletteValue(int index, int count)
    {
        var h = (double)index / count;
        HsvToRgb(h, 1.0, 1.0, out byte r, out byte g, out byte b);
        return new SKColor(r, g, b, 255);
    }
    
    private static SKColor PaletteValueViridis(int index, int count)
    {
        double t = (double)index / (count - 1);
        byte r = (byte)(255 * Math.Clamp(0.267004 + t * (0.329415 + t * (-1.513099 + t * (1.981588 + t * (-0.896436)))), 0, 1));
        byte g = (byte)(255 * Math.Clamp(0.004874 + t * (0.815552 + t * (0.170499 + t * (-0.701524 + t * (0.311595)))), 0, 1));
        byte b = (byte)(255 * Math.Clamp(0.329415 + t * (0.617668 + t * (-0.963814 + t * (1.135455 + t * (-0.525790)))), 0, 1));
        return new SKColor(r, g, b, 255);
    }
    
    private static SKColor PaletteValuePlasma(int index, int count)
    {
        double t = (double)index / (count - 1);
        byte r = (byte)(255 * Math.Clamp(0.050383 + t * (2.388937 + t * (-1.433420)), 0, 1));
        byte g = (byte)(255 * Math.Clamp(0.029803 + t * (0.256790 + t * (0.712953)), 0, 1));
        byte b = (byte)(255 * Math.Clamp(0.529803 + t * (0.741791 + t * (-1.271584)), 0, 1));
        return new SKColor(r, g, b, 255);
    }
    
    /// <summary>
    /// Converts HSV to RGB without allocating a byte array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void HsvToRgb(double h, double s, double v, out byte r, out byte g, out byte b)
    {
        double rd, gd, bd;
        int i = (int)(h * 6);
        double f = h * 6 - i;
        double p = v * (1 - s);
        double q = v * (1 - f * s);
        double t = v * (1 - (1 - f) * s);
        
        switch (i % 6)
        {
            case 0: rd = v; gd = t; bd = p; break;
            case 1: rd = q; gd = v; bd = p; break;
            case 2: rd = p; gd = v; bd = t; break;
            case 3: rd = p; gd = q; bd = v; break;
            case 4: rd = t; gd = p; bd = v; break;
            default: rd = v; gd = p; bd = q; break;
        }
        
        r = (byte)(rd * 255);
        g = (byte)(gd * 255);
        b = (byte)(bd * 255);
    }
    
    private sealed class MagnitudeToIndexMapper
    {
        private readonly double _minMagnitude;
        private readonly double _multiplier;
        private readonly double _addend;
        private readonly int _maxIndex;

        public MagnitudeToIndexMapper(double decibelRange, int indexMax)
        {
            _maxIndex = indexMax;
            double mappingScale = indexMax / decibelRange;
            _minMagnitude = Math.Pow(10.0, -decibelRange / 20.0);
            _multiplier = 20.0 * mappingScale;
            _addend = decibelRange * mappingScale;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Map(double magnitude)
        {
            if (magnitude < _minMagnitude)
                return 0;
            
            int result = (int)(Math.Log10(magnitude) * _multiplier + _addend);
            return Math.Clamp(result, 0, _maxIndex);
        }
    }
}
