using System.Text.Json.Serialization;

namespace HanumanInstitute.LibMpv;

/// <summary>
/// Mangle colors like (xy-)vsfilter do (default: basic).
/// </summary>
public enum SubAssVsFilterColor
{
    /// <summary>
    /// Handle only BT.601->BT.709 mangling, if the subtitles seem to indicate that this is required (default).
    /// </summary>
    Basic,
    /// <summary>
    /// Handle the full YCbCr Matrix header with all video color spaces supported by libass and mpv. This might lead to bad breakages in corner cases and is not strictly needed for compatibility (hopefully), which is why this is not default.
    /// </summary>
    Full,
    /// <summary>
    /// Force BT.601->BT.709 mangling, regardless of subtitle headers or video color space.
    /// </summary>
    [JsonPropertyName("force-601")]
    Force601,
    /// <summary>
    /// Disable color mangling completely. All colors are RGB.
    /// </summary>
    No
}