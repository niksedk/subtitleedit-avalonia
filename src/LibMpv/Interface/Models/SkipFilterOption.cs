namespace HanumanInstitute.LibMpv;

/// <summary>
/// Skips the loop filter (AKA deblocking) during H.264 decoding.
/// </summary>
public enum SkipFilterOption
{
    /// <summary>
    /// Never skip.
    /// </summary>
    None,
    /// <summary>
    /// Skip useless processing steps (e.g. 0 size packets in AVI).
    /// </summary>
    Default,
    /// <summary>
    /// Skip frames that are not referenced (i.e. not used for decoding other frames, the error cannot "build up").
    /// </summary>
    Nonref,
    /// <summary>
    /// Skip B-Frames.
    /// </summary>
    Bidir,
    /// <summary>
    /// Skip all frames except keyframes.
    /// </summary>
    Nonkey,
    /// <summary>
    /// Skip all frames.
    /// </summary>
    All
}