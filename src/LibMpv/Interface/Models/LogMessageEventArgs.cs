namespace HanumanInstitute.LibMpv;

/// <summary>
/// Arguments for LogMessage event.
/// </summary>
public class LogMessageEventArgs
{
    /// <summary>
    /// The module prefix, identifies the sender of the message. This is what the terminal player puts in front of the message text when using the --v option, and is also what is used for --msg-level.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;
    /// <summary>
    /// The log level as string. See msg.log for possible log level names. Note that later versions of mpv might add new levels or remove (undocumented) existing ones.
    /// </summary>
    public LogLevel Level { get; set; }
    /// <summary>
    /// The log message. The text will end with a newline character. Sometimes it can contain multiple lines.
    /// </summary>
    public string Text { get; set; } = string.Empty;
}