namespace HanumanInstitute.LibMpv;

/// <summary>
/// Decides what to do if the input has an alpha component.
/// </summary>
public enum AlphaMode
{
    /// <summary>
    /// Try to create a framebuffer with alpha component. This only makes sense if the video contains alpha information (which is extremely rare). May not be supported on all platforms. If alpha framebuffers are unavailable, it silently falls back on a normal framebuffer. Note that if you set the --fbo-format option to a non-default value, a format with alpha must be specified, or this won't work. Whether this really works depends on the windowing system and desktop environment.
    /// </summary>
    Yes,
    /// <summary>
    /// Ignore alpha component.
    /// </summary>
    No,
    /// <summary>
    /// Blend the frame against the background color (--background, normally black).
    /// </summary>
    Blend,
    /// <summary>
    /// Blend the frame against a 16x16 gray/white tiles background (default).
    /// </summary>
    BlendTiles
}