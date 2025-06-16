namespace HanumanInstitute.LibMpv;

/// <summary>
/// Specifies how to seek in files.
/// </summary>
public enum IndexMode
{
    /// <summary>
    /// Use an index if the file has one, or build it if missing.
    /// </summary>
    Default,
    /// <summary>
    /// Don't read or use the file's index.
    /// </summary>
    Recreate
}