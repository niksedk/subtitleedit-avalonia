namespace HanumanInstitute.LibMpv;

/// <summary>
/// Set font hinting type.
/// </summary>
public enum SubAssHinting
{
    /// <summary>
    /// No hinting (default).
    /// </summary>
    None,
    /// <summary>
    /// FreeType autohinter, light mode.
    /// </summary>
    Light,
    /// <summary>
    /// FreeType autohinter, normal mode.
    /// </summary>
    Normal,
    /// <summary>
    /// Font native hinter.
    /// </summary>
    Native
}