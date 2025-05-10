namespace HanumanInstitute.LibMpv;

/// <summary>
/// Specifies which libass font provider backend to use.
/// </summary>
public enum FontProvider
{
    /// <summary>
    /// Attempt to use the native font provider: fontconfig on Linux, CoreText on OSX, DirectWrite on Windows.
    /// </summary>
    Auto,
    /// <summary>
    /// Disables system fonts.
    /// </summary>
    None,
    /// <summary>
    /// Forces fontconfig, if libass was built with support (if not, it behaves like none).
    /// </summary>
    Fontconfig
}