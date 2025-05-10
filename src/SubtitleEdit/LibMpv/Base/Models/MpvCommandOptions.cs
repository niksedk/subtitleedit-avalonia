namespace HanumanInstitute.LibMpv;

/// <summary>
/// Contains additional options for sending API commands.
/// </summary>
public class MpvCommandOptions : MpvAsyncOptions
{
    public MpvCommandOptions()
    {}
    
    /// <summary>
    /// When set, use the default behavior for this command. This is the default for input.conf commands. Some libmpv/scripting/IPC APIs do not use this as default, but use no-osd instead.
    /// </summary>
    public bool OsdAuto { get; set; }

    /// <summary>
    /// When set, do not use any OSD for this command.
    /// </summary>
    public bool NoOsd { get; set; }

    /// <summary>
    /// When set, if possible, show a bar with this command. Seek commands will show the progress bar, property changing commands may show the newly set value.
    /// </summary>
    public bool OsdBar { get; set; }

    /// <summary>
    /// When set, if possible, show an OSD message with this command. Seek command show the current playback time, property changing commands show the newly set value as text.
    /// </summary>
    public bool OsdMsg { get; set; }

    /// <summary>
    /// When set, combine OsdBar and OsdMsg.
    /// </summary>
    public bool OsdMsgBar { get; set; }

    /// <summary>
    /// When set, do not expand properties in string arguments. (Like "${property-name}".) This is the default for some libmpv/scripting/IPC APIs.
    /// </summary>
    public bool Raw { get; set; }

    /// <summary>
    /// When set, all string arguments are expanded as described in Property Expansion. This is the default for input.conf commands.
    /// </summary>
    public bool ExpandProperties { get; set; }

    /// <summary>
    /// When set, for some commands, keeping a key pressed doesn't run the command repeatedly. This prefix forces enabling key repeat in any case.
    /// </summary>
    public bool Repeatable { get; set; }

    /// <summary>
    /// When set, allow asynchronous execution (if possible). Note that only a few commands will support this (usually this is explicitly documented). Some commands are asynchronous by default (or rather, their effects might manifest after completion of the command). The semantics of this flag might change in the future. Set it only if you don't rely on the effects of this command being fully realized when it returns. See Synchronous vs. Asynchronous.
    /// </summary>
    public bool Async { get; set; }

    /// <summary>
    /// When set, allow synchronous execution (if possible). Normally, all commands are synchronous by default, but some are asynchronous by default for compatibility with older behavior.
    /// </summary>
    public bool Sync { get; set; }

    /// <summary>
    /// Returns the list of prefixes that are set.
    /// </summary>
    public IList<string> GetPrefixes()
    {
        var result = new List<string>();
        AddPrefix(OsdAuto, "osd-auto");
        AddPrefix(NoOsd, "no-osd");
        AddPrefix(OsdBar, "osd-bar");
        AddPrefix(OsdMsg, "osd-msg");
        AddPrefix(OsdMsgBar, "osd-msg-bar");
        AddPrefix(Raw, "raw");
        AddPrefix(ExpandProperties, "expand-properties");
        AddPrefix(Repeatable, "repeatable");
        AddPrefix(Async, "async");
        AddPrefix(Sync, "sync");
        return result;

        void AddPrefix(bool cond, string name)
        {
            if (cond)
            {
                result.Add(name);
            }
        }
    }
}
