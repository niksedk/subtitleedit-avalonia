using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media.Optimized;

public class PeakGeneratorOptimized
{
    private const int StreamingBufferSize = 32 * 1024 * 1024; // 32MB buffer for streaming reads
    
    private readonly WaveHeader2 _header;
    private readonly Stream _stream;
    
    public PeakGeneratorOptimized(Stream stream, WaveHeader2 header)
    {
        _stream = stream;
        _header = header;
    }
    
    public WavePeakData2 GeneratePeaks(int delayInMilliseconds, string peakFileName)
    {
        var totalSw = System.Diagnostics.Stopwatch.StartNew();
        
        int peaksPerSecond = Math.Min(Configuration.Settings.VideoControls.WaveformMinimumSampleRate, _header.SampleRate);
        
        while (_header.SampleRate % peaksPerSecond != 0)
        {
            peaksPerSecond++;
        }
        
        int delaySampleCount = (int)(_header.SampleRate * (delayInMilliseconds / TimeCode.BaseUnit));
        delaySampleCount = Math.Max(delaySampleCount, 0);
        
        float sampleAndChannelScale = (float)WaveDataReader.GetSampleAndChannelScale(_header.BytesPerSample, _header.NumberOfChannels);
        long fileSampleCount = _header.LengthInSamples;
        int samplesPerPeak = _header.SampleRate / peaksPerSecond;
        int totalPeaks = (int)Math.Ceiling((double)(fileSampleCount + delaySampleCount) / samplesPerPeak);
        
        var peaks = new List<WavePeak2>(totalPeaks);
        
        // Add delay peaks (silence at the beginning)
        int delayPeakCount = delaySampleCount / samplesPerPeak;
        for (int i = 0; i < delayPeakCount; i++)
        {
            peaks.Add(new WavePeak2(0, 0));
        }
        
        // Calculate remaining delay samples after full peaks
        int remainingDelaySamples = delaySampleCount % samplesPerPeak;
        
        // Streaming read with pooled buffer
        var readSw = System.Diagnostics.Stopwatch.StartNew();
        _stream.Seek(_header.DataStartPosition, SeekOrigin.Begin);
        
        var buffer = WaveDataReader.RentBuffer(StreamingBufferSize);
        try
        {
            int blockAlign = _header.BlockAlign;
            int bytesPerSample = _header.BytesPerSample;
            int numberOfChannels = _header.NumberOfChannels;
            int bytesPerPeak = samplesPerPeak * blockAlign;
            
            long totalBytesToRead = _header.DataChunkSize;
            long bytesRead = 0;
            
            // Accumulator for samples across buffer boundaries
            var sampleAccumulator = new List<float>(samplesPerPeak * 2);
            int samplesNeededForNextPeak = samplesPerPeak - remainingDelaySamples;
            
            // Skip samples for partial delay peak (will be filled with zeros)
            if (remainingDelaySamples > 0)
            {
                // Add zeros for the remaining delay portion of the first peak
                for (int i = 0; i < remainingDelaySamples; i++)
                {
                    sampleAccumulator.Add(0f);
                    sampleAccumulator.Add(0f);
                }
            }
            
            while (bytesRead < totalBytesToRead)
            {
                int bytesToRead = (int)Math.Min(StreamingBufferSize, totalBytesToRead - bytesRead);
                int actualBytesRead = _stream.Read(buffer, 0, bytesToRead);
                
                if (actualBytesRead == 0)
                    break;
                
                bytesRead += actualBytesRead;
                
                // Process buffer in parallel chunks for better CPU utilization
                ProcessBufferIntoPeaks(
                    buffer, actualBytesRead, 
                    blockAlign, bytesPerSample, numberOfChannels, 
                    sampleAndChannelScale, samplesPerPeak,
                    sampleAccumulator, peaks);
            }
            
            // Handle any remaining samples in accumulator
            if (sampleAccumulator.Count > 0)
            {
                peaks.Add(CalculatePeak(sampleAccumulator));
            }
            
            readSw.Stop();
            System.Diagnostics.Debug.WriteLine($"[PERF] Optimized Peaks: Streamed {bytesRead / (1024 * 1024)}MB in {readSw.ElapsedMilliseconds}ms");
        }
        finally
        {
            WaveDataReader.ReturnBuffer(buffer);
        }
        
        // Save results
        if (!string.IsNullOrWhiteSpace(peakFileName))
        {
            var saveSw = System.Diagnostics.Stopwatch.StartNew();
            using var stream = File.Create(peakFileName);
            WavePeakGenerator2.WriteWaveformData(stream, peaksPerSecond, peaks);
            saveSw.Stop();
            System.Diagnostics.Debug.WriteLine($"[PERF] Optimized Peaks: Saved in {saveSw.ElapsedMilliseconds}ms");
        }
        
        totalSw.Stop();
        System.Diagnostics.Debug.WriteLine($"[PERF] Optimized Peaks TOTAL: {peaks.Count} peaks in {totalSw.ElapsedMilliseconds}ms ({_header.LengthInSeconds:F1}s audio)");
        
        return new WavePeakData2(peaksPerSecond, peaks);
    }
    
    private void ProcessBufferIntoPeaks(
        byte[] buffer, int bufferLength,
        int blockAlign, int bytesPerSample, int numberOfChannels,
        float sampleAndChannelScale, int samplesPerPeak,
        List<float> sampleAccumulator, List<WavePeak2> peaks)
    {
        int offset = 0;
        
        // 1. Handle Start Residual (Serial)
        if (sampleAccumulator.Count > 0)
        {
            // We need 2 accumulated values per sample (pos/neg)
            // Total samples needed for one peak is samplesPerPeak
            // Total accumulated values needed is samplesPerPeak * 2
            int valuesNeeded = samplesPerPeak * 2 - sampleAccumulator.Count;
            int samplesNeeded = valuesNeeded / 2;
            int bytesNeeded = samplesNeeded * blockAlign;
            
            if (bytesNeeded > bufferLength)
            {
                // Buffer is too small to even finish the current peak
                AppendSamplesToAccumulator(buffer, 0, bufferLength, blockAlign, bytesPerSample, numberOfChannels, sampleAndChannelScale, sampleAccumulator);
                return;
            }
            
            AppendSamplesToAccumulator(buffer, 0, bytesNeeded, blockAlign, bytesPerSample, numberOfChannels, sampleAndChannelScale, sampleAccumulator);
            peaks.Add(CalculatePeak(sampleAccumulator));
            sampleAccumulator.Clear();
            offset += bytesNeeded;
        }
        
        // 2. Parallel Processing of Full Peaks
        int remainingBytes = bufferLength - offset;
        int remainingFrames = remainingBytes / blockAlign;
        int fullPeaksCount = remainingFrames / samplesPerPeak;
        
        if (fullPeaksCount > 0)
        {
            var parallelPeaks = new WavePeak2[fullPeaksCount];
            int parallelOptions = Environment.ProcessorCount;
            
            Parallel.For(0, fullPeaksCount, new ParallelOptions { MaxDegreeOfParallelism = parallelOptions }, i =>
            {
                int peakStartOffset = offset + (i * samplesPerPeak * blockAlign);
                parallelPeaks[i] = CalculatePeakFromBuffer(
                    buffer, peakStartOffset, 
                    samplesPerPeak, 
                    blockAlign, bytesPerSample, numberOfChannels, 
                    sampleAndChannelScale);
            });
            
            peaks.AddRange(parallelPeaks);
            
            offset += fullPeaksCount * samplesPerPeak * blockAlign;
        }
        
        // 3. Handle End Residual (Serial)
        if (offset < bufferLength)
        {
            AppendSamplesToAccumulator(buffer, offset, bufferLength - offset, blockAlign, bytesPerSample, numberOfChannels, sampleAndChannelScale, sampleAccumulator);
        }
    }
    
    private static void AppendSamplesToAccumulator(
        byte[] buffer, int offset, int length,
        int blockAlign, int bytesPerSample, int numberOfChannels,
        float scale, List<float> sampleAccumulator)
    {
        int endOffset = offset + length;
        int currentOffset = offset;
        
        while (currentOffset < endOffset)
        {
            float valuePositive = 0f;
            float valueNegative = 0f;
            
            for (int iChannel = 0; iChannel < numberOfChannels; iChannel++)
            {
                if (currentOffset + bytesPerSample > endOffset)
                    break;
                    
                var v = WaveDataReader.ReadSampleValue(buffer, ref currentOffset, bytesPerSample);
                if (v < 0)
                    valueNegative += v;
                else
                    valuePositive += v;
            }
            
            sampleAccumulator.Add(valueNegative * scale);
            sampleAccumulator.Add(valuePositive * scale);
        }
    }

    private static WavePeak2 CalculatePeakFromBuffer(
        byte[] buffer, int startOffset, int samplesToRead,
        int blockAlign, int bytesPerSample, int numberOfChannels,
        float scale)
    {
        float max = 0f;
        float min = 0f;
        bool initialized = false;
        
        int offset = startOffset;
        int endOffset = startOffset + (samplesToRead * blockAlign);
        
        while (offset < endOffset)
        {
            float valPos = 0f;
            float valNeg = 0f;
            
            for (int c = 0; c < numberOfChannels; c++)
            {
                 var v = WaveDataReader.ReadSampleValue(buffer, ref offset, bytesPerSample);
                 if (v < 0) valNeg += v;
                 else valPos += v;
            }
            
            float p = valPos * scale;
            float n = valNeg * scale;
            
            if (!initialized) { 
                max = p; 
                min = n; 
                initialized = true; 
            }
            else 
            {
                if (p > max) max = p;
                else if (p < min) min = p;
                
                if (n > max) max = n;
                else if (n < min) min = n;
            }
        }
        
        if (!initialized) return new WavePeak2(0, 0);
        
        return new WavePeak2((short)(short.MaxValue * max), (short)(short.MaxValue * min));
    }

    private static WavePeak2 CalculatePeak(List<float> samples)
    {
        if (samples.Count == 0) 
            return new WavePeak2();
        
        var max = samples[0];
        var min = samples[0];
        
        for (var i = 1; i < samples.Count; i++)
        {
            var value = samples[i];
            if (value > max) max = value;
            else if (value < min) min = value;
        }
        
        return new WavePeak2((short)(short.MaxValue * max), (short)(short.MaxValue * min));
    }
}
