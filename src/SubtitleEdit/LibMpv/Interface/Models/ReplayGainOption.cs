namespace HanumanInstitute.LibMpv;

/// <summary>
/// Adjust volume gain according to replaygain values stored in the file metadata.
/// </summary>
public enum ReplayGainOption
{
    /// <summary>
    /// Perform no adjustment (default).
    /// </summary>
    No,
    /// <summary>
    /// Apply track gain.
    /// </summary>
    Track,
    /// <summary>
    /// Apply album gain if present and fall back to track gain otherwise.
    /// </summary>
    Album
}