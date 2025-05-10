namespace HanumanInstitute.LibMpv;

/// <summary>
/// How the player synchronizes audio and video.
/// </summary>
public enum VideoSyncMode
{
    /// <summary>
    /// Time video frames to audio. This is the most robust mode, because the player doesn't have to assume anything about how the display behaves. The disadvantage is that it can lead to occasional frame drops or repeats. If audio is disabled, this uses the system clock. This is the default mode.
    /// </summary>
    Audio,
    /// <summary>
    /// Resample audio to match the video. This mode will also try to adjust audio speed to compensate for other drift. (This means it will play the audio at a different speed every once in a while to reduce the A/V difference.)
    /// </summary>
    DisplayResample,
    /// <summary>
    /// Resample audio to match the video. Drop video frames to compensate for drift.
    /// </summary>
    DisplayResampleVdrop,
    /// <summary>
    /// Like the previous mode, but no A/V compensation.
    /// </summary>
    DisplayResampleDesync,
    /// <summary>
    /// Drop or repeat video frames to compensate desyncing video. (Although it should have the same effects as audio, the implementation is very different.)
    /// </summary>
    DisplayVdrop,
    /// <summary>
    /// Drop or repeat audio data to compensate desyncing video. See --video-sync-adrop-size. This mode will cause severe audio artifacts if the real monitor refresh rate is too different from the reported or forced rate. Since mpv 0.33.0, this acts on entire audio frames, instead of single samples.
    /// </summary>
    DisplayAdrop,
    /// <summary>
    /// Sync video to display, and let audio play on its own.
    /// </summary>
    DisplayDesync,
    /// <summary>
    /// Sync video according to system clock, and let audio play on its own.
    /// </summary>
    Desync
}