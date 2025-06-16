namespace HanumanInstitute.LibMpv;

/// <summary>
/// Specifies whether to show album art during audio-only playback.
/// </summary>
public enum AudioDisplayOption
{
    /// <summary>
    /// Disables display of video entirely when playing audio files.
    /// </summary>
    No,
    /// <summary>
    /// Displays image attachments (e.g. album cover art) when playing audio files. It will display the first image found, and additional images are available as video tracks.
    /// </summary>
    Attachment
}