using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.Media.Optimized;

public static class WaveProcessorFactory
{
    public static WavePeakData2 GeneratePeaks(string waveFileName, int delayInMilliseconds, string peakFileName)
    {
        using var stream = new FileStream(waveFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return GeneratePeaks(stream, delayInMilliseconds, peakFileName);
    }
    
    public static WavePeakData2 GeneratePeaks(Stream stream, int delayInMilliseconds, string peakFileName)
    {
        var header = new WaveHeader2(stream);
        
        if (!IsSupported(header))
        {
            throw new InvalidDataException("Unsupported wave format");
        }
        
        if (Configuration.Settings.VideoControls.UseExperimentalRenderer)
        {
            System.Diagnostics.Debug.WriteLine("[PERF] Using OPTIMIZED peak generator");
            var generator = new PeakGeneratorOptimized(stream, header);
            return generator.GeneratePeaks(delayInMilliseconds, peakFileName);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[PERF] Using LEGACY peak generator");
            stream.Position = 0;
            using var legacyGenerator = new WavePeakGenerator2(stream);
            return legacyGenerator.GeneratePeaks(delayInMilliseconds, peakFileName);
        }
    }
    
    public static SpectrogramData2 GenerateSpectrogram(string waveFileName, int delayInMilliseconds, string spectrogramDirectory, CancellationToken token)
    {
        using var stream = new FileStream(waveFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return GenerateSpectrogram(stream, delayInMilliseconds, spectrogramDirectory, token);
    }
    
    public static SpectrogramData2 GenerateSpectrogram(Stream stream, int delayInMilliseconds, string spectrogramDirectory, CancellationToken token)
    {
        var header = new WaveHeader2(stream);
        
        if (!IsSupported(header))
        {
            throw new InvalidDataException("Unsupported wave format");
        }
        
        if (Configuration.Settings.VideoControls.UseExperimentalRenderer)
        {
            System.Diagnostics.Debug.WriteLine("[PERF] Using OPTIMIZED spectrogram generator");
            var generator = new SpectrogramGeneratorOptimized(stream, header);
            return generator.GenerateSpectrogram(delayInMilliseconds, spectrogramDirectory, token);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[PERF] Using LEGACY spectrogram generator");
            stream.Position = 0;
            using var legacyGenerator = new WavePeakGenerator2(stream);
            return legacyGenerator.GenerateSpectrogram(delayInMilliseconds, spectrogramDirectory, token);
        }
    }
    
    public static (WavePeakData2 Peaks, SpectrogramData2? Spectrogram) GeneratePeaksAndSpectrogram(
        string waveFileName,
        int delayInMilliseconds,
        string peakFileName,
        string spectrogramDirectory,
        bool generateSpectrogram,
        CancellationToken token)
    {
        using var stream = new FileStream(waveFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var header = new WaveHeader2(stream);
        
        if (!IsSupported(header))
        {
            throw new InvalidDataException("Unsupported wave format");
        }
        
        WavePeakData2 peaks;
        SpectrogramData2? spectrogram = null;
        
        if (Configuration.Settings.VideoControls.UseExperimentalRenderer)
        {
            System.Diagnostics.Debug.WriteLine("[PERF] Using OPTIMIZED generators (parallel processing enabled)");
            
            // Generate peaks
            var peakGenerator = new PeakGeneratorOptimized(stream, header);
            peaks = peakGenerator.GeneratePeaks(delayInMilliseconds, peakFileName);
            
            // Generate spectrogram if requested
            if (generateSpectrogram && !token.IsCancellationRequested)
            {
                stream.Position = 0;
                var spectrogramGenerator = new SpectrogramGeneratorOptimized(stream, header);
                spectrogram = spectrogramGenerator.GenerateSpectrogram(delayInMilliseconds, spectrogramDirectory, token);
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[PERF] Using LEGACY generators (sequential processing)");
            stream.Position = 0;
            using var legacyGenerator = new WavePeakGenerator2(stream);
            
            peaks = legacyGenerator.GeneratePeaks(delayInMilliseconds, peakFileName);
            
            if (generateSpectrogram && !token.IsCancellationRequested)
            {
                spectrogram = legacyGenerator.GenerateSpectrogram(delayInMilliseconds, spectrogramDirectory, token);
            }
        }
        
        return (peaks, spectrogram);
    }
    
    private static bool IsSupported(WaveHeader2 header)
    {
        return header.AudioFormat == WaveHeader2.AudioFormatPcm && header.Format == "WAVE";
    }
}
