using System.Text.Json.Serialization;

namespace HanumanInstitute.LibMpv;

/// <summary>
/// Select a specific D3D11 output color space to utilize for D3D11 rendering.
/// </summary>
public enum D3DOutputColorSpace
{
    Auto,
    Srgb,
    Linear,
    Pq,
    [JsonPropertyName("bt.2020")]
    Bt2020
}