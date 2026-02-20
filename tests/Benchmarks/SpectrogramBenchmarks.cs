using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.Media.Optimized;
using SkiaSharp;
using System.Threading;

namespace Benchmarks;

/// <summary>
/// Benchmarks comparing legacy spectrogram generation vs optimized parallel,
/// and JPEG vs binary spectrogram save/load.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class SpectrogramGenerationBenchmarks
{
    private string _wavFile = null!;
    private string _tempDir = null!;

    [Params("1min", "10min")]
    public string Duration { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"se_spec_bench_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _wavFile = WavFileGenerator.Generate(Duration, _tempDir);
    }

    [Benchmark(Baseline = true)]
    public void Legacy()
    {
        var specDir = Path.Combine(_tempDir, $"spec_legacy_{Guid.NewGuid():N}");
        try
        {
            using var generator = new WavePeakGenerator2(File.OpenRead(_wavFile));
            generator.GenerateSpectrogram(0, specDir, CancellationToken.None);
        }
        finally
        {
            if (Directory.Exists(specDir))
            {
                Directory.Delete(specDir, true);
            }
        }
    }

    [Benchmark]
    public void Optimized()
    {
        var specDir = Path.Combine(_tempDir, $"spec_opt_{Guid.NewGuid():N}");
        try
        {
            using var stream = File.OpenRead(_wavFile);
            var header = new WaveHeader2(stream);
            var generator = new SpectrogramGeneratorOptimized(stream, header);
            generator.GenerateSpectrogram(0, specDir, CancellationToken.None);
        }
        finally
        {
            if (Directory.Exists(specDir))
            {
                Directory.Delete(specDir, true);
            }
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch
        {
            // ignore cleanup errors
        }
    }
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class SpectrogramLoadBenchmarks
{
    private string _tempDir = null!;
    private string _jpegDir = null!;
    private string _binaryDir = null!;
    private SKBitmap[] _testImages = null!;

    [GlobalSetup]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"se_specload_bench_{Guid.NewGuid():N}");
        _jpegDir = Path.Combine(_tempDir, "jpeg");
        _binaryDir = Path.Combine(_tempDir, "binary");
        Directory.CreateDirectory(_jpegDir);
        Directory.CreateDirectory(_binaryDir);

        // Generate test spectrogram images (100 chunks of 1024x128)
        const int chunkCount = 100;
        const int width = 1024;
        const int height = 128;
        _testImages = new SKBitmap[chunkCount];

        for (int i = 0; i < chunkCount; i++)
        {
            var bmp = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(bmp);
            // Fill with gradient pattern for realistic data
            using var paint = new SKPaint();
            for (int y = 0; y < height; y++)
            {
                paint.Color = new SKColor((byte)(i % 256), (byte)(y * 2), (byte)((i + y) % 256), 255);
                canvas.DrawLine(0, y, width, y, paint);
            }
            _testImages[i] = bmp;
        }

        // Save as JPEG
        for (int i = 0; i < chunkCount; i++)
        {
            string path = Path.Combine(_jpegDir, $"{i}.jpg");
            using var stream = File.OpenWrite(path);
            using var data = _testImages[i].Encode(SKEncodedImageFormat.Jpeg, 50);
            data.SaveTo(stream);
        }

        // Save spectrogram metadata XML
        SaveInfoXml(_jpegDir, chunkCount, width, height);
        SaveInfoXml(_binaryDir, chunkCount, width, height);

        // Save as binary format
        BinarySpectrogramFormat.Save(_testImages, _binaryDir);
    }

    private static void SaveInfoXml(string dir, int chunkCount, int width, int height)
    {
        const int fftSize = 256;
        const int sampleRate = 44100;
        double sampleDuration = (double)fftSize / sampleRate;
        double secondsPerImage = (double)(fftSize * width) / sampleRate;

        string xml = $@"<SpectrogramInfo>
  <SampleDuration>{sampleDuration.ToString(System.Globalization.CultureInfo.InvariantCulture)}</SampleDuration>
  <NFFT>{fftSize}</NFFT>
  <ImageWidth>{width}</ImageWidth>
  <SecondsPerImage>{secondsPerImage.ToString(System.Globalization.CultureInfo.InvariantCulture)}</SecondsPerImage>
</SpectrogramInfo>";

        File.WriteAllText(Path.Combine(dir, "Info.xml"), xml);
    }

    [Benchmark(Baseline = true)]
    public void LoadJpeg()
    {
        var data = SpectrogramData2.FromDisk(_jpegDir);
        data.Load();
        data.Dispose();
    }

    [Benchmark]
    public void LoadBinaryLazy()
    {
        var loader = BinarySpectrogramFormat.CreateLazyLoader(_binaryDir);
        if (loader != null)
        {
            // Access a few chunks to simulate real usage
            for (int i = 0; i < Math.Min(10, loader.ChunkCount); i++)
            {
                var chunk = loader.GetChunk(i);
            }
            loader.Dispose();
        }
    }

    [Benchmark]
    public void LoadBinaryMemoryMapped()
    {
        var loader = BinarySpectrogramFormat.CreateMemoryMappedLoader(_binaryDir);
        if (loader != null)
        {
            // Access a few chunks to simulate real usage
            for (int i = 0; i < Math.Min(10, loader.ChunkCount); i++)
            {
                var chunk = loader.GetChunk(i);
                chunk.Dispose();
            }
            loader.Dispose();
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        foreach (var img in _testImages)
        {
            img.Dispose();
        }

        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch
        {
            // ignore
        }
    }
}
