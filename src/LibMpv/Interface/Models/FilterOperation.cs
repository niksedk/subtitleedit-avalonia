namespace HanumanInstitute.LibMpv;

/// <summary>
/// Chooses what happens to the audio or video filter.
/// </summary>
public enum FilterOperation
{
    /// <summary>
    /// Overwrite the previous filter chain with the new one.
    /// </summary>
    Set,
    /// <summary>
    /// Append the new filter chain to the previous one.
    /// </summary>
    Add,
    /// <summary>
    /// Check if the given filter (with the exact parameters) is already in the video chain. If yes, remove the filter. If no, add the filter. (If several filters are passed to the command, this is done for each filter.)
    /// A special variant is combining this with labels, and using @name without filter name and parameters as filter entry.This toggles the enable/disable flag.
    /// </summary>
    Toggle,
    /// <summary>
    /// Remove the given filters from the video chain. Unlike in the other cases, the second parameter is a comma separated list of filter names or integer indexes. 0 would denote the first filter. Negative indexes start from the last filter, and -1 denotes the last filter. Deprecated.
    /// </summary>
    Del,
    /// <summary>
    /// Remove all filters. Note that like the other sub-commands, this does not control automatically inserted filters.
    /// </summary>
    Clr
}