namespace HanumanInstitute.LibMpv;

/// <summary>
/// Load additional audio files matching the video filename. The parameter specifies how external audio files are matched.
/// </summary>
public enum FileAutoLoadOption
{
    /// <summary>
    /// Don't automatically load external audio files (default).
    /// </summary>
    No,
    /// <summary>
    /// Load the media filename with audio file extension.
    /// </summary>
    Exact,
    /// <summary>
    /// Load all audio files containing media filename.
    /// </summary>
    Fuzzy,
    /// <summary>
    /// Load all audio files in the current and --audio-file-paths directories.
    /// </summary>
    All
}