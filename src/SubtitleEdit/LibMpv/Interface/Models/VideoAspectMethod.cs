namespace HanumanInstitute.LibMpv;

/// <summary>
/// Sets the default video aspect determination method (if the aspect is _not_ overridden by the user with --video-aspect-override or others).
/// </summary>
public enum VideoAspectMethod
{
    /// <summary>
    /// Strictly prefer the container aspect ratio. This is apparently the default behavior with VLC, at least with Matroska. Note that if the container has no aspect ratio set, the behavior is the same as with bitstream.
    /// </summary>
    Container,
    /// <summary>
    /// Strictly prefer the bitstream aspect ratio, unless the bitstream aspect ratio is not set. This is apparently the default behavior with XBMC/kodi, at least with Matroska.
    /// </summary>
    Bitstream,
}