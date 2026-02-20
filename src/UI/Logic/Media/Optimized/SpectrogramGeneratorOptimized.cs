using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System;
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
    private const int StreamingBufferSize = 32 * 1024 * 1024; // 32MB buffer for streaming reads
    
    private readonly WaveHeader2 _header;
    private readonly Stream _stream;
    private readonly ThreadLocal<SpectrogramDrawerOptimized> _drawerPool = new(() => new SpectrogramDrawerOptimized(FftSize));
    
    public SpectrogramGeneratorOptimized(Stream stream, WaveHeader2 header)
    {
        _stream = stream;
        _header = header;
    }
    
    public SpectrogramData2 GenerateSpectrogram(int delayInMilliseconds, string spectrogramDirectory, CancellationToken token)
    {
        double sampleDuration = (double)FftSize / _header.SampleRate;

        // Check if spectrogram already exists and is valid
        if (Configuration.Settings.VideoControls.UseExperimentalRenderer && BinarySpectrogramFormat.Exists(spectrogramDirectory))
        {
            // If exists, try to load it instead of regenerating
            try 
            {
                var loader = BinarySpectrogramFormat.CreateMemoryMappedLoader(spectrogramDirectory);
                if (loader != null)
                {
                    System.Diagnostics.Debug.WriteLine("[PERF] Spectrogram already exists (memory mapped), skipping generation");
                    return new SpectrogramData2(FftSize, ImageWidth, sampleDuration, new BinarySpectrogramFormat.SpectrogramImageList(loader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WARN] Failed to load existing spectrogram: {ex.Message}. Regenerating.");
            }
        }

        var totalSw = System.Diagnostics.Stopwatch.StartNew();
        
        int delaySampleCount = (int)(_header.SampleRate * (delayInMilliseconds / TimeCode.BaseUnit));
        delaySampleCount = Math.Max(delaySampleCount, 0);
        
        int chunkSampleCount = FftSize * ImageWidth; // Samples per spectrogram image
        long fileSampleCount = _header.LengthInSamples;
        int chunkCount = (int)Math.Ceiling((double)(fileSampleCount + delaySampleCount) / chunkSampleCount);
        
        Directory.CreateDirectory(spectrogramDirectory);
        
        // Use streaming approach: read audio in batches, process in parallel, save immediately
        var processSw = System.Diagnostics.Stopwatch.StartNew();
        
        // Calculate how many chunks we can process per batch based on buffer size
        int bytesPerChunk = chunkSampleCount * _header.BlockAlign;
        int chunksPerBatch = Math.Max(1, StreamingBufferSize / bytesPerChunk);
        chunksPerBatch = Math.Min(chunksPerBatch, Environment.ProcessorCount * 4); // Limit batch size
        
        var images = new SKBitmap[chunkCount];
        double sampleAndChannelScale = WaveDataReader.GetSampleAndChannelScale(_header.BytesPerSample, _header.NumberOfChannels);
        
        _stream.Seek(_header.DataStartPosition, SeekOrigin.Begin);
        
        int processedChunks = 0;
        long fileSampleOffset = -delaySampleCount;
        
        // Rent a buffer for streaming reads
        var buffer = WaveDataReader.RentBuffer(StreamingBufferSize);
        try
        {
            while (processedChunks < chunkCount && !token.IsCancellationRequested)
            {
                int batchSize = Math.Min(chunksPerBatch, chunkCount - processedChunks);
                
                // Read audio data for this batch
                var batchSamples = ReadBatchSamples(
                    buffer, batchSize, chunkSampleCount, 
                    ref fileSampleOffset, fileSampleCount, sampleAndChannelScale);
                
                if (token.IsCancellationRequested) break;
                
                // Process batch in parallel
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken = token
                };
                
                int batchStartIndex = processedChunks;
                try
                {
                    Parallel.For(0, batchSize, parallelOptions, i =>
                    {
                        var drawer = _drawerPool.Value!;
                        images[batchStartIndex + i] = drawer.Draw(batchSamples[i]);
                    });
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                
                processedChunks += batchSize;
                
                System.Diagnostics.Debug.WriteLine($"[PERF] Spectrogram: Processed {processedChunks}/{chunkCount} chunks");
            }
        }
        finally
        {
            WaveDataReader.ReturnBuffer(buffer);
        }
        
        processSw.Stop();
        System.Diagnostics.Debug.WriteLine($"[PERF] Optimized: Generated {chunkCount} spectrograms in {processSw.ElapsedMilliseconds}ms ({Environment.ProcessorCount} threads)");
        
        if (token.IsCancellationRequested) 
            return CreateEmptyResult(sampleDuration);
        
        // Filter out null images if cancelled mid-way
        var validImages = new SKBitmap[processedChunks];
        Array.Copy(images, validImages, processedChunks);
        
        // Save images
        var saveSw = System.Diagnostics.Stopwatch.StartNew();
        
        if (Configuration.Settings.VideoControls.UseExperimentalRenderer)
        {
            BinarySpectrogramFormat.Save(validImages, spectrogramDirectory);
        }
        else
        {
            // Legacy JPEG format
            var saveOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 4,
                CancellationToken = token
            };
            
            try
            {
                Parallel.For(0, validImages.Length, saveOptions, chunkIndex =>
                {
                    string imagePath = Path.Combine(spectrogramDirectory, chunkIndex + ".jpg");
                    using var stream = File.OpenWrite(imagePath);
                    using var imageData = validImages[chunkIndex].Encode(SKEncodedImageFormat.Jpeg, 50);
                    imageData.SaveTo(stream);
                });
            }
            catch (OperationCanceledException)
            {
                return CreateEmptyResult(sampleDuration);
            }
        }
        saveSw.Stop();
        System.Diagnostics.Debug.WriteLine($"[PERF] Optimized: Saved {validImages.Length} images in {saveSw.ElapsedMilliseconds}ms");
        
        // Save metadata
        SaveMetadata(spectrogramDirectory, chunkSampleCount, token);
        
        totalSw.Stop();
        System.Diagnostics.Debug.WriteLine($"[PERF] Optimized TOTAL: {chunkCount} chunks in {totalSw.ElapsedMilliseconds}ms (avg: {totalSw.ElapsedMilliseconds / (double)chunkCount:F2}ms/chunk, {_header.LengthInSeconds:F1}s audio)");
        
        return new SpectrogramData2(FftSize, ImageWidth, sampleDuration, validImages);
    }
    
    /// <summary>
    /// Reads a batch of sample arrays for parallel processing.
    /// </summary>
    private double[][] ReadBatchSamples(
        byte[] buffer, int batchSize, int chunkSampleCount,
        ref long fileSampleOffset, long fileSampleCount, double sampleAndChannelScale)
    {
        var batchSamples = new double[batchSize][];
        int blockAlign = _header.BlockAlign;
        int bytesPerSample = _header.BytesPerSample;
        int numberOfChannels = _header.NumberOfChannels;
        
        for (int chunkIdx = 0; chunkIdx < batchSize; chunkIdx++)
        {
            var chunkSamples = new double[chunkSampleCount];
            batchSamples[chunkIdx] = chunkSamples;
            
            int sampleIndex = 0;
            
            // Handle delay (padding at start)
            if (fileSampleOffset < 0)
            {
                int paddingSamples = (int)Math.Min(-fileSampleOffset, chunkSampleCount);
                // Samples are already zero-initialized
                sampleIndex = paddingSamples;
                fileSampleOffset += paddingSamples;
            }
            
            // Read from file
            int samplesToRead = chunkSampleCount - sampleIndex;
            long availableSamples = fileSampleCount - Math.Max(0, fileSampleOffset);
            samplesToRead = (int)Math.Min(samplesToRead, availableSamples);
            
            if (samplesToRead > 0)
            {
                int bytesToRead = samplesToRead * blockAlign;
                int bytesRead = _stream.Read(buffer, 0, bytesToRead);
                
                int bufferOffset = 0;
                while (bufferOffset < bytesRead && sampleIndex < chunkSampleCount)
                {
                    double value = 0.0;
                    for (int iChannel = 0; iChannel < numberOfChannels; iChannel++)
                    {
                        if (bufferOffset + bytesPerSample <= bytesRead)
                        {
                            value += WaveDataReader.ReadSampleValue(buffer, ref bufferOffset, bytesPerSample);
                        }
                    }
                    chunkSamples[sampleIndex++] = value * sampleAndChannelScale;
                }
                
                fileSampleOffset += samplesToRead;
            }
            
            // Remaining samples stay zero (end padding)
        }
        
        return batchSamples;
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
    
    private SpectrogramData2 CreateEmptyResult(double sampleDuration)
    {
        return new SpectrogramData2(FftSize, ImageWidth, sampleDuration, Array.Empty<SKBitmap>());
    }
}
