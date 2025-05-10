namespace HanumanInstitute.LibMpv;

/// <summary>
/// Arguments for EndFile event.
/// </summary>
public class EndFileEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the reason why playback ended.
    /// </summary>
    public EndReason Reason { get; set; }
}