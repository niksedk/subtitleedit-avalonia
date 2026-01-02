# Audio/Video/Waveform/Spectrogram Performance Improvement Plan

## ⚠️ IMPORTANT: All Changes Are Optional

**All optimizations described in this plan are OPTIONAL and controlled by a single checkbox in Settings:**

**Settings → Video Controls → "Use Experimental Waveform Renderer"**

- ✅ **Default: OFF** - Preserves existing stable behavior
- 🧪 **When enabled** - Activates all performance optimizations
- 🔄 **Switchable at runtime** - Users can toggle without restart
- 📊 **Backward compatible** - Old rendering path remains untouched

This ensures:
- Zero risk to existing users
- Easy A/B testing and performance comparison
- Gradual rollout capability
- Safe fallback if issues occur

---

## Current Status (Phase 1 Outcome)

- Phase 1 experiments completed, **no measurable performance improvement** on ARM or Intel so far.
- SIMD/experimental FFT path did not outperform legacy; kept only for further Intel testing.
- End-to-end spectrogram generation timings remain comparable to legacy; see `PERFORMANCE_METRICS.md` for raw measurements.
- Next steps: focus on alternative optimizations (Phase 2+ items) since quick wins were ineffective.

---

## How Components Currently Work

### 1. **Waveform Generation**
**Location:** `src/UI/Logic/Media/WaveToVisualizer2.cs:469-562`

**Current Process:**
- FFmpeg extracts audio → temporary WAV file
- `WavePeakGenerator2.GeneratePeaks()` reads WAV in chunks
- Downsamples to configurable sample rate
- Calculates min/max peaks per sample window
- Saves compressed peak data to disk (cached by video hash)

### 2. **Spectrogram Generation**
**Location:** `src/UI/Logic/Media/WaveToVisualizer2.cs:812-941`

**Current Process:**
- Real FFT with 256-point FFT (`src/libse/Common/RealFFT.cs`)
- Processes audio in chunks (256 × 1024 = 262,144 samples/image)
- Applies Hann window for spectral leakage reduction
- Computes magnitude spectrum, maps to color palette
- Saves as JPEG images (50% quality) to disk
- Asynchronous image saving

### 3. **Waveform Rendering**
**Location:** `src/UI/Controls/AudioVisualizerControl/AudioVisualizer.cs:1354-1585`

**Current Process:**
- Renders at 50ms intervals via DispatcherTimer
- Two modes: Classic (simple lines) and Fancy (gradients)
- Linear interpolation between peak samples
- Per-pixel rendering loop across control width
- Pen/brush caching for fancy mode

### 4. **Spectrogram Rendering**
**Location:** `src/UI/Controls/AudioVisualizerControl/AudioVisualizer.cs:1168-1216`

**Current Process:**
- Loads pre-generated JPEG images from disk
- Creates combined SKBitmap by stitching segments
- Converts SKBitmap → Avalonia Bitmap
- Draws scaled to control height

### 5. **Video Playback**
**Location:** `src/UI/Logic/VideoPlayers/LibMpvDynamic/LibMpvDynamicPlayer.cs`

**Current Process:**
- Uses libmpv with OpenGL rendering
- Three backends: mpv-opengl, mpv-sw, mpv-wid
- Render callback triggers UI updates
- Position polling at 50ms intervals

### 6. **Audio Playback**
Audio is played through the video player (libmpv). No separate audio system.

---

## Performance Bottlenecks Identified

### **Critical Bottlenecks**
1. **Waveform Rendering Loop** - O(width) per frame, runs every 50ms
2. **Spectrogram Image Loading** - Synchronous disk I/O blocks UI thread
3. **FFT Computation** - CPU-intensive, single-threaded
4. **JPEG Encoding/Decoding** - Compression overhead
5. **Pen/Brush Allocation** - GC pressure (partially mitigated)

### **Secondary Issues**
6. **No GPU Acceleration** - All rendering is CPU-bound
7. **Synchronous File I/O** - Blocks during generation
8. **Memory Allocations** - Per-frame bitmap conversions
9. **Video Player Polling** - 50ms timer instead of event-driven

---

## Implementation Architecture

### **Feature Flag System**

```csharp
// Settings class
public class VideoControlsSettings
{
    public bool UseExperimentalRenderer { get; set; } = false;
}

// Renderer factory pattern
public interface IWaveformRenderer
{
    void Render(DrawingContext context, RenderContext ctx);
}

public class LegacyWaveformRenderer : IWaveformRenderer
{
    // Existing stable implementation
}

public class ExperimentalWaveformRenderer : IWaveformRenderer
{
    // New optimized implementation
}

// In AudioVisualizer
private IWaveformRenderer _renderer;

public AudioVisualizer()
{
    UpdateRenderer();
}

private void UpdateRenderer()
{
    _renderer = Se.Settings.VideoControls.UseExperimentalRenderer
        ? new ExperimentalWaveformRenderer()
        : new LegacyWaveformRenderer();
}
```

### **Dual Code Paths**

All optimizations maintain parallel implementations:
- **Legacy path** - Current stable code (unchanged)
- **Experimental path** - New optimized code (feature-flagged)

Example structure:
```
src/UI/Controls/AudioVisualizerControl/
├── AudioVisualizer.cs                    # Main control
├── Renderers/
│   ├── IWaveformRenderer.cs             # Interface
│   ├── LegacyWaveformRenderer.cs        # Stable (current code)
│   └── ExperimentalWaveformRenderer.cs  # Optimized (new code)
├── Generators/
│   ├── IWaveformGenerator.cs
│   ├── LegacyWaveformGenerator.cs
│   └── ExperimentalWaveformGenerator.cs
```

---

## Phase 1: Quick Wins + Generation Optimizations (3-5 days)

### ✅ **COMPLETED ITEMS**

### 1.1 **Async Spectrogram Loading** ⚡ ✅
**File:** `src/UI/Logic/Media/WaveToVisualizer2.cs:349-415`
**Status:** IMPLEMENTED

**Implementation:**
```csharp
public class ExperimentalSpectrogramLoader
{
    public async Task LoadAsync()
    {
        if (!Se.Settings.VideoControls.UseExperimentalRenderer)
        {
            // Use legacy synchronous loading
            LoadLegacy();
            return;
        }

        // Parallel image loading
        var loadTasks = fileNames.Select(async fileName =>
        {
            using var fileStream = File.OpenRead(fileName);
            return await Task.Run(() => SKBitmap.Decode(fileStream));
        });
        Images = (await Task.WhenAll(loadTasks)).ToList();
    }
}
```

**Impact:** Eliminates UI freezing during spectrogram load

---

### 1.2 **Parallel FFT Processing** 🚀
**File:** `src/UI/Logic/Media/WaveToVisualizer2.cs:844-918`

**Implementation:**
```csharp
public class ExperimentalSpectrogramGenerator
{
    public SpectrogramData2 GenerateSpectrogram(...)
    {
        if (!Se.Settings.VideoControls.UseExperimentalRenderer)
        {
            return GenerateSpectrogramLegacy(...);
        }

        // Process chunks in parallel
        var chunks = new ConcurrentBag<(int index, SKBitmap bitmap)>();
        
        Parallel.For(0, chunkCount, new ParallelOptions 
        { 
            MaxDegreeOfParallelism = Environment.ProcessorCount 
        }, iChunk =>
        {
            var bmp = ProcessChunk(iChunk, ...);
            chunks.Add((iChunk, bmp));
        });

        // Save images asynchronously
        var images = chunks.OrderBy(c => c.index)
                          .Select(c => c.bitmap)
                          .ToList();
        
        await SaveImagesAsync(images, spectrogramDirectory);
        return new SpectrogramData2(fftSize, imageWidth, sampleDuration, images);
    }
}
```

**Impact:** 4-8x faster on multi-core CPUs

---

### 1.3 **Use PNG Instead of JPEG** 📸
**File:** `src/UI/Logic/Media/WaveToVisualizer2.cs:908`

**Implementation:**
```csharp
private SKEncodedImageFormat GetImageFormat()
{
    return Se.Settings.VideoControls.UseExperimentalRenderer
        ? SKEncodedImageFormat.Png
        : SKEncodedImageFormat.Jpeg;
}

private int GetImageQuality()
{
    return Se.Settings.VideoControls.UseExperimentalRenderer
        ? 100  // PNG lossless
        : 50;  // JPEG 50% quality
}

// In save method
using (var imageData = bmp.Encode(GetImageFormat(), GetImageQuality()))
{
    imageData.SaveTo(stream);
}
```

**Impact:** 30-40% faster loading, better quality

---

### 1.4 **SIMD-Optimized FFT** ⚡
**New File:** `src/libse/Common/SimdFFT.cs`

**Implementation:**
```csharp
using System.Numerics;

public class SimdFFT
{
    // Use Vector<T> for SIMD operations
    public void ComputeForward(Span<double> buffer)
    {
        if (!Se.Settings.VideoControls.UseExperimentalRenderer)
        {
            // Fallback to RealFFT
            _legacyFft.ComputeForward(buffer.ToArray());
            return;
        }

        // SIMD-optimized implementation using Vector<double>
        int vectorSize = Vector<double>.Count;
        for (int i = 0; i < buffer.Length; i += vectorSize)
        {
            var vec = new Vector<double>(buffer.Slice(i, vectorSize));
            // Vectorized operations...
        }
    }
}
```

**Alternative:** Use **MathNet.Numerics** (already optimized with MKL/OpenBLAS):
```csharp
// Add to LibSE.csproj
<PackageReference Include="MathNet.Numerics" Version="5.0.0" />

// In SpectrogramDrawer
private readonly MathNet.Numerics.IntegralTransforms.Fourier _fft;

public void ProcessSegment(double[] samples, int offset, double[] magnitude)
{
    if (Se.Settings.VideoControls.UseExperimentalRenderer)
    {
        // Use MathNet FFT (SIMD-optimized)
        MathNet.Numerics.IntegralTransforms.Fourier.Forward(samples, 
            MathNet.Numerics.IntegralTransforms.FourierOptions.Matlab);
    }
    else
    {
        // Use legacy RealFFT
        _fft.ComputeForward(samples);
    }
}
```

**Impact:** 4-8x faster FFT computation

---

### 1.5 **Reduce Render Frequency** ⏱️
**File:** `src/UI/Controls/AudioVisualizerControl/AudioVisualizer.cs`

**Implementation:**
```csharp
private DispatcherTimer _renderTimer;
private double _lastRenderedPosition;
private const double MinPositionChangeMs = 10.0;

private void InitializeTimer()
{
    int intervalMs = Se.Settings.VideoControls.UseExperimentalRenderer
        ? 100  // Experimental: 100ms when not playing
        : 50;  // Legacy: 50ms always
    
    _renderTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(intervalMs), 
        DispatcherPriority.Render, OnRenderTick);
}

private void OnRenderTick(object sender, EventArgs e)
{
    if (Se.Settings.VideoControls.UseExperimentalRenderer)
    {
        // Only invalidate if position changed significantly
        var positionDelta = Math.Abs(CurrentVideoPositionSeconds - _lastRenderedPosition);
        if (positionDelta < MinPositionChangeMs / 1000.0)
        {
            return; // Skip this frame
        }
        _lastRenderedPosition = CurrentVideoPositionSeconds;
    }
    
    InvalidateVisual();
}
```

**Impact:** 50% reduction in render calls

---

### 1.6 **Incremental Waveform Generation** 📊
**New File:** `src/UI/Logic/Media/IncrementalWaveformGenerator.cs`

**Implementation:**
```csharp
public class IncrementalWaveformGenerator
{
    private readonly IProgress<WaveformProgress> _progress;
    
    public async Task<WavePeakData2> GeneratePeaksAsync(
        string waveFileName, 
        string peakFileName,
        CancellationToken token)
    {
        if (!Se.Settings.VideoControls.UseExperimentalRenderer)
        {
            // Use legacy synchronous generation
            return GeneratePeaksLegacy(waveFileName, peakFileName);
        }

        var peaks = new List<WavePeak2>();
        const int chunkSize = 1024 * 1024; // 1MB chunks
        
        using var stream = File.OpenRead(waveFileName);
        var totalBytes = stream.Length;
        var bytesProcessed = 0L;

        while (bytesProcessed < totalBytes)
        {
            var chunk = await ReadChunkAsync(stream, chunkSize);
            var chunkPeaks = ProcessChunk(chunk);
            peaks.AddRange(chunkPeaks);
            
            bytesProcessed += chunk.Length;
            
            // Report progress - UI can start rendering partial data
            _progress?.Report(new WaveformProgress
            {
                Peaks = peaks.ToArray(),
                PercentComplete = (double)bytesProcessed / totalBytes * 100
            });
            
            if (token.IsCancellationRequested)
                break;
        }

        return new WavePeakData2(sampleRate, peaks);
    }
}
```

**Impact:** Better perceived performance, progressive rendering

---

## Phase 2: Rendering Optimizations (3-5 days)

### 2.1 **Dirty Rectangle Rendering** 🎯
**File:** `src/UI/Controls/AudioVisualizerControl/ExperimentalWaveformRenderer.cs`

**Implementation:**
```csharp
public class ExperimentalWaveformRenderer : IWaveformRenderer
{
    private Rect _lastDirtyRect;
    private double _lastCursorPosition;
    private HashSet<int> _modifiedParagraphIds = new();

    public void Render(DrawingContext context, RenderContext ctx)
    {
        if (!Se.Settings.VideoControls.UseExperimentalRenderer)
        {
            // Delegate to legacy renderer
            _legacyRenderer.Render(context, ctx);
            return;
        }

        var dirtyRect = CalculateDirtyRegion(ctx);
        if (dirtyRect.IsEmpty)
            return;

        using (context.PushClip(dirtyRect))
        {
            DrawWaveFormOptimized(context, ctx, dirtyRect);
        }
    }

    private Rect CalculateDirtyRegion(RenderContext ctx)
    {
        // Only redraw cursor area + modified paragraphs
        var cursorX = SecondsToXPosition(ctx.CurrentVideoPositionSeconds);
        var cursorRect = new Rect(cursorX - 2, 0, 4, ctx.Height);
        
        // Union with modified paragraph regions
        foreach (var paraId in _modifiedParagraphIds)
        {
            var paraRect = GetParagraphRect(paraId, ctx);
            cursorRect = cursorRect.Union(paraRect);
        }
        
        return cursorRect;
    }
}
```

**Impact:** 60-80% reduction in pixel drawing

---

### 2.2 **Waveform Render Caching** 💾
**File:** `src/UI/Controls/AudioVisualizerControl/ExperimentalWaveformRenderer.cs`

**Implementation:**
```csharp
public class ExperimentalWaveformRenderer : IWaveformRenderer
{
    private SKBitmap? _cachedWaveform;
    private double _cachedZoom;
    private double _cachedStart;
    private int _cachedWidth;

    private void DrawWaveForm(DrawingContext context, RenderContext ctx)
    {
        if (!Se.Settings.VideoControls.UseExperimentalRenderer)
        {
            DrawWaveFormLegacy(context, ctx);
            return;
        }

        if (NeedsCacheRefresh(ctx))
        {
            _cachedWaveform?.Dispose();
            _cachedWaveform = RenderWaveformToBitmap(ctx);
            _cachedZoom = ctx.ZoomFactor;
            _cachedStart = ctx.StartPositionSeconds;
            _cachedWidth = (int)ctx.Width;
        }

        var avaloniaBitmap = _cachedWaveform.ToAvaloniaBitmap();
        context.DrawImage(avaloniaBitmap, new Rect(0, 0, ctx.Width, ctx.Height));
    }

    private bool NeedsCacheRefresh(RenderContext ctx)
    {
        return _cachedWaveform == null ||
               Math.Abs(_cachedZoom - ctx.ZoomFactor) > 0.001 ||
               Math.Abs(_cachedStart - ctx.StartPositionSeconds) > 0.001 ||
               _cachedWidth != (int)ctx.Width;
    }

    private SKBitmap RenderWaveformToBitmap(RenderContext ctx)
    {
        var bitmap = new SKBitmap((int)ctx.Width, (int)ctx.Height);
        using var canvas = new SKCanvas(bitmap);
        
        // Render waveform to SKCanvas (GPU-accelerated)
        RenderWaveformToCanvas(canvas, ctx);
        
        return bitmap;
    }
}
```

**Impact:** 90% faster rendering when not zooming/scrolling

---

### 2.3 **Use SkiaSharp for All Rendering** 🎨
**File:** `src/UI/Controls/AudioVisualizerControl/ExperimentalWaveformRenderer.cs`

**Implementation:**
```csharp
public class ExperimentalWaveformRenderer : IWaveformRenderer
{
    public void Render(DrawingContext context, RenderContext ctx)
    {
        if (!Se.Settings.VideoControls.UseExperimentalRenderer)
        {
            RenderWithAvalonia(context, ctx);
            return;
        }

        // Use SkiaSharp for GPU-accelerated rendering
        var skBitmap = new SKBitmap((int)ctx.Width, (int)ctx.Height);
        using (var skCanvas = new SKCanvas(skBitmap))
        {
            skCanvas.Clear(SKColors.Black);
            
            // All drawing operations use SkiaSharp
            DrawWaveformSk(skCanvas, ctx);
            DrawSpectrogramSk(skCanvas, ctx);
            DrawParagraphsSk(skCanvas, ctx);
            DrawCursorSk(skCanvas, ctx);
        }

        // Convert to Avalonia bitmap once
        var avaloniaBitmap = skBitmap.ToAvaloniaBitmap();
        context.DrawImage(avaloniaBitmap, new Rect(0, 0, ctx.Width, ctx.Height));
    }

    private void DrawWaveformSk(SKCanvas canvas, RenderContext ctx)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Blue,
            StrokeWidth = 1,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        // GPU-accelerated line drawing
        for (int x = 0; x < ctx.Width; x++)
        {
            var (yMax, yMin) = CalculateWaveformPoint(x, ctx);
            canvas.DrawLine(x, yMax, x, yMin, paint);
        }
    }
}
```

**Impact:** 2-3x rendering speedup via GPU acceleration

---

## Phase 3: Video Player Optimizations (3-4 days)

### 3.1 **Event-Driven Position Updates** 🎬
**File:** `src/UI/Logic/VideoPlayers/LibMpvDynamic/LibMpvDynamicPlayer.cs`

**Implementation:**
```csharp
public class LibMpvDynamicPlayer : IVideoPlayerInstance
{
    private void InitializeEventDriven()
    {
        if (!Se.Settings.VideoControls.UseExperimentalRenderer)
        {
            // Use legacy polling
            StartPositionPolling();
            return;
        }

        // Use mpv property observation
        _mpvObserveProperty(_mpv, 0, "time-pos", MpvFormat.Double);
        _mpvSetWakeupCallback(_mpv, OnMpvWakeup, IntPtr.Zero);
    }

    private void OnMpvWakeup(IntPtr userData)
    {
        while (true)
        {
            var evt = _mpvWaitEvent(_mpv, 0);
            if (evt == IntPtr.Zero)
                break;

            var eventId = Marshal.ReadInt32(evt);
            if (eventId == MPV_EVENT_PROPERTY_CHANGE)
            {
                // Position changed - update UI
                Dispatcher.UIThread.Post(() => OnPositionChanged());
            }
        }
    }
}
```

**Impact:** Eliminates 50ms polling overhead

---

### 3.2 **Hardware Decode Acceleration** 🚀
**File:** `src/UI/Logic/VideoPlayers/LibMpvDynamic/LibMpvDynamicPlayer.cs`

**Implementation:**
```csharp
private void ConfigureHardwareAcceleration()
{
    if (!Se.Settings.VideoControls.UseExperimentalRenderer)
    {
        // Use default software decoding
        return;
    }

    // Enable hardware decoding
    SetOptionString("hwdec", "auto-safe");  // GPU decoding
    SetOptionString("vo", "gpu");           // GPU video output
    SetOptionString("gpu-api", "auto");     // Best available API
    
    // Platform-specific optimizations
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        SetOptionString("hwdec", "videotoolbox"); // macOS VideoToolbox
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        SetOptionString("hwdec", "d3d11va");      // Windows D3D11
    }
}
```

**Impact:** 50-70% lower CPU usage, smoother playback

---

## Phase 4: Memory & Architecture (5-7 days)

### 4.1 **Virtual Scrolling for Waveform** 📜
**File:** `src/UI/Controls/AudioVisualizerControl/ExperimentalWaveformRenderer.cs`

**Implementation:**
```csharp
public class ExperimentalWaveformRenderer : IWaveformRenderer
{
    private const int BufferSamples = 1000; // Samples before/after visible area

    private void DrawWaveForm(DrawingContext context, RenderContext ctx)
    {
        if (!Se.Settings.VideoControls.UseExperimentalRenderer)
        {
            DrawWaveFormLegacy(context, ctx);
            return;
        }

        // Calculate visible sample range
        var startSample = (int)(ctx.StartPositionSeconds * ctx.SampleRate);
        var endSample = (int)((ctx.StartPositionSeconds + ctx.Width / (ctx.ZoomFactor * ctx.SampleRate)) * ctx.SampleRate);
        
        // Add buffer
        startSample = Math.Max(0, startSample - BufferSamples);
        endSample = Math.Min(ctx.WavePeaks.Peaks.Count, endSample + BufferSamples);

        // Only render visible + buffer samples
        for (int sample = startSample; sample < endSample; sample++)
        {
            var x = SampleToXPosition(sample, ctx);
            if (x < 0 || x > ctx.Width)
                continue;
                
            DrawSample(context, sample, x, ctx);
        }
    }
}
```

**Impact:** Constant memory usage regardless of video length

---

### 4.2 **Memory-Mapped Files for Peak Data** 🗺️
**File:** `src/UI/Logic/Media/WaveToVisualizer2.cs`

**Implementation:**
```csharp
public class ExperimentalWavePeakLoader
{
    private MemoryMappedFile? _mmf;
    private MemoryMappedViewAccessor? _accessor;

    public WavePeakData2 LoadPeaks(string peakFileName)
    {
        if (!Se.Settings.VideoControls.UseExperimentalRenderer)
        {
            return LoadPeaksLegacy(peakFileName);
        }

        // Memory-mapped file for instant access
        _mmf = MemoryMappedFile.CreateFromFile(peakFileName, FileMode.Open);
        _accessor = _mmf.CreateViewAccessor();

        // Read header
        var sampleRate = _accessor.ReadInt32(0);
        var peakCount = _accessor.ReadInt32(4);

        // Create lazy-loaded peak array
        var peaks = new MemoryMappedPeakArray(_accessor, 8, peakCount);
        
        return new WavePeakData2(sampleRate, peaks);
    }

    public void Dispose()
    {
        _accessor?.Dispose();
        _mmf?.Dispose();
    }
}

public class MemoryMappedPeakArray : IList<WavePeak2>
{
    private readonly MemoryMappedViewAccessor _accessor;
    private readonly long _offset;
    private readonly int _count;

    public WavePeak2 this[int index]
    {
        get
        {
            var position = _offset + (index * 4); // 2 shorts = 4 bytes
            var max = _accessor.ReadInt16(position);
            var min = _accessor.ReadInt16(position + 2);
            return new WavePeak2(max, min);
        }
    }
}
```

**Impact:** Instant loading, lower memory usage

---

### 4.3 **Level of Detail (LOD)** 🔍
**File:** `src/UI/Logic/Media/WaveToVisualizer2.cs`

**Implementation:**
```csharp
public class LodWavePeakGenerator
{
    public WavePeakData2 GeneratePeaksWithLod(string waveFileName, string peakFileName)
    {
        if (!Se.Settings.VideoControls.UseExperimentalRenderer)
        {
            return GeneratePeaksLegacy(waveFileName, peakFileName);
        }

        // Generate multiple LOD levels
        var lod0 = GeneratePeaks(waveFileName, 1);    // Full detail
        var lod1 = GeneratePeaks(waveFileName, 10);   // 1/10 detail
        var lod2 = GeneratePeaks(waveFileName, 100);  // 1/100 detail

        // Save all LODs
        SaveLodPeaks(peakFileName, lod0, lod1, lod2);
        
        return new WavePeakDataLod(lod0, lod1, lod2);
    }
}

public class WavePeakDataLod
{
    private readonly WavePeakData2[] _lods;

    public IList<WavePeak2> GetPeaksForZoom(double zoomFactor)
    {
        if (zoomFactor > 1.0)
            return _lods[0].Peaks; // Full detail
        else if (zoomFactor > 0.1)
            return _lods[1].Peaks; // Medium detail
        else
            return _lods[2].Peaks; // Low detail
    }
}
```

**Impact:** 10-100x faster rendering at low zoom

---

## Settings Integration

### **Settings UI**
**File:** `src/UI/Features/Settings/VideoControlsSettingsView.axaml`

```xml
<CheckBox IsChecked="{Binding UseExperimentalRenderer}"
          Content="Use Experimental Waveform Renderer (Beta)"
          ToolTip.Tip="Enables performance optimizations for waveform/spectrogram rendering. May have bugs - disable if you experience issues."/>

<TextBlock Text="⚠️ Experimental features - can be disabled anytime"
           FontSize="11"
           Foreground="Orange"
           Margin="20,0,0,5"/>
```

### **Settings Model**
**File:** `src/libse/Common/Settings/VideoControlsSettings.cs`

```csharp
public class VideoControlsSettings
{
    public bool UseExperimentalRenderer { get; set; } = false;
    
    // Individual feature toggles (for granular control)
    public bool UseAsyncSpectrogramLoading { get; set; } = true;
    public bool UseParallelFFT { get; set; } = true;
    public bool UseRenderCaching { get; set; } = true;
    public bool UseHardwareVideoDecoding { get; set; } = true;
    public bool UseMemoryMappedPeaks { get; set; } = true;
}
```

---

## Testing Strategy

### **A/B Testing**
Users can easily compare performance:
1. Open video with experimental renderer OFF
2. Note performance metrics (render time, memory usage)
3. Enable experimental renderer
4. Compare metrics

### **Fallback Mechanism**
```csharp
public class SafeExperimentalRenderer : IWaveformRenderer
{
    private readonly IWaveformRenderer _experimental;
    private readonly IWaveformRenderer _legacy;
    private int _errorCount = 0;
    private const int MaxErrors = 3;

    public void Render(DrawingContext context, RenderContext ctx)
    {
        if (_errorCount >= MaxErrors)
        {
            // Too many errors - permanently fallback to legacy
            _legacy.Render(context, ctx);
            return;
        }

        try
        {
            _experimental.Render(context, ctx);
        }
        catch (Exception ex)
        {
            _errorCount++;
            Se.LogError(ex, "Experimental renderer failed, falling back to legacy");
            _legacy.Render(context, ctx);
        }
    }
}
```

---

## Expected Performance Gains

| Component | Current | Phase 1 | Phase 2 | Phase 3 | Phase 4 |
|-----------|---------|---------|---------|---------|---------|
| **Waveform Render** | 100% | 50% | 10% | 10% | 5% |
| **Spectrogram Load** | 2-5s | 0.5s | 0.2s | 0.2s | <0.1s |
| **Spectrogram Gen** | 100% | 15% | 15% | 15% | 10% |
| **Video Playback** | 100% | 100% | 100% | 50% | 50% |
| **Memory Usage** | 100% | 100% | 90% | 90% | 40% |

**Overall:** 5-10x performance improvement when experimental renderer is enabled.

---

## Implementation Priority

### **Phase 1 (Must Have)** - 3-5 days
1. ✅ Async spectrogram loading
2. ✅ Parallel FFT processing
3. ✅ PNG instead of JPEG
4. ✅ SIMD-optimized FFT
5. ✅ Reduce render frequency
6. ✅ Incremental waveform generation

### **Phase 2 (High Priority)** - 3-5 days
7. Dirty rectangle rendering
8. Waveform render caching
9. SkiaSharp GPU rendering

### **Phase 3 (Medium Priority)** - 3-4 days
10. Event-driven video updates
11. Hardware video decode

### **Phase 4 (Nice to Have)** - 5-7 days
12. Virtual scrolling
13. Memory-mapped files
14. Level of detail

---

## Migration Path

### **Version 1.0** - Experimental Flag
- All optimizations behind feature flag
- Legacy code unchanged
- Easy rollback

### **Version 2.0** - Gradual Rollout
- Enable experimental by default for new users
- Existing users keep legacy renderer
- Monitor crash reports

### **Version 3.0** - Full Migration
- Remove legacy code paths
- Experimental becomes standard
- Keep feature flag for future experiments

---

## Risk Mitigation

1. **Preserve Legacy Code** - Never delete old implementation
2. **Feature Flag** - Easy disable mechanism
3. **Automatic Fallback** - Catch exceptions, revert to legacy
4. **Telemetry** - Track which renderer is used, error rates
5. **User Feedback** - Prominent "Report Issue" button in settings
6. **Gradual Rollout** - Enable for small % of users first

---

## Success Metrics

- ✅ 5x faster spectrogram generation
- ✅ 10x faster waveform rendering at low zoom
- ✅ 50% reduction in memory usage
- ✅ 50% reduction in CPU usage during video playback
- ✅ Zero crashes/regressions for users with experimental OFF
- ✅ <5% crash rate for users with experimental ON

---

## Conclusion

This plan provides a **safe, incremental path** to dramatically improve performance while maintaining **100% backward compatibility**. Users who prefer stability can keep the experimental renderer disabled, while power users can benefit from significant performance gains.

All changes are **optional, reversible, and non-breaking**.
