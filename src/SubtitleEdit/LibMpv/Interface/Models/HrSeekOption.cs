namespace HanumanInstitute.LibMpv;

/// <summary>
/// Specifies when to use precise seeking.
/// </summary>
public enum HrSeekOption
{
    Default,
    /// <summary>
    /// Never use precise seeks.
    /// </summary>
    No,
    /// <summary>
    /// Use precise seeks if the seek is to an absolute position in the file, such as a chapter seek, but not for relative seeks like the default behavior of arrow keys (default).
    /// </summary>
    Absolute,
    /// <summary>
    /// Use precise seeks whenever possible.
    /// </summary>
    Yes
}