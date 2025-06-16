namespace HanumanInstitute.LibMpv;

/// <summary>
/// Control whether OSD messages are shown on the console when no video output is available.
/// </summary>
public enum TermOsdMode
{
    /// <summary>
    /// Disable terminal OSD.
    /// </summary>
    No,
    /// <summary>
    /// Use terminal OSD if no video output active.
    /// </summary>
    Auto,
    /// <summary>
    /// Use terminal OSD even if video output active
    /// </summary>
    Force
}