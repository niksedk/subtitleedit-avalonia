using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.Media.Optimized;

public static class BinarySpectrogramFormat
{
    private const uint Magic = 0x53504543; // "SPEC"
    private const int Version = 1;
    private const string FileName = "spectrogram.bin";
    
    public static string GetFilePath(string spectrogramDirectory) => Path.Combine(spectrogramDirectory, FileName);
    
    public static bool Exists(string spectrogramDirectory) => File.Exists(GetFilePath(spectrogramDirectory));
    
    public static void Save(SKBitmap[] images, string spectrogramDirectory)
    {
        if (images.Length == 0) return;
        
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var memBefore = GC.GetTotalMemory(false);
        
        string filePath = GetFilePath(spectrogramDirectory);
        int width = images[0].Width;
        int height = images[0].Height;
        int bytesPerChunk = width * height * 4; // RGBA
        
        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 
            bufferSize: 1024 * 1024, // 1MB buffer for better sequential write performance
            FileOptions.SequentialScan);
        
        // Header (20 bytes)
        Span<byte> header = stackalloc byte[20];
        BitConverter.TryWriteBytes(header, Magic);
        BitConverter.TryWriteBytes(header.Slice(4), Version);
        BitConverter.TryWriteBytes(header.Slice(8), images.Length);
        BitConverter.TryWriteBytes(header.Slice(12), width);
        BitConverter.TryWriteBytes(header.Slice(16), height);
        stream.Write(header);
        
        for (int i = 0; i < images.Length; i++)
        {
            stream.Write(images[i].GetPixelSpan());
        }
        
        sw.Stop();
        var memAfter = GC.GetTotalMemory(false);
        var fileSizeMB = new FileInfo(filePath).Length / (1024.0 * 1024.0);
        
        System.Diagnostics.Debug.WriteLine($"[PERF] Binary spectrogram SAVE: {images.Length} chunks in {sw.ElapsedMilliseconds}ms");
        System.Diagnostics.Debug.WriteLine($"[PERF] Binary spectrogram SIZE: {fileSizeMB:F1}MB ({bytesPerChunk / 1024}KB/chunk)");
        System.Diagnostics.Debug.WriteLine($"[MEM] Binary save: {(memAfter - memBefore) / (1024.0 * 1024.0):F1}MB allocated during save");
    }
    
    public static SKBitmap[] Load(string spectrogramDirectory)
    {
        string filePath = GetFilePath(spectrogramDirectory);
        if (!File.Exists(filePath))
            return Array.Empty<SKBitmap>();
        
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var memBefore = GC.GetTotalMemory(false);
        
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 1024 * 1024, // 1MB buffer
            FileOptions.SequentialScan);
        
        // Read header (20 bytes: magic + version + chunkCount + width + height)
        Span<byte> header = stackalloc byte[20];
        stream.ReadExactly(header);
        
        uint magic = BitConverter.ToUInt32(header);
        if (magic != Magic)
            throw new InvalidDataException($"Invalid spectrogram file: expected magic {Magic:X}, got {magic:X}");
        
        int version = BitConverter.ToInt32(header.Slice(4));
        if (version != Version)
            throw new InvalidDataException($"Unsupported spectrogram version: {version}");
        
        int chunkCount = BitConverter.ToInt32(header.Slice(8));
        int width = BitConverter.ToInt32(header.Slice(12));
        int height = BitConverter.ToInt32(header.Slice(16));
        
        var images = new SKBitmap[chunkCount];
        
        for (int i = 0; i < chunkCount; i++)
        {
            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            stream.ReadExactly(bitmap.GetPixelSpan());
            images[i] = bitmap;
        }
        
        sw.Stop();
        var memAfter = GC.GetTotalMemory(false);
        
        System.Diagnostics.Debug.WriteLine($"[PERF] Binary spectrogram LOAD: {chunkCount} chunks in {sw.ElapsedMilliseconds}ms");
        System.Diagnostics.Debug.WriteLine($"[MEM] Binary load: {(memAfter - memBefore) / (1024.0 * 1024.0):F1}MB allocated");
        
        return images;
    }
    
    public static long GetMemoryEstimate(int chunkCount, int width = 1024, int height = 128)
    {
        // Each chunk: width * height * 4 bytes (RGBA)
        // Plus SKBitmap overhead (~100 bytes per bitmap)
        return (long)chunkCount * (width * height * 4 + 100);
    }
}
