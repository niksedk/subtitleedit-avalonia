using System.Text.Json.Serialization;

namespace HanumanInstitute.LibMpv;

/// <summary>
/// Contains named arguments for Subprocess command.
/// </summary>
public class SubProcessRequest
{
    public string? Name { get; set; }
    /// <summary>
    /// Array of strings with the command as first argument, and subsequent command line arguments following. This is just like the run command argument list.
    /// The first array entry is either an absolute path to the executable, or a filename with no path components, in which case the PATH environment variable.On Unix, this is equivalent to posix_spawnp and execvp behavior.
    /// </summary>
    public IList<string> Args { get; } = new List<string>();
    /// <summary>
    /// Whether the process should be killed when playback terminates (optional, default: True). If enabled, stopping playback will automatically kill the process, and you can't start it outside of playback.
    /// </summary>
    [JsonPropertyName("playback_only")]
    public bool? PlaybackOnly { get; set; }
    /// <summary>
    /// Maximum number of stdout plus stderr bytes that can be captured (optional, default: 64MB). If the number of bytes exceeds this, capturing is stopped. The limit is per captured stream.
    /// </summary>
    [JsonPropertyName("capture_size")]
    public long? CaptureSize { get; set; }
    /// <summary>
    /// Capture all data the process outputs to stdout and return it once the process ends (optional, default: false).
    /// </summary>
    [JsonPropertyName("capture_stdout")]
    public bool? CaptureStdOut { get; set; }
    /// <summary>
    /// Capture all data the process outputs to stderr and return it once the process ends (optional, default: false).
    /// </summary>
    [JsonPropertyName("capture_stderr")]
    public bool? CaptureStdErr { get; set; }

}