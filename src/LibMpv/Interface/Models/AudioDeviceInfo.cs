namespace HanumanInstitute.LibMpv;

/// <summary>
/// Represents an audio device.
/// </summary>
public class AudioDeviceInfo
{
    /// <summary>
    /// The name is what is to be passed to the --audio-device option (and often a rather cryptic audio API-specific ID)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// human readable free form text. The description is set to the device name (minus mpv-specific <driver>/ prefix) if no description is available or the description would have been an empty string.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}