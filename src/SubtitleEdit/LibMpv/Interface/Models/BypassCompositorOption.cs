namespace HanumanInstitute.LibMpv;

/// <summary>
/// (X11 only) Specifies when to bypass the compositor.
/// </summary>
public enum BypassCompositorOption
{
    /// <summary>
    /// Ask the compositor to unredirect the mpv window.
    /// </summary>
    Yes,
    /// <summary>
    /// Sets _NET_WM_BYPASS_COMPOSITOR to 0, which is the default value as declared by the EWMH specification, i.e. no change is done.
    /// </summary>
    No,
    /// <summary>
    /// Asks the window manager to disable the compositor only in fullscreen mode. (default)
    /// </summary>
    FsOnly,
    /// <summary>
    /// Asks the window manager to never disable the compositor.
    /// </summary>
    Never
}