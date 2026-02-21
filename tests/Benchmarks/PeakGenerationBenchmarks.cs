using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.Media.Optimized;

namespace Benchmarks;

/// <summary>
/// Benchmarks comparing legacy WavePeakGenerator2 vs PeakGeneratorOptimized.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class PeakGenerationBenchmarks
{
    private string _wavFile = null!;
    private string _tempDir = null!;

    [Params("1min", "10min", "60min")]
    public string Duration { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"se_bench_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _wavFile = WavFileGenerator.Generate(Duration, _tempDir);
    }

    [Benchmark(Baseline = true)]
    public void Legacy()
    {
        var peakFile = Path.Combine(_tempDir, $"legacy_{Guid.NewGuid():N}.peak");
        try
        {
            using var generator = new WavePeakGenerator2(File.OpenRead(_wavFile));
            generator.GeneratePeaks(0, peakFile);
        }
        finally
        {
            File.Delete(peakFile);
        }
    }

    [Benchmark]
    public void Optimized()
    {
        var peakFile = Path.Combine(_tempDir, $"opt_{Guid.NewGuid():N}.peak");
        try
        {
            using var stream = File.OpenRead(_wavFile);
            var header = new WaveHeader2(stream);
            var generator = new PeakGeneratorOptimized(stream, header);
            generator.GeneratePeaks(0, peakFile);
        }
        finally
        {
            File.Delete(peakFile);
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
