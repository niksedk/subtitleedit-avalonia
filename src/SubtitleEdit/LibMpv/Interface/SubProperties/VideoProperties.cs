namespace HanumanInstitute.LibMpv;

/// <summary>
/// Exposes VideoParams sub-properties.
/// </summary>
public class VideoProperties
{
    private readonly MpvContext _mpv;
    private readonly string _prefix;

    public VideoProperties(MpvContext mpv, string propertyName)
    {
        _mpv = mpv;
        _prefix = propertyName;
    }

    /// <summary>
    /// The pixel format as string. This uses the same names as used in other places of mpv.
    /// </summary>
    public MpvPropertyReadString PixelFormat => new(_mpv, _prefix + "/pixelformat");

    /// <summary>
    /// The underlying pixel format as string. This is relevant for some cases of hardware decoding and unavailable otherwise.
    /// </summary>
    public MpvPropertyReadString PixelFormatHardware => new(_mpv, _prefix + "/hw-pixelformat");

    /// <summary>
    /// Average bits-per-pixel as integer. Subsampled planar formats use a different resolution, which is the reason this value can sometimes be odd or confusing. Can be unavailable with some formats.
    /// </summary>
    public MpvPropertyRead<int> AverageBitPerPixel => new(_mpv, _prefix + "/average-bpp");

    /// <summary>
    /// Bit depth for each color component as integer. This is only exposed for planar or single-component formats, and is unavailable for other formats.
    /// </summary>
    public MpvPropertyRead<int> PlaneDepth => new(_mpv, _prefix + "/plane-depth");

    /// <summary>
    /// Video width, with no aspect correction applied.
    /// </summary>
    public MpvPropertyRead<int> Width => new(_mpv, _prefix + "/w");

    /// <summary>
    /// Video height, with no aspect correction applied.
    /// </summary>
    public MpvPropertyRead<int> Height => new(_mpv, _prefix + "/h");

    /// <summary>
    /// Video width, scaled for correct aspect ratio.
    /// </summary>
    public MpvPropertyRead<int> DisplayWidth => new(_mpv, _prefix + "/dw");

    /// <summary>
    /// Video height, scaled for correct aspect ratio.
    /// </summary>
    public MpvPropertyRead<int> DisplayHeight => new(_mpv, _prefix + "/dh");

    /// <summary>
    /// Display aspect ratio.
    /// </summary>
    public MpvPropertyRead<float> AspectRatio => new(_mpv, _prefix + "/aspect");

    /// <summary>
    /// Pixel aspect ratio.
    /// </summary>
    public MpvPropertyReadString PixelAspectRatio => new(_mpv, _prefix + "/par");

    /// <summary>
    /// The colormatrix in use as string. (Exact values subject to change.)
    /// </summary>
    public MpvPropertyReadString ColorMatrix => new(_mpv, _prefix + "/colormatrix");

    /// <summary>
    /// The colorlevels as string. (Exact values subject to change.)
    /// </summary>
    public MpvPropertyReadString ColorLevels => new(_mpv, _prefix + "/colorlevels");

    /// <summary>
    /// The primaries in use as string. (Exact values subject to change.)
    /// </summary>
    public MpvPropertyReadString Primaries => new(_mpv, _prefix + "/primaries");

    /// <summary>
    /// The gamma function in use as string. (Exact values subject to change.)
    /// </summary>
    public MpvPropertyReadString Gamma => new(_mpv, _prefix + "/gamma");

    /// <summary>
    /// The video file's tagged signal peak as float.
    /// </summary>
    public MpvPropertyRead<float> SignalPeak => new(_mpv, _prefix + "/sig-peak");

    /// <summary>
    /// The light type in use as a string. (Exact values subject to change.)
    /// </summary>
    public MpvPropertyReadString Light => new(_mpv, _prefix + "/light");

    /// <summary>
    /// Chroma location as string. (Exact values subject to change.)
    /// </summary>
    public MpvPropertyReadString ChromaLocation => new(_mpv, _prefix + "/chroma-location");

    /// <summary>
    /// Intended display rotation in degrees (clockwise).
    /// </summary>
    public MpvPropertyRead<int> Rotate => new(_mpv, _prefix + "/rotate");

    /// <summary>
    /// Source file stereo 3D mode. (See the format video filter's stereo-in option.)
    /// </summary>
    public MpvPropertyReadString StereoIn => new(_mpv, _prefix + "/stereo-in");

    /// <summary>
    /// Number of audio channels.
    /// </summary>
    public MpvPropertyRead<int> ChannelCount => new(_mpv, _prefix + "/channel-count");
}
