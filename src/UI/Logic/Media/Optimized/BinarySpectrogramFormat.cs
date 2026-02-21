using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.Media.Optimized;

/// <summary>
/// Binary spectrogram format with random access and LRU caching.
/// Supports both full load (legacy) and lazy loading with cache.
/// </summary>
public static class BinarySpectrogramFormat
{
    private const uint Magic = 0x53504543; // "SPEC"
    private const int Version = 1;
    private const int HeaderSize = 20;
    private const string FileName = "spectrogram.bin";
    
    public interface ISpectrogramLoader : IDisposable
    {
        SKBitmap GetChunk(int index);
        int ChunkCount { get; }
        int Width { get; }
        int Height { get; }
    }
    
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
        
        int tightRowBytes = width * 4; // RGBA, tightly packed on disk
        for (int i = 0; i < images.Length; i++)
        {
            var bitmap = images[i];
            var pixelSpan = bitmap.GetPixelSpan();
            int rowBytes = bitmap.RowBytes;

            if (rowBytes == tightRowBytes)
            {
                stream.Write(pixelSpan);
            }
            else
            {
                // Write row-by-row to avoid per-row padding in the file
                for (int y = 0; y < height; y++)
                {
                    stream.Write(pixelSpan.Slice(y * rowBytes, tightRowBytes));
                }
            }
        }
        
        sw.Stop();
        var memAfter = GC.GetTotalMemory(false);
        var fileSizeMB = new FileInfo(filePath).Length / (1024.0 * 1024.0);
        
        System.Diagnostics.Debug.WriteLine($"[PERF] Binary spectrogram SAVE: {images.Length} chunks in {sw.ElapsedMilliseconds}ms");
        System.Diagnostics.Debug.WriteLine($"[PERF] Binary spectrogram SIZE: {fileSizeMB:F1}MB ({bytesPerChunk / 1024}KB/chunk)");
        System.Diagnostics.Debug.WriteLine($"[MEM] Binary save: {(memAfter - memBefore) / (1024.0 * 1024.0):F1}MB allocated during save");
    }
    
    /// <summary>
    /// Loads all spectrogram images into memory (legacy behavior).
    /// Use CreateLazyLoader for memory-efficient access.
    /// </summary>
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
        
        var header = ReadHeader(stream);
        var images = new SKBitmap[header.ChunkCount];
        int tightRowBytes = header.Width * 4;

        for (int i = 0; i < header.ChunkCount; i++)
        {
            var bitmap = new SKBitmap(header.Width, header.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            int rowBytes = bitmap.RowBytes;

            if (rowBytes == tightRowBytes)
            {
                stream.ReadExactly(bitmap.GetPixelSpan());
            }
            else
            {
                // Read row-by-row when bitmap has per-row padding
                var pixelSpan = bitmap.GetPixelSpan();
                for (int y = 0; y < header.Height; y++)
                {
                    stream.ReadExactly(pixelSpan.Slice(y * rowBytes, tightRowBytes));
                }
            }

            images[i] = bitmap;
        }
        
        sw.Stop();
        var memAfter = GC.GetTotalMemory(false);
        
        System.Diagnostics.Debug.WriteLine($"[PERF] Binary spectrogram LOAD: {header.ChunkCount} chunks in {sw.ElapsedMilliseconds}ms");
        System.Diagnostics.Debug.WriteLine($"[MEM] Binary load: {(memAfter - memBefore) / (1024.0 * 1024.0):F1}MB allocated");
        
        return images;
    }
    
    /// <summary>
    /// Creates a lazy loader with LRU cache for memory-efficient spectrogram access.
    /// </summary>
    /// <param name="spectrogramDirectory">Directory containing spectrogram.bin</param>
    /// <param name="maxCachedChunks">Maximum number of chunks to keep in memory (default: 128 = ~64MB)</param>
    public static SpectrogramLazyLoader? CreateLazyLoader(string spectrogramDirectory, int maxCachedChunks = 128)
    {
        string filePath = GetFilePath(spectrogramDirectory);
        if (!File.Exists(filePath))
            return null;
        
        return new SpectrogramLazyLoader(filePath, maxCachedChunks);
    }

    /// <summary>
    /// Creates a memory-mapped loader for instant access and OS-managed paging.
    /// This is the most efficient way to access large spectrograms.
    /// </summary>
    public static ISpectrogramLoader? CreateMemoryMappedLoader(string spectrogramDirectory)
    {
        string filePath = GetFilePath(spectrogramDirectory);
        if (!File.Exists(filePath))
            return null;
            
        try 
        {
            return new MemoryMappedSpectrogramLoader(filePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WARN] Failed to create memory mapped loader: {ex.Message}. Falling back to lazy loader.");
            return CreateLazyLoader(spectrogramDirectory);
        }
    }
    
    public static long GetMemoryEstimate(int chunkCount, int width = 1024, int height = 128)
    {
        // Each chunk: width * height * 4 bytes (RGBA)
        // Plus SKBitmap overhead (~100 bytes per bitmap)
        return (long)chunkCount * (width * height * 4 + 100);
    }
    
    private static SpectrogramHeader ReadHeader(Stream stream)
    {
        Span<byte> header = stackalloc byte[HeaderSize];
        stream.ReadExactly(header);
        
        uint magic = BitConverter.ToUInt32(header);
        if (magic != Magic)
            throw new InvalidDataException($"Invalid spectrogram file: expected magic {Magic:X}, got {magic:X}");
        
        int version = BitConverter.ToInt32(header.Slice(4));
        if (version != Version)
            throw new InvalidDataException($"Unsupported spectrogram version: {version}");
        
        return new SpectrogramHeader
        {
            ChunkCount = BitConverter.ToInt32(header.Slice(8)),
            Width = BitConverter.ToInt32(header.Slice(12)),
            Height = BitConverter.ToInt32(header.Slice(16))
        };
    }
    
    private struct SpectrogramHeader
    {
        public int ChunkCount;
        public int Width;
        public int Height;
        public int BytesPerChunk => Width * Height * 4;
    }
    
    /// <summary>
    /// Lazy loader with LRU cache for memory-efficient spectrogram access.
    /// Thread-safe for concurrent reads.
    /// </summary>
    public sealed class SpectrogramLazyLoader : ISpectrogramLoader
    {
        private readonly string _filePath;
        private readonly int _maxCachedChunks;
        private readonly SpectrogramHeader _header;
        private readonly Lock _lock = new();
        
        // LRU cache: LinkedList for order, Dictionary for O(1) lookup
        private readonly Dictionary<int, LinkedListNode<CacheEntry>> _cacheMap;
        private readonly LinkedList<CacheEntry> _cacheOrder;
        
        private FileStream? _stream;
        private bool _disposed;
        
        public int ChunkCount => _header.ChunkCount;
        public int Width => _header.Width;
        public int Height => _header.Height;
        
        internal SpectrogramLazyLoader(string filePath, int maxCachedChunks)
        {
            _filePath = filePath;
            _maxCachedChunks = maxCachedChunks;
            _cacheMap = new Dictionary<int, LinkedListNode<CacheEntry>>(maxCachedChunks);
            _cacheOrder = new LinkedList<CacheEntry>();
            
            // Read header
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _header = ReadHeader(stream);
            
            System.Diagnostics.Debug.WriteLine($"[PERF] SpectrogramLazyLoader: {_header.ChunkCount} chunks, cache size {maxCachedChunks} (~{maxCachedChunks * _header.BytesPerChunk / (1024 * 1024)}MB max)");
        }
        
        /// <summary>
        /// Gets a spectrogram chunk by index. Uses LRU cache.
        /// </summary>
        public SKBitmap GetChunk(int index)
        {
            if (index < 0 || index >= _header.ChunkCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            lock (_lock)
            {
                // Check cache
                if (_cacheMap.TryGetValue(index, out var node))
                {
                    // Move to front (most recently used)
                    _cacheOrder.Remove(node);
                    _cacheOrder.AddFirst(node);
                    return node.Value.Bitmap;
                }
                
                // Load from file
                var bitmap = LoadChunkFromFile(index);
                
                // Add to cache
                var entry = new CacheEntry { Index = index, Bitmap = bitmap };
                var newNode = _cacheOrder.AddFirst(entry);
                _cacheMap[index] = newNode;
                
                // Evict if over capacity
                while (_cacheOrder.Count > _maxCachedChunks)
                {
                    var lastNode = _cacheOrder.Last!;
                    _cacheOrder.RemoveLast();
                    _cacheMap.Remove(lastNode.Value.Index);
                    lastNode.Value.Bitmap.Dispose();
                }
                
                return bitmap;
            }
        }
        
        /// <summary>
        /// Preloads a range of chunks into cache (for smooth scrolling).
        /// </summary>
        public void PreloadRange(int startIndex, int count)
        {
            int endIndex = Math.Min(startIndex + count, _header.ChunkCount);
            for (int i = startIndex; i < endIndex; i++)
            {
                if (!_cacheMap.ContainsKey(i))
                {
                    GetChunk(i); // This will load and cache
                }
            }
        }
        
        private SKBitmap LoadChunkFromFile(int index)
        {
            _stream ??= new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: _header.BytesPerChunk,
                FileOptions.RandomAccess);

            long offset = HeaderSize + (long)index * _header.BytesPerChunk;
            _stream.Seek(offset, SeekOrigin.Begin);

            var bitmap = new SKBitmap(_header.Width, _header.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            int rowBytes = bitmap.RowBytes;
            int tightRowBytes = _header.Width * 4;

            if (rowBytes == tightRowBytes)
            {
                _stream.ReadExactly(bitmap.GetPixelSpan());
            }
            else
            {
                var pixelSpan = bitmap.GetPixelSpan();
                for (int y = 0; y < _header.Height; y++)
                {
                    _stream.ReadExactly(pixelSpan.Slice(y * rowBytes, tightRowBytes));
                }
            }

            return bitmap;
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            lock (_lock)
            {
                _stream?.Dispose();
                _stream = null;
                
                foreach (var entry in _cacheOrder)
                {
                    entry.Bitmap.Dispose();
                }
                _cacheOrder.Clear();
                _cacheMap.Clear();
            }
        }
        
        private struct CacheEntry
        {
            public int Index;
            public SKBitmap Bitmap;
        }
    }
    
    /// <summary>
    /// Memory-mapped loader for zero-copy access to spectrogram data.
    /// Relies on OS paging for memory management.
    /// </summary>
    public sealed class MemoryMappedSpectrogramLoader : ISpectrogramLoader
    {
        private readonly MemoryMappedFile _mmf;
        private readonly MemoryMappedViewAccessor _accessor;
        private readonly SpectrogramHeader _header;
        private bool _disposed;
        
        public int ChunkCount => _header.ChunkCount;
        public int Width => _header.Width;
        public int Height => _header.Height;
        
        internal MemoryMappedSpectrogramLoader(string filePath)
        {
            _mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            _accessor = _mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            
            // Read header directly from accessor
            byte[] headerBytes = new byte[HeaderSize];
            _accessor.ReadArray(0, headerBytes, 0, HeaderSize);
            
            uint magic = BitConverter.ToUInt32(headerBytes, 0);
            if (magic != Magic)
                throw new InvalidDataException($"Invalid spectrogram file: expected magic {Magic:X}, got {magic:X}");
            
            int version = BitConverter.ToInt32(headerBytes, 4);
            if (version != Version)
                throw new InvalidDataException($"Unsupported spectrogram version: {version}");
                
            _header = new SpectrogramHeader
            {
                ChunkCount = BitConverter.ToInt32(headerBytes, 8),
                Width = BitConverter.ToInt32(headerBytes, 12),
                Height = BitConverter.ToInt32(headerBytes, 16)
            };
            
            System.Diagnostics.Debug.WriteLine($"[PERF] MemoryMappedSpectrogramLoader: {_header.ChunkCount} chunks, mapped to memory");
        }
        
        public unsafe SKBitmap GetChunk(int index)
        {
            if (index < 0 || index >= _header.ChunkCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            long offset = HeaderSize + (long)index * _header.BytesPerChunk;

            // Bounds check against mapped view capacity
            long requiredEnd = offset + _header.BytesPerChunk;
            if (requiredEnd > (long)_accessor.SafeMemoryMappedViewHandle.ByteLength)
                throw new InvalidDataException($"Spectrogram file is truncated: need {requiredEnd} bytes but file has {_accessor.SafeMemoryMappedViewHandle.ByteLength}");

            var bitmap = new SKBitmap(_header.Width, _header.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            int rowBytes = bitmap.RowBytes;
            int tightRowBytes = _header.Width * 4;

            byte* ptr = null;
            _accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
            try
            {
                var destPixels = (byte*)bitmap.GetPixels();

                if (rowBytes == tightRowBytes)
                {
                    Buffer.MemoryCopy(ptr + offset, destPixels, _header.BytesPerChunk, _header.BytesPerChunk);
                }
                else
                {
                    // Copy row-by-row when bitmap has per-row padding
                    for (int y = 0; y < _header.Height; y++)
                    {
                        Buffer.MemoryCopy(
                            ptr + offset + y * tightRowBytes,
                            destPixels + y * rowBytes,
                            tightRowBytes, tightRowBytes);
                    }
                }
            }
            finally
            {
                _accessor.SafeMemoryMappedViewHandle.ReleasePointer();
            }

            return bitmap;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            _accessor.Dispose();
            _mmf.Dispose();
        }
    }

    /// <summary>
    /// Wrapper that adapts ISpectrogramLoader to IList&lt;SKBitmap&gt; interface.
    /// Allows seamless integration with existing code expecting a list of bitmaps.
    /// </summary>
    public class SpectrogramImageList : IList<SKBitmap>, IDisposable
    {
        private readonly ISpectrogramLoader _loader;
        
        public SpectrogramImageList(ISpectrogramLoader loader)
        {
            _loader = loader;
        }

        public SKBitmap this[int index] 
        { 
            get => _loader.GetChunk(index); 
            set => throw new NotSupportedException("List is read-only"); 
        }

        public int Count => _loader.ChunkCount;
        public bool IsReadOnly => true;

        public void Dispose() => _loader.Dispose();

        public void Add(SKBitmap item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Contains(SKBitmap item) => false;
        public void CopyTo(SKBitmap[] array, int arrayIndex) => throw new NotSupportedException();
        public IEnumerator<SKBitmap> GetEnumerator() 
        {
            // Warning: Enumerating the whole list will cause thrashing of the LRU cache
            // and likely poor performance. Avoid if possible.
            for (int i = 0; i < Count; i++)
                yield return _loader.GetChunk(i);
        }
        public int IndexOf(SKBitmap item) => -1;
        public void Insert(int index, SKBitmap item) => throw new NotSupportedException();
        public bool Remove(SKBitmap item) => throw new NotSupportedException();
        public void RemoveAt(int index) => throw new NotSupportedException();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
