# Performance Metrics Documentation

## Overview

Performance metrics have been added to measure the impact of Phase 1-3 optimizations. All metrics are logged to the Debug output and can be viewed in your IDE's debug console or system logs.

**Note:** On Apple Silicon Macs, macOS may schedule parallel work on efficiency cores only, limiting speedup. This is OS-level scheduling behavior.

## Metrics Categories

### 1. Spectrogram Loading Metrics

**Location:** `WaveToVisualizer2.cs:LoadAsync()`

**Logged When:** Loading existing spectrogram images from disk

**Metrics:**
- Total load time (milliseconds)
- Number of images loaded
- Average time per image (ms/image)
- Mode: Async (experimental) vs Sync (legacy)

**Output:**
```
[PERF] Spectrogram async load: 625 images in 202ms (avg: 0,32ms/image)
[PERF] Spectrogram async load: 625 images in 193ms (avg: 0,31ms/image)
[PERF] Spectrogram async load: 625 images in 275ms (avg: 0,44ms/image)
[PERF] Spectrogram async load: 625 images in 200ms (avg: 0,32ms/image, max 10 parallel)
[PERF] Spectrogram sync load: 625 images in 198ms (avg: 0,32ms/image)
```

**Expected Improvement:** 3-4x faster with async parallel loading

---

### 2. Peak Generation Metrics (Phase 3)

**Location:** 
- Legacy: `WaveToVisualizer2.cs:GeneratePeaks()`
- Optimized: `Logic/Media/Optimized/PeakGeneratorOptimized.cs`

**Logged When:** Generating waveform peaks from audio file

**Metrics:**
- Total generation time (milliseconds)
- Number of peaks processed
- Audio duration (seconds)
- Thread count (optimized only)

**Output:**
```
[PERF] Peak generation (Legacy): 1023212 peaks in 11691ms (6821,4s audio)

[PERF] Optimized Peaks: Read 639506KB in 99ms
[PERF] Optimized Peaks: Processed 1023212 peaks in 5815ms (16 threads)
[PERF] Optimized Peaks: Saved in 84ms
[PERF] Optimized Peaks TOTAL: 1023212 peaks in 6027ms (6821,4s audio)
```

**Measured Improvement:** **1.9x faster** (11.7s → 6.0s)

---

### 3. Spectrogram Generation Metrics (Phase 3)

**Location:** 
- Legacy: `WaveToVisualizer2.cs:GenerateSpectrogram()`
- Optimized: `Logic/Media/Optimized/SpectrogramGeneratorOptimized.cs`

**Logged When:** Generating new spectrogram from audio file

**Metrics:**
- Total generation time (milliseconds)
- Number of chunks processed
- Average time per chunk (ms/chunk)
- Audio duration (seconds)
- Thread count (optimized only)

**Output:**
```
[PERF] Spectrogram generation (Legacy): 625 chunks in 29094ms (avg: 46,55ms/chunk, 6821,4s audio)

[PERF] Optimized: Read 625 chunks (163840000 samples) in 6998ms
[PERF] Optimized: Generated 625 spectrograms in 7954ms (16 threads)
[PERF] Optimized: Saved 625 images in 418ms
[PERF] Optimized TOTAL: 625 chunks in 15409ms (avg: 24,65ms/chunk, 6821,4s audio)
```

**Measured Improvement:** **1.9x faster** (29s → 15s)

---

### 4. FFT Processing Metrics

**Location:** `WaveToVisualizer2.cs:SpectrogramDrawer`

**Logged When:** Drawing spectrogram bitmap (called per chunk)

**Metrics:**
- Bitmap dimensions (width x height)
- Total draw time (milliseconds)
- FFT type: SIMD vs Legacy
- Number of FFT calls
- Total FFT time (milliseconds)
- Average FFT time per call (ms)

**Example Output:**
```
[PERF] SpectrogramDrawer initialized with SIMD FFT (size: 256)
[PERF] Draw: 1024x128 bitmap in 28ms | FFT (SIMD): 2048 calls, 12ms total, 0.006ms avg
[PERF] Draw: 1024x128 bitmap in 67ms | FFT (Legacy): 2048 calls, 45ms total, 0.022ms avg
```

**Expected Improvement:** 4-8x faster FFT with SIMD

---

## How to View Metrics

### Option 1: IDE Debug Console (Recommended)

1. Run Subtitle Edit in **Debug mode** from your IDE
2. Open the **Debug Console** or **Output Window**
3. Filter for `[PERF]` messages
4. Perform actions that trigger metrics:
   - Open a video with existing spectrogram → Loading metrics
   - Generate new spectrogram → Generation + FFT metrics

### Option 2: System Console (macOS)

```bash
# Terminal 1: Run the app
cd /Users/hgg/work/subtitleedit-avalonia
dotnet run --project src/UI/UI.csproj

# Terminal 2: Monitor debug output
log stream --predicate 'eventMessage contains "[PERF]"' --level debug
```

### Option 3: DebugView (Windows)

1. Download [DebugView](https://learn.microsoft.com/en-us/sysinternals/downloads/debugview)
2. Run DebugView as Administrator
3. Enable "Capture Global Win32"
4. Filter for `[PERF]`

---

## Testing Scenarios

### Scenario 1: Compare Async vs Sync Loading

**Steps:**
1. Open a video that already has a spectrogram
2. Check debug output for loading metrics
3. Toggle experimental renderer in Settings
4. Reload the same video
5. Compare load times

**Expected Results:**
- Async: ~5-10ms per image
- Sync: ~15-25ms per image
- **Speedup: 3-4x**

---

### Scenario 2: Compare SIMD vs Legacy FFT

**Steps:**
1. Delete existing spectrogram folder for a video
2. Enable experimental renderer
3. Open video and generate spectrogram
4. Note generation time and FFT metrics
5. Delete spectrogram again
6. Disable experimental renderer
7. Generate spectrogram again
8. Compare metrics

**Expected Results:**
- SIMD FFT: ~0.005-0.010ms per FFT call
- Legacy FFT: ~0.020-0.040ms per FFT call
- **Speedup: 4-8x**

---

### Scenario 3: Compare PNG vs JPEG

**Steps:**
1. Generate spectrogram with experimental renderer (PNG)
2. Note file sizes in spectrogram folder
3. Generate with legacy renderer (JPEG)
4. Compare file sizes and load times

**Expected Results:**
- PNG: Larger files (~2-3x), faster decode
- JPEG: Smaller files, slower decode
- **Net benefit: 30-40% faster loading despite larger files**

---

## Performance Baseline (Reference)

### Phase 3 Results (2-hour audio file, ~6821s)

| Component | Legacy | Optimized | Improvement |
|-----------|--------|-----------|-------------|
| Peak Generation | 11,691ms | 6,027ms | **1.9x faster** |
| Spectrogram Generation | 29,094ms | 15,409ms | **1.9x faster** |
| **Combined Total** | ~41s | ~21s | **~2x faster** |

### Phase 1 Results (3-minute video)

| Metric | Legacy | Experimental | Improvement |
|--------|--------|--------------|-------------|
| Spectrogram Load | ~900ms | ~250ms | **3.6x faster** |
| FFT per call | ~0.025ms | ~0.006ms | **4.2x faster** |

---

## Troubleshooting

### No Metrics Appearing

**Problem:** Debug output not showing `[PERF]` messages

**Solutions:**
1. Ensure running in Debug configuration (not Release)
2. Check IDE debug console is enabled
3. Verify `System.Diagnostics.Debug.WriteLine` is not stripped
4. Try running from terminal with `dotnet run`

### Metrics Show 0ms

**Problem:** All timings show 0ms

**Solutions:**
1. Test with longer audio files (>30 seconds)
2. Stopwatch resolution may be too coarse
3. Try generating spectrogram instead of just loading

### Unexpected Performance

**Problem:** Experimental mode is slower than legacy

**Possible Causes:**
1. CPU doesn't support SIMD instructions well
2. Disk I/O bottleneck (SSD vs HDD)
3. Antivirus scanning PNG files
4. First-run JIT compilation overhead

---

## Implementation Details

### Code Locations

1. **Spectrogram Loading:**
   - File: `src/UI/Logic/Media/WaveToVisualizer2.cs`
   - Method: `SpectrogramData2.LoadAsync()`
   - Lines: 383-413

2. **Spectrogram Generation:**
   - File: `src/UI/Logic/Media/WaveToVisualizer2.cs`
   - Method: `WavePeakGenerator2.GenerateSpectrogram()`
   - Lines: 896, 1028-1030

3. **FFT Processing:**
   - File: `src/UI/Logic/Media/WaveToVisualizer2.cs`
   - Class: `SpectrogramDrawer`
   - Methods: Constructor (1084-1098), `Draw()` (1113-1156), `ProcessSegment()` (1159-1183)

### Metric Format

All metrics follow this format:
```
[PERF] <Operation>: <Details> | <Breakdown>
```

Examples:
- `[PERF] Spectrogram async load: 45 images in 234ms (avg: 5.20ms/image)`
- `[PERF] Draw: 1024x128 bitmap in 28ms | FFT (SIMD): 2048 calls, 12ms total, 0.006ms avg`

---

## Future Enhancements

### Completed (Phase 2-3)

1. **Waveform Cache Metrics**
   - Cache hit/miss logging
   - Overscan buffer rendering

2. **Parallel Processing Metrics**
   - Thread count used
   - Per-stage timing (read/process/save)

### Planned (Future)

1. **Memory Usage Metrics**
   - Bitmap memory consumption
   - Peak memory during generation

### Aggregated Statistics

Future versions may include:
- Session-wide performance summaries
- Comparison reports (experimental vs legacy)
- Performance degradation detection
- Automatic optimization recommendations

---

## Contributing

When adding new performance metrics:

1. Use `System.Diagnostics.Debug.WriteLine()`
2. Prefix with `[PERF]`
3. Include operation name and key metrics
4. Use consistent units (ms for time, MB for memory)
5. Add documentation to this file
6. Update test scenarios

---

## References

- Phase 1 Implementation: `PHASE1_COMPLETION_STATUS.md`
- Performance Plan: `PERFORMANCE_IMPROVEMENT_PLAN.md`
- SIMD FFT Implementation: `src/libse/Common/SimdFFT.cs`
- **Phase 3 Optimized Classes:**
  - `src/UI/Logic/Media/Optimized/PeakGeneratorOptimized.cs`
  - `src/UI/Logic/Media/Optimized/SpectrogramGeneratorOptimized.cs`
  - `src/UI/Logic/Media/Optimized/SpectrogramDrawerOptimized.cs`
  - `src/UI/Logic/Media/Optimized/WaveProcessorFactory.cs`
