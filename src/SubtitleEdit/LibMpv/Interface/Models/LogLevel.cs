namespace HanumanInstitute.LibMpv;

/// <summary>
/// Represents the MPV log level.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Disable absolutely all messages.
    /// </summary>
    No,
    /// <summary>
    /// Show critical and aborting errors.
    /// </summary>
    Fatal,
    /// <summary>
    /// Show simple errors.
    /// </summary>
    Error,
    /// <summary>
    /// Show possible problems.
    /// </summary>
    Warn,
    /// <summary>
    /// Show informational messages.
    /// </summary>
    Info,
    /// <summary>
    /// Show noisy informational messages.
    /// </summary>
    V,
    /// <summary>
    /// Show very noisy technical information.
    /// </summary>
    Debug,
    /// <summary>
    /// Show extremely noisy messages.
    /// </summary>
    Trace
}