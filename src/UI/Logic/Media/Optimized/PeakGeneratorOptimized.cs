using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media.Optimized;

public class PeakGeneratorOptimized
{
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
        
        float sampleAndChannelScale = (float)GetSampleAndChannelScale();
        long fileSampleCount = _header.LengthInSamples;
        long fileSampleOffset = -delaySampleCount;
        int chunkSampleCount = _header.SampleRate / peaksPerSecond;
        
        // Read all data at once for better I/O performance
        var readSw = System.Diagnostics.Stopwatch.StartNew();
        _stream.Seek(_header.DataStartPosition, SeekOrigin.Begin);
        
        if (fileSampleOffset > 0)
        {
            _stream.Seek(fileSampleOffset * _header.BlockAlign, SeekOrigin.Current);
        }
        
        int totalChunks = (int)Math.Ceiling((double)(fileSampleCount + delaySampleCount) / chunkSampleCount);
        var allData = new byte[_header.DataChunkSize];
        int bytesRead = _stream.Read(allData, 0, allData.Length);
        readSw.Stop();
        System.Diagnostics.Debug.WriteLine($"[PERF] Optimized Peaks: Read {bytesRead / 1024}KB in {readSw.ElapsedMilliseconds}ms");
        
        // Process peaks in parallel
        var processSw = System.Diagnostics.Stopwatch.StartNew();
        var peaks = new WavePeak2[totalChunks];
        int blockAlign = _header.BlockAlign;
        int bytesPerSample = _header.BytesPerSample;
        int numberOfChannels = _header.NumberOfChannels;
        int bytesPerChunk = chunkSampleCount * blockAlign;
        
        int threadCount = Math.Min(Environment.ProcessorCount, 16);
        Parallel.For(0, totalChunks, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, chunkIndex =>
        {
            long chunkFileSampleOffset = (long)chunkIndex * chunkSampleCount - delaySampleCount;
            
            int startSkipSampleCount = 0;
            if (chunkFileSampleOffset < 0)
            {
                startSkipSampleCount = (int)Math.Min(-chunkFileSampleOffset, chunkSampleCount);
                chunkFileSampleOffset += startSkipSampleCount;
            }
            
            long fileSamplesRemaining = fileSampleCount - Math.Max(chunkFileSampleOffset, 0);
            int fileReadSampleCount = (int)Math.Min(fileSamplesRemaining, chunkSampleCount - startSkipSampleCount);
            
            if (fileReadSampleCount > 0)
            {
                int dataOffset = (int)(chunkFileSampleOffset * blockAlign);
                if (dataOffset < 0) dataOffset = 0;
                
                var chunkSamples = new float[fileReadSampleCount * 2];
                int chunkSampleOffset = 0;
                int fileReadByteCount = fileReadSampleCount * blockAlign;
                int endOffset = Math.Min(dataOffset + fileReadByteCount, bytesRead);
                
                while (dataOffset < endOffset)
                {
                    float valuePositive = 0F;
                    float valueNegative = 0F;
                    for (int iChannel = 0; iChannel < numberOfChannels; iChannel++)
                    {
                        var v = ReadSampleValue(allData, ref dataOffset, bytesPerSample);
                        if (v < 0)
                            valueNegative += v;
                        else
                            valuePositive += v;
                    }
                    
                    chunkSamples[chunkSampleOffset++] = valueNegative * sampleAndChannelScale;
                    chunkSamples[chunkSampleOffset++] = valuePositive * sampleAndChannelScale;
                }
                
                peaks[chunkIndex] = CalculatePeak(chunkSamples, fileReadSampleCount * 2);
            }
            else
            {
                peaks[chunkIndex] = new WavePeak2(0, 0);
            }
        });
        processSw.Stop();
        System.Diagnostics.Debug.WriteLine($"[PERF] Optimized Peaks: Processed {peaks.Length} peaks in {processSw.ElapsedMilliseconds}ms ({threadCount} threads)");
        
        var peakList = new List<WavePeak2>(peaks);
        
        // Save results
        if (!string.IsNullOrWhiteSpace(peakFileName))
        {
            var saveSw = System.Diagnostics.Stopwatch.StartNew();
            using var stream = File.Create(peakFileName);
            WavePeakGenerator2.WriteWaveformData(stream, peaksPerSecond, peakList);
            saveSw.Stop();
            System.Diagnostics.Debug.WriteLine($"[PERF] Optimized Peaks: Saved in {saveSw.ElapsedMilliseconds}ms");
        }
        
        totalSw.Stop();
        System.Diagnostics.Debug.WriteLine($"[PERF] Optimized Peaks TOTAL: {peakList.Count} peaks in {totalSw.ElapsedMilliseconds}ms ({_header.LengthInSeconds:F1}s audio)");
        
        return new WavePeakData2(peaksPerSecond, peakList);
    }
    
    private static int ReadSampleValue(byte[] data, ref int index, int bytesPerSample)
    {
        return bytesPerSample switch
        {
            1 => ReadValue8Bit(data, ref index),
            2 => ReadValue16Bit(data, ref index),
            3 => ReadValue24Bit(data, ref index),
            4 => ReadValue32Bit(data, ref index),
            _ => throw new InvalidDataException("Cannot read bytes per sample of " + bytesPerSample)
        };
    }
    
    private static WavePeak2 CalculatePeak(float[] chunk, int count)
    {
        if (count == 0) return new WavePeak2();
        
        var max = chunk[0];
        var min = chunk[0];
        for (var i = 1; i < count; i++)
        {
            var value = chunk[i];
            if (value > max) max = value;
            else if (value < min) min = value;
        }
        return new WavePeak2((short)(short.MaxValue * max), (short)(short.MaxValue * min));
    }
    
    private double GetSampleAndChannelScale()
    {
        return (1.0 / Math.Pow(2.0, _header.BytesPerSample * 8 - 1)) / _header.NumberOfChannels;
    }
    
    private static int ReadValue8Bit(byte[] data, ref int index)
    {
        int result = sbyte.MinValue + data[index];
        index += 1;
        return result;
    }
    
    private static int ReadValue16Bit(byte[] data, ref int index)
    {
        int result = (short)((data[index]) | (data[index + 1] << 8));
        index += 2;
        return result;
    }
    
    private static int ReadValue24Bit(byte[] data, ref int index)
    {
        int result = ((data[index] << 8) | (data[index + 1] << 16) | (data[index + 2] << 24)) >> 8;
        index += 3;
        return result;
    }
    
    private static int ReadValue32Bit(byte[] data, ref int index)
    {
        int result = (data[index]) | (data[index + 1] << 8) | (data[index + 2] << 16) | (data[index + 3] << 24);
        index += 4;
        return result;
    }
}
