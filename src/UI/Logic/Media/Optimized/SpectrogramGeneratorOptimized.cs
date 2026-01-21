using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.Media.Optimized;

public class SpectrogramGeneratorOptimized
{
    private const int FftSize = 256;
    private const int ImageWidth = 1024;
    
    private readonly WaveHeader2 _header;
    private readonly Stream _stream;
    
    public SpectrogramGeneratorOptimized(Stream stream, WaveHeader2 header)
    {
        _stream = stream;
        _header = header;
    }
    
    public SpectrogramData2 GenerateSpectrogram(int delayInMilliseconds, string spectrogramDirectory, CancellationToken token)
    {
        var totalSw = System.Diagnostics.Stopwatch.StartNew();
        
        int delaySampleCount = (int)(_header.SampleRate * (delayInMilliseconds / TimeCode.BaseUnit));
        delaySampleCount = Math.Max(delaySampleCount, 0);
        
        int chunkSampleCount = FftSize * ImageWidth;
        long fileSampleCount = _header.LengthInSamples;
        int chunkCount = (int)Math.Ceiling((double)(fileSampleCount + delaySampleCount) / chunkSampleCount);
        
        Directory.CreateDirectory(spectrogramDirectory);
        
        // Phase 1: Read all samples into memory (sequential I/O is faster)
        var readSw = System.Diagnostics.Stopwatch.StartNew();
        var allSamples = ReadAllSamples(delaySampleCount, chunkSampleCount, chunkCount);
        readSw.Stop();
        System.Diagnostics.Debug.WriteLine($"[PERF] Optimized: Read {chunkCount} chunks ({allSamples.Length} samples) in {readSw.ElapsedMilliseconds}ms");
        
        if (token.IsCancellationRequested) return CreateEmptyResult();
        
        // Phase 2: Generate spectrograms in parallel
        var processSw = System.Diagnostics.Stopwatch.StartNew();
        var images = new SKBitmap[chunkCount];
        var parallelOptions = new ParallelOptions 
        { 
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = token 
        };
        
        try
        {
            Parallel.For(0, chunkCount, parallelOptions, chunkIndex =>
            {
                var drawer = new SpectrogramDrawerOptimized(FftSize);
                var chunkSamples = ExtractChunkSamples(allSamples, chunkIndex, chunkSampleCount);
                images[chunkIndex] = drawer.Draw(chunkSamples);
            });
        }
        catch (OperationCanceledException)
        {
            return CreateEmptyResult();
        }
        processSw.Stop();
        System.Diagnostics.Debug.WriteLine($"[PERF] Optimized: Generated {chunkCount} spectrograms in {processSw.ElapsedMilliseconds}ms ({Environment.ProcessorCount} threads)");
        
        if (token.IsCancellationRequested) return CreateEmptyResult();
        
        // Phase 3: Save images
        var saveSw = System.Diagnostics.Stopwatch.StartNew();
        
        if (Configuration.Settings.VideoControls.UseBinarySpectrogramFormat)
        {
            // Binary format: single file, no encoding overhead
            BinarySpectrogramFormat.Save(images, spectrogramDirectory);
        }
        else
        {
            // Legacy JPEG format: multiple files
            var saveOptions = new ParallelOptions 
            { 
                MaxDegreeOfParallelism = 4,
                CancellationToken = token 
            };
            
            try
            {
                Parallel.For(0, chunkCount, saveOptions, chunkIndex =>
                {
                    string imagePath = Path.Combine(spectrogramDirectory, chunkIndex + ".jpg");
                    using var stream = File.OpenWrite(imagePath);
                    using var imageData = images[chunkIndex].Encode(SKEncodedImageFormat.Jpeg, 50);
                    imageData.SaveTo(stream);
                });
            }
            catch (OperationCanceledException)
            {
                return CreateEmptyResult();
            }
        }
        saveSw.Stop();
        System.Diagnostics.Debug.WriteLine($"[PERF] Optimized: Saved {chunkCount} images in {saveSw.ElapsedMilliseconds}ms");
        
        // Save metadata
        SaveMetadata(spectrogramDirectory, chunkSampleCount, token);
        
        totalSw.Stop();
        System.Diagnostics.Debug.WriteLine($"[PERF] Optimized TOTAL: {chunkCount} chunks in {totalSw.ElapsedMilliseconds}ms (avg: {totalSw.ElapsedMilliseconds / (double)chunkCount:F2}ms/chunk, {_header.LengthInSeconds:F1}s audio)");
        
        double sampleDuration = (double)FftSize / _header.SampleRate;
        return new SpectrogramData2(FftSize, ImageWidth, sampleDuration, images);
    }
    
    private double[] ReadAllSamples(int delaySampleCount, int chunkSampleCount, int chunkCount)
    {
        long fileSampleCount = _header.LengthInSamples;
        long fileSampleOffset = -delaySampleCount;
        int totalSamples = chunkSampleCount * chunkCount;
        var allSamples = new double[totalSamples];
        
        var readSampleDataValue = GetSampleDataReader();
        double sampleAndChannelScale = GetSampleAndChannelScale();
        byte[] data = new byte[chunkSampleCount * _header.BlockAlign];
        
        _stream.Seek(_header.DataStartPosition, SeekOrigin.Begin);
        
        if (fileSampleOffset > 0)
        {
            _stream.Seek(fileSampleOffset * _header.BlockAlign, SeekOrigin.Current);
        }
        
        int globalSampleOffset = 0;
        
        for (var iChunk = 0; iChunk < chunkCount; iChunk++)
        {
            int startPaddingSampleCount = 0;
            if (fileSampleOffset < 0)
            {
                startPaddingSampleCount = (int)Math.Min(-fileSampleOffset, chunkSampleCount);
                fileSampleOffset += startPaddingSampleCount;
            }
            
            long fileSamplesRemaining = fileSampleCount - Math.Max(fileSampleOffset, 0);
            int fileReadSampleCount = (int)Math.Min(fileSamplesRemaining, chunkSampleCount - startPaddingSampleCount);
            int endPaddingSampleCount = chunkSampleCount - startPaddingSampleCount - fileReadSampleCount;
            
            // Start padding (zeros)
            globalSampleOffset += startPaddingSampleCount;
            
            // Read samples
            if (fileReadSampleCount > 0)
            {
                int fileReadByteCount = fileReadSampleCount * _header.BlockAlign;
                _ = _stream.Read(data, 0, fileReadByteCount);
                fileSampleOffset += fileReadSampleCount;
                
                int dataByteOffset = 0;
                while (dataByteOffset < fileReadByteCount)
                {
                    double value = 0D;
                    for (int iChannel = 0; iChannel < _header.NumberOfChannels; iChannel++)
                    {
                        value += readSampleDataValue(data, ref dataByteOffset);
                    }
                    allSamples[globalSampleOffset] = value * sampleAndChannelScale;
                    globalSampleOffset++;
                }
            }
            
            // End padding (zeros already default)
            globalSampleOffset += endPaddingSampleCount;
        }
        
        return allSamples;
    }
    
    private static double[] ExtractChunkSamples(double[] allSamples, int chunkIndex, int chunkSampleCount)
    {
        var chunkSamples = new double[chunkSampleCount];
        int startIndex = chunkIndex * chunkSampleCount;
        Array.Copy(allSamples, startIndex, chunkSamples, 0, chunkSampleCount);
        return chunkSamples;
    }
    
    private void SaveMetadata(string spectrogramDirectory, int chunkSampleCount, CancellationToken token)
    {
        if (token.IsCancellationRequested) return;
        
        var doc = new XmlDocument();
        var culture = CultureInfo.InvariantCulture;
        double sampleDuration = (double)FftSize / _header.SampleRate;
        doc.LoadXml("<SpectrogramInfo><SampleDuration/><NFFT/><ImageWidth/><SecondsPerImage/></SpectrogramInfo>");
        
        if (doc.DocumentElement != null)
        {
            doc.DocumentElement.SelectSingleNode("SampleDuration")!.InnerText = sampleDuration.ToString(culture);
            doc.DocumentElement.SelectSingleNode("NFFT")!.InnerText = FftSize.ToString(culture);
            doc.DocumentElement.SelectSingleNode("ImageWidth")!.InnerText = ImageWidth.ToString(culture);
            doc.DocumentElement.SelectSingleNode("SecondsPerImage")!.InnerText = ((double)chunkSampleCount / _header.SampleRate).ToString(culture);
        }
        
        doc.Save(Path.Combine(spectrogramDirectory, "Info.xml"));
    }
    
    private SpectrogramData2 CreateEmptyResult()
    {
        double sampleDuration = (double)FftSize / _header.SampleRate;
        return new SpectrogramData2(FftSize, ImageWidth, sampleDuration, Array.Empty<SKBitmap>());
    }
    
    private delegate int ReadSampleDataValue(byte[] data, ref int index);
    
    private ReadSampleDataValue GetSampleDataReader()
    {
        return _header.BytesPerSample switch
        {
            1 => ReadValue8Bit,
            2 => ReadValue16Bit,
            3 => ReadValue24Bit,
            4 => ReadValue32Bit,
            _ => throw new InvalidDataException("Cannot read bits per sample of " + _header.BitsPerSample)
        };
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
