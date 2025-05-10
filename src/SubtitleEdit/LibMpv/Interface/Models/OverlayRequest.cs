using System.Text.Json.Serialization;

namespace HanumanInstitute.LibMpv;

/// <summary>
/// Represents an overlay for OsdOverlayAdd (osd-overlay) command.
/// </summary>
public class OverlayRequest
{
    public string? Name { get; set; }
    public int? Id { get; set; }
    public string? Format { get; set; }
    public string? Data { get; set; }
    [JsonPropertyName("res_x")]
    public int? ResX { get; set; }
    [JsonPropertyName("res_y")]
    public int? ResY { get; set; }
    public int? Z { get; set; }
}