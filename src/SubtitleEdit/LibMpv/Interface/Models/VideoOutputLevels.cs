namespace HanumanInstitute.LibMpv;

/// <summary>
/// RGB color levels used with YUV to RGB conversion.
/// </summary>
public enum VideoOutputLevels
{
    /// <summary>
    /// Automatic selection (equals to full range) (default).
    /// </summary>
    Auto,
    /// <summary>
    /// Limited range (16-235 per component), studio levels.
    /// </summary>
    Limited,
    /// <summary>
    /// Full range (0-255 per component), PC levels.
    /// </summary>
    Full
}