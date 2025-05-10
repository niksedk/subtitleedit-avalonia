using System.Text.Json.Serialization;

namespace HanumanInstitute.LibMpv;

/// <summary>
/// Skip displaying some frames to maintain A/V sync on slow systems, or playing high framerate video on video outputs that have an upper framerate limit.
/// </summary>
public enum FrameDropOption
{
    /// <summary>
    /// Disable any frame dropping. Not recommended, for testing only.
    /// </summary>
    No,
    /// <summary>
    /// Drop late frames on video output (default). This still decodes and filters all frames, but doesn't render them on the VO. Drops are indicated in the terminal status line as Dropped: field.
    /// In audio sync. mode, this drops frames that are outdated at the time of display. If the decoder is too slow, in theory all frames would have to be dropped (because all frames are too late) - to avoid this, frame dropping stops if the effective framerate is below 10 FPS.
    /// In display-sync. modes (see --video-sync), this affects only how A/V drops or repeats frames. If this mode is disabled, A/V desync will in theory not affect video scheduling anymore (much like the display-resample-desync mode). However, even if disabled, frames will still be skipped (i.e. dropped) according to the ratio between video and display frequencies.
    /// This is the recommended mode, and the default.
    /// </summary>
    Vo,
    /// <summary>
    /// Old, decoder-based framedrop mode. (This is the same as --framedrop=yes in mpv 0.5.x and before.) This tells the decoder to skip frames (unless they are needed to decode future frames). May help with slow systems, but can produce unwatchable choppy output, or even freeze the display completely.
    /// This uses a heuristic which may not make sense, and in general cannot achieve good results, because the decoder's frame dropping cannot be controlled in a predictable manner. Not recommended.
    /// Even if you want to use this, prefer decoder+vo for better results.
    /// The --vd-lavc-framedrop option controls what frames to drop.
    /// </summary>
    Decoder,
    /// <summary>
    /// Enable both modes. Not recommended. Better than just decoder mode.
    /// </summary>
    [JsonPropertyName("decoder+vo")]
    DecoderVo
}