using System.Text.Json.Serialization;

namespace HanumanInstitute.LibMpv;

/// <summary>
/// Whether to probe stream information.
/// </summary>
public enum ProbeOption
{
    /// <summary>
    /// Probe stream information.
    /// </summary>
    Yes,
    /// <summary>
    /// Do not probe stream information.
    /// </summary>
    No,
    /// <summary>
    /// Tries to skip this for a few know-safe whitelisted formats, while calling it for everything else.
    /// </summary>
    Auto,
    /// <summary>
    /// Probes if and only if the file seems to contain no streams after opening (helpful in cases when calling the function is needed to detect streams at all, such as with FLV files).
    /// </summary>
    [JsonPropertyName("nostreams")]
    NoStreams
}