namespace HanumanInstitute.LibMpv;

/// <summary>
/// Blend subtitles directly onto upscaled video frames.
/// </summary>
public enum BlendSubtitlesMode
{
    /// <summary>
    /// Enable.
    /// </summary>
    Yes,
    /// <summary>
    /// Disable.
    /// </summary>
    No,
    /// <summary>
    /// Similar to yes, but subs are drawn at the video's native resolution, and scaled along with the video.
    /// </summary>
    Video
}