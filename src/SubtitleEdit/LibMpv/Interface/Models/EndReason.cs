namespace HanumanInstitute.LibMpv;

/// <summary>
/// Specifies the reason why playback ended.
/// </summary>
public enum EndReason
{
    /// <summary>
    /// Unknown. Normally doesn't happen.
    /// </summary>
    Unknown,
    /// <summary>
    /// The file has ended. This can (but doesn't have to) include incomplete files or broken network connections under circumstances.
    /// </summary>
    Eof,
    /// <summary>
    /// Playback was ended by a command.
    /// </summary>
    Stop,
    /// <summary>
    /// Playback was ended by sending the quit command.
    /// </summary>
    Quit,
    /// <summary>
    /// An error happened. In this case, an error field is present with the error string.
    /// </summary>
    Error,
    /// <summary>
    /// Happens with playlists and similar.
    /// </summary>
    Redirect
}