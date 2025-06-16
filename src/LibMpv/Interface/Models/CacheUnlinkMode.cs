namespace HanumanInstitute.LibMpv;

/// <summary>
/// Whether or when to unlink cache files.
/// </summary>
public enum CacheUnlinkMode
{
    /// <summary>
    /// Don't delete cache files. They will consume disk space without having a use.
    /// </summary>
    No,
    /// <summary>
    /// Unlink cache file after they were created. The cache files won't be visible anymore, even though they're in use. This ensures they are guaranteed to be removed from disk when the player terminates, even if it crashes.
    /// </summary>
    Immediate,
    /// <summary>
    /// Delete cache files after they are closed.
    /// </summary>
    Whendone
}