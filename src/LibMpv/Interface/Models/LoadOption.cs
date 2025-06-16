namespace HanumanInstitute.LibMpv;

/// <summary>
/// Flags to control how subtitles or audio resources are loaded
/// </summary>
public enum LoadOption
{
    /// <summary>
    /// Select the resource immediately.
    /// </summary>
    Select,
    /// <summary>
    /// Don't select the loaded resource. (Or in some special situations, let the default stream selection mechanism decide.)
    /// </summary>
    Auto,
    /// <summary>
    /// Select the resource. If a resource with the same filename was already added, that one is selected, instead of loading a duplicate entry. (In this case, title/language are ignored, and if the was changed since it was loaded, these changes won't be reflected.)
    /// </summary>
    Cached
}