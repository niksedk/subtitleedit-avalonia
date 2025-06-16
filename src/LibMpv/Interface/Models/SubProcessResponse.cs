using System.Text.Json.Serialization;

namespace HanumanInstitute.LibMpv;

/// <summary>
/// Contains named arguments for Subprocess response.
/// </summary>
public class SubProcessResponse
{
    /// <summary>
    /// The raw exit status of the process. It will be negative on error. The meaning of negative values is undefined, other than meaning error (and does not necessarily correspond to OS low level exit status values).
    /// </summary>
    public long? Status { get; set; }
    /// <summary>
    /// Captured stdout stream, limited to capture_size.
    /// </summary>
    [JsonPropertyName("stdout")]
    public IEnumerable<byte>? StdOut { get; set; }
    /// <summary>
    /// Captured stderr stream, limited to capture_size.
    /// </summary>
    [JsonPropertyName("stderr")]
    public IEnumerable<byte>? StdErr { get; set; }
    /// <summary>
    /// Empty string if the process exited gracefully. The string killed if the process was terminated in an unusual way. The string init if the process could not be started.
    /// On Windows, killed is only returned when the process has been killed by mpv as a result of playback_only being set to yes.
    /// </summary>
    [JsonPropertyName("error_string")]
    public string? ErrorString { get; set; }
    /// <summary>
    /// Set to True if the process has been killed by mpv, for example as a result of playback_only being set to yes, aborting the command (e.g. by mp.abort_async_command()), or if the player is about to exit.
    /// </summary>
    [JsonPropertyName("killed_by_us")]
    public bool? KilledByUs { get; set; }
}