using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;

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
    
    public unsafe SKBitmap Draw(double[] samples)
    {
        int width = samples.Length / _nfft;
        int height = _nfft / 2;
        var bmp = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        
        IntPtr pixelsPtr = bmp.GetPixels();
        byte* pixels = (byte*)pixelsPtr.ToPointer();
        int stride = bmp.RowBytes;
        
        for (int x = 0; x < width; x++)
        {
            ProcessSegment(samples, (x * _nfft) - (x > 0 ? _nfft / 4 : 0), _magnitude1);
            ProcessSegment(samples, (x * _nfft) + (x < width - 1 ? _nfft / 4 : 0), _magnitude2);
            
            for (int y = 0; y < height; y++)
            {
                int colorIndex = _mapper.Map((_magnitude1[y] + _magnitude2[y]) / 2.0);
                SKColor color = _palette[colorIndex];
                
                int pixelY = height - y - 1;
                byte* pixel = pixels + (pixelY * stride) + (x * 4);
                pixel[0] = color.Red;
                pixel[1] = color.Green;
                pixel[2] = color.Blue;
                pixel[3] = color.Alpha;
            }
        }
        
        bmp.NotifyPixelsChanged();
        return bmp;
    }
    
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
    
    private static void MagnitudeSpectrum(double[] segment, double[] magnitude)
    {
        magnitude[0] = Math.Sqrt(SquareSum(segment[0], segment[1]));
        for (int i = 2; i < segment.Length; i += 2)
        {
            magnitude[i / 2] = Math.Sqrt(SquareSum(segment[i], segment[i + 1]) * 2.0);
        }
    }
    
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
        var rgb = HsvToRgb(h, 1.0, 1.0);
        return new SKColor(rgb[0], rgb[1], rgb[2], 255);
    }
    
    private static SKColor PaletteValueViridis(int index, int count)
    {
        double t = (double)index / (count - 1);
        byte r = (byte)(255 * (0.267004 + t * (0.329415 + t * (-1.513099 + t * (1.981588 + t * (-0.896436))))));
        byte g = (byte)(255 * (0.004874 + t * (0.815552 + t * (0.170499 + t * (-0.701524 + t * (0.311595))))));
        byte b = (byte)(255 * (0.329415 + t * (0.617668 + t * (-0.963814 + t * (1.135455 + t * (-0.525790))))));
        return new SKColor(r, g, b, 255);
    }
    
    private static SKColor PaletteValuePlasma(int index, int count)
    {
        double t = (double)index / (count - 1);
        byte r = (byte)(255 * Math.Min(1, 0.050383 + t * (2.388937 + t * (-1.433420))));
        byte g = (byte)(255 * Math.Max(0, Math.Min(1, 0.029803 + t * (0.256790 + t * (0.712953)))));
        byte b = (byte)(255 * Math.Max(0, Math.Min(1, 0.529803 + t * (0.741791 + t * (-1.271584)))));
        return new SKColor(r, g, b, 255);
    }
    
    private static byte[] HsvToRgb(double h, double s, double v)
    {
        double r, g, b;
        int i = (int)(h * 6);
        double f = h * 6 - i;
        double p = v * (1 - s);
        double q = v * (1 - f * s);
        double t = v * (1 - (1 - f) * s);
        
        switch (i % 6)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            default: r = v; g = p; b = q; break;
        }
        
        return new[] { (byte)(r * 255), (byte)(g * 255), (byte)(b * 255) };
    }
    
    private class MagnitudeToIndexMapper
    {
        private readonly double _minMagnitude;
        private readonly double _multiplier;
        private readonly double _addend;

        public MagnitudeToIndexMapper(double decibelRange, int indexMax)
        {
            double mappingScale = indexMax / decibelRange;
            _minMagnitude = Math.Pow(10.0, -decibelRange / 20.0);
            _multiplier = 20.0 * mappingScale;
            _addend = decibelRange * mappingScale;
        }

        public int Map(double magnitude)
        {
            return magnitude >= _minMagnitude ? (int)(Math.Log10(magnitude) * _multiplier + _addend) : 0;
        }
    }
}
