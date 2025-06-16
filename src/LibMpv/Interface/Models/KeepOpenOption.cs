namespace HanumanInstitute.LibMpv;

/// <summary>
/// Specifies what to do when file ends.
/// </summary>
[Flags]
public enum KeepOpenOption
{
    /// <summary>
    /// If the current file ends, go to the next file or terminate. (Default.)
    /// </summary>
    No,
    /// <summary>
    /// Don't terminate if the current file is the last playlist entry. Equivalent to --keep-open without arguments.
    /// </summary>
    Yes,
    /// <summary>
    /// Like yes, but also applies to files before the last playlist entry. This means playback will never automatically advance to the next file.
    /// </summary>
    Always
}