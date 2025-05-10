namespace HanumanInstitute.LibMpv;

/// <summary>
/// Flags to control the seek mode.
/// Multiple flags can be combined, e.g.: absolute+keyframes.
/// By default, keyframes is used for relative, relative-percent, and absolute-percent seeks, while exact is used for absolute seeks.
/// </summary>
[Flags]
public enum SeekOption
{
    None = 0,
    /// <summary>
    /// Seek relative to current position (a negative value seeks backwards).
    /// </summary>
    Relative = 1,
    /// <summary>
    /// Seek to a given time (a negative value starts from the end of the file).
    /// </summary>
    Absolute = 2,
    /// <summary>
    /// Seek to a given percent position.
    /// </summary>
    AbsolutePercent = 4,
    /// <summary>
    /// Seek relative to current position in percent.
    /// </summary>
    RelativePercent = 8,
    /// <summary>
    /// Always restart playback at keyframe boundaries (fast).
    /// </summary>
    Keyframes = 16,
    /// <summary>
    /// Always do exact/hr/precise seeks (slow).
    /// </summary>
    Exact = 32
}