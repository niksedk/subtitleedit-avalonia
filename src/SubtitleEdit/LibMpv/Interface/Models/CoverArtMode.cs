namespace HanumanInstitute.LibMpv;

/// <summary>
/// Whether to load _external_ cover art automatically.
/// </summary>
public enum CoverArtMode
{
    /// <summary>
    /// Disabled.
    /// </summary>
    No,
    /// <summary>
    /// Picks up a whitelist of "album art" filenames (such as cover.jpg)
    /// </summary>
    Fuzzy
}