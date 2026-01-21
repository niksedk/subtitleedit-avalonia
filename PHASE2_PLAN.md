# Phase 2: Rendering Optimizations

## ✅ STATUS: COMPLETED

**Implementation Date:** Phase 2 implemented with bitmap caching for waveform and spectrogram rendering.

---

## Phase 1 Post-Mortem

### What Went Wrong

Phase 1 experimental optimizations produced **negative results**:

| Metric | Legacy | Experimental | Result |
|--------|--------|--------------|--------|
| Spectrogram Generation | 8,499ms | 23,006ms | **2.7x SLOWER** |
| FFT per chunk | 13.60ms | 36.81ms | **2.7x SLOWER** |

### Root Causes

1. **MathNet.Numerics FFT Overhead** - The SIMD-optimized library has significant initialization and data marshaling overhead that outweighs gains for small 256-point FFTs
2. **PNG vs JPEG** - PNG encoding is computationally expensive; JPEG 50% quality is faster despite decode overhead
3. **Wrong Bottleneck** - FFT computation was not the actual bottleneck; image encoding/disk I/O dominates

### Lessons Learned

- Profile before optimizing
- Small FFT sizes (256) don't benefit from SIMD libraries designed for large transforms
- The custom `RealFFT` is already well-optimized for this use case

---

## Phase 2 Strategy: Focus on Rendering

The **real bottleneck** is the per-frame rendering loop that runs every 50ms:

```csharp
// Current: O(width) per frame, ~800-1200 DrawLine calls
for (var x = 0; x < renderCtx.Width; x++)
{
    context.DrawLine(pen, new Point(x, yMax), new Point(x, yMin));
}
```

**Key Insight**: The waveform only changes when:
1. User scrolls (StartPositionSeconds changes)
2. User zooms (ZoomFactor changes)
3. Control resizes (Width/Height changes)
4. Video cursor moves (only 2-pixel wide line needs update)

**Solution**: Cache rendered waveform bitmap, only re-render on state change.

---

## Implementation Plan

### 2.1 Waveform Bitmap Caching ⭐ HIGH IMPACT

**Goal**: Render waveform to off-screen bitmap once, blit to screen each frame.

**Cache Invalidation Triggers**:
- `StartPositionSeconds` changed
- `ZoomFactor` changed  
- `VerticalZoomFactor` changed
- Control `Width` or `Height` changed
- `WavePeaks` data changed
- Selected paragraphs changed (affects coloring)

**Expected Impact**: 90%+ reduction in render time during playback

### 2.2 Spectrogram Bitmap Caching ⭐ HIGH IMPACT

**Current Problem** (line 1189-1208):
```csharp
// Creates new bitmap EVERY frame
using var skBitmapCombined = new SKBitmap(width, _spectrogram.FftSize / 2);
```

**Solution**: Cache combined spectrogram bitmap, invalidate only on scroll/zoom.

**Expected Impact**: Eliminates bitmap allocation per frame

### 2.3 Cursor-Only Rendering ⭐ MEDIUM IMPACT

**Current**: Full redraw every 50ms during playback
**Better**: Cache static content, only redraw cursor line (2px wide)

**Implementation**: 
- Render static content (waveform, spectrogram, paragraphs) to cached bitmap
- Composite cached bitmap + cursor in `Render()`

### 2.4 Smart Invalidation ⭐ LOW IMPACT

Track render state, skip `InvalidateVisual()` if nothing changed:
```csharp
if (_lastRenderState.Equals(currentState))
    return; // Skip unnecessary render
```

---

## Implementation Details

### Cached State Structure

```csharp
private record struct WaveformCacheKey(
    double StartPositionSeconds,
    double ZoomFactor,
    double VerticalZoomFactor,
    int Width,
    int Height,
    int PeaksHash,
    int SelectedParagraphsHash
);

private SKBitmap? _cachedWaveformBitmap;
private WaveformCacheKey _cachedWaveformKey;
```

### Rendering Flow (After Optimization)

```
OnRender():
├── Check if cache valid
│   ├── YES → Draw cached bitmap
│   └── NO  → Render to new bitmap, update cache
├── Draw cursor (always, 2px)
└── Draw paragraph markers (check if changed)
```

---

## Files to Modify

1. **`AudioVisualizer.cs`**
   - Add cache fields and key structure
   - Modify `DrawWaveForm()` to use caching
   - Modify `DrawSpectrogram()` to use caching
   - Add cache invalidation on property changes

2. **`WaveToVisualizer2.cs`**
   - Revert experimental FFT/PNG changes (use legacy JPEG/RealFFT)
   - Keep async loading (that part works)

---

## Success Metrics

| Metric | Current | Target | Method |
|--------|---------|--------|--------|
| Render time (playback) | ~5-10ms | <1ms | Bitmap blit only |
| Render time (scroll/zoom) | ~5-10ms | ~5-10ms | Full re-render |
| Memory overhead | ~0MB | ~2-5MB | Cached bitmaps |
| GC pressure | High | Low | Reuse bitmaps |

---

## Risk Mitigation

1. **Memory Usage** - Dispose cached bitmaps when control unloaded
2. **Visual Glitches** - Ensure cache invalidation catches all state changes
3. **Fallback** - Keep experimental flag; disable caching if issues occur

---

## Phase 2 Checklist

- [x] Revert Phase 1 FFT/PNG experimental changes (SIMD FFT removed - legacy RealFFT is faster)
- [x] Implement `WaveformCacheKey` structure
- [x] Add waveform bitmap caching in `DrawWaveForm()`
- [x] Add spectrogram bitmap caching in `DrawSpectrogram()`
- [x] Implement cache invalidation (`InvalidateRenderCaches()`)
- [x] Add performance metrics logging (`[PERF]` cache MISS logging)
- [ ] Test with various video lengths
- [ ] Document results in PERFORMANCE_METRICS.md

## Implementation Summary

### Files Modified

1. **`src/UI/Controls/AudioVisualizerControl/AudioVisualizer.cs`**
   - Added `WaveformCacheKey` and `SpectrogramCacheKey` record structs
   - Added cached bitmap fields for waveform and spectrogram
   - Added `DrawWaveFormCached()` - renders waveform to SKBitmap, caches for reuse
   - Added `RenderWaveformToSkCanvas()` - SkiaSharp-based waveform rendering
   - Added `DrawSpectrogramCached()` - caches combined spectrogram bitmap
   - Added `RenderSpectrogramToCanvas()` - extracted spectrogram stitching logic
   - Added `InvalidateRenderCaches()` - clears caches when data changes

2. **`src/UI/Logic/Media/WaveToVisualizer2.cs`**
   - Removed SIMD FFT usage (was 2.7x slower than legacy RealFFT)
   - Removed `_simdFft` field from `SpectrogramDrawer`
   - Updated performance logging labels

### Expected Performance Improvement

| Scenario | Before (Legacy) | After (Experimental) |
|----------|-----------------|----------------------|
| Playback render | ~5-10ms/frame | <1ms (bitmap blit) |
| Scroll/zoom | ~5-10ms | ~5-10ms (cache miss) |
| Memory overhead | 0 | ~2-5MB cached bitmaps |

**Key Benefit**: During video playback, the waveform/spectrogram are cached and only a simple bitmap blit is performed each frame, instead of recalculating and redrawing thousands of lines.
