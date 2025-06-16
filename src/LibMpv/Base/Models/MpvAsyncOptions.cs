namespace HanumanInstitute.LibMpv;

/// <summary>
/// Contains additional options for sending API commands.
/// </summary>
public class MpvAsyncOptions
{
    /// <summary>
    /// Gets or sets whether to wait for server response when sending the command. If null, takes the default value configured on the controller.
    /// </summary>
    public bool? WaitForResponse { get; set; }

    /// <summary>
    /// Gets or sets the response timeout. If null, takes the default value configured on the controller.
    /// </summary>
    public int? ResponseTimeout { get; set; }

    /// <summary>
    /// Gets or sets whether to throw an exception when the response contains an error. If null, takes the default value configured on the controller.
    /// </summary>
    public bool? ThrowOnError { get; set; }
}
