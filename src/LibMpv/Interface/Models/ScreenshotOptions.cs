namespace HanumanInstitute.LibMpv;

/// <summary>
/// Represents screenshot flags. Multiple flags can be combined.
/// </summary>
[Flags]
public enum ScreenshotOptions
{
    None = 0,
    /// <summary>
    /// Save the video image, in its original resolution, and with subtitles. Some video outputs may still include the OSD in the output under certain circumstances.
    /// </summary>
    Subtitles = 1,
    /// <summary>
    /// Like subtitles, but typically without OSD or subtitles. The exact behavior depends on the selected video output.
    /// </summary>
    Video = 2,
    /// <summary>
    /// Save the contents of the mpv window. Typically scaled, with OSD and subtitles. The exact behavior depends on the selected video output, and if no support is available, this will act like video.
    /// </summary>
    Window = 4,
    /// <summary>
    /// Take a screenshot each frame. Issue this command again to stop taking screenshots. Note that you should disable frame-dropping when using this mode - or you might receive duplicate images in cases when a frame was dropped. This flag can be combined with the other flags, e.g. video+each-frame.
    /// </summary>
    EachFrame = 8
}