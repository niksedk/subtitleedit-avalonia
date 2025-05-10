namespace HanumanInstitute.LibMpv;

/// <summary>
/// Control whether user style overrides should be applied.
/// </summary>
public enum SubAssOverride
{
    /// <summary>
    /// Render subtitles as specified by the subtitle scripts, without overrides.
    /// </summary>
    No,
    /// <summary>
    /// Apply all the --sub-ass-* style override options. Changing the default for any of these options can lead to incorrect subtitle rendering (default).
    /// </summary>
    Yes,
    /// <summary>
    /// Like yes, but also force all --sub-* options. Can break rendering easily.
    /// </summary>
    Force,
    /// <summary>
    /// Like yes, but also apply --sub-scale.
    /// </summary>
    Scale,
    /// <summary>
    /// Radically strip all ASS tags and styles from the subtitle.
    /// </summary>
    Strip
}