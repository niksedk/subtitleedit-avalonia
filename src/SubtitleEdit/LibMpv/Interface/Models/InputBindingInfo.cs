using System.Text.Json.Serialization;

namespace HanumanInstitute.LibMpv;

/// <summary>
/// Represents a binding for a single key/command
/// </summary>
public class InputBindingInfo
{
    /// <summary>
    /// The key name. This is normalized and may look slightly different from how it was specified in the source (e.g. in input.conf).
    /// </summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>
    /// The command mapped to the key. (Currently, this is exactly the same string as specified in the source, other than stripping whitespace and comments. It's possible that it will be normalized in the future.)
    /// </summary>
    public string Cmd { get; set; } = string.Empty;
    /// <summary>
    /// If set to true, any existing and active user bindings will take priority.
    /// </summary>
    [JsonPropertyName("is_weak")]
    public bool? IsWeak { get; set; }
    /// <summary>
    /// If this entry exists, the name of the script (or similar) which added this binding.
    /// </summary>
    public string Owner { get; set; } = string.Empty;
    /// <summary>
    /// Name of the section this binding is part of. This is a rarely used mechanism. This entry may be removed or change meaning in the future.
    /// </summary>
    public string Section { get; set; } = string.Empty;
    /// <summary>
    /// A number. Bindings with a higher value are preferred over bindings with a lower value. If the value is negative, this binding is inactive and will not be triggered by input. Note that mpv does not use this value internally, and matching of bindings may work slightly differently in some cases. In addition, this value is dynamic and can change around at runtime.
    /// </summary>
    public int? Priority { get; set; }
    /// <summary>
    /// If available, the comment following the command on the same line. (For example, the input.conf entry f cycle bla # toggle bla would result in an entry with comment = "toggle bla", cmd = "cycle bla".)
    /// </summary>
    public string Comment { get; set; } = string.Empty;
}