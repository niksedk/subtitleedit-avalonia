namespace HanumanInstitute.LibMpv;

/// <summary>
/// Represents a decoder.
/// </summary>
public class DecoderInfo
{
    /// <summary>
    /// Canonical codec name, which identifies the format the decoder can handle.
    /// </summary>
    public string Codec { get; set; } = string.Empty;
    /// <summary>
    /// The name of the decoder itself. Often, this is the same as codec. Sometimes it can be different. It is used to distinguish multiple decoders for the same codec.
    /// </summary>
    public string Driver { get; set; } = string.Empty;
    /// <summary>
    /// Human readable description of the decoder and codec.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}