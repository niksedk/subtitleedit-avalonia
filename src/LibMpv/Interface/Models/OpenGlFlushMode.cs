namespace HanumanInstitute.LibMpv;

/// <summary>
/// Specifies when to calls DwmFlush after swapping buffers on Windows.
/// </summary>
public enum OpenGlFlushMode
{
    /// <summary>
    /// Disabled.
    /// </summary>
    No,
    /// <summary>
    /// Enabled, also in fullscreen.
    /// </summary>
    Yes,
    /// <summary>
    /// Try to determine whether the compositor is active, and calls DwmFlush only if it seems to be.
    /// </summary>
    Auto,
    /// <summary>
    /// Only in window mode.
    /// </summary>
    Windowed
}