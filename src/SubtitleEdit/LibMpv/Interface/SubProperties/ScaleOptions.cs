namespace HanumanInstitute.LibMpv;

/// <summary>
/// Exposes the scale sub-properties.
/// </summary>
public class ScaleOptions
{
    private readonly MpvContext _mpv;
    private readonly string _prefix;

    public ScaleOptions(MpvContext mpv, string propertyName)
    {
        _mpv = mpv;
        _prefix = propertyName;
    }

    /// <summary>
    /// The filter function to use when upscaling video.
    /// </summary>
    public MpvOptionString Filter => new(_mpv, _prefix);

    /// <summary>
    /// Set filter parameters. By default, these are set to the special string default, which maps to a scaler-specific default value. Ignored if the filter is not tunable.
    /// </summary>
    public MpvOption<double> Param1 => new(_mpv, _prefix + "-param1");

    /// <summary>
    /// Set filter parameters. By default, these are set to the special string default, which maps to a scaler-specific default value. Ignored if the filter is not tunable.
    /// </summary>
    public MpvOption<double> Param2 => new(_mpv, _prefix + "-param2");

    /// <summary>
    /// Kernel scaling factor (also known as a blur factor). Decreasing this makes the result sharper, increasing it makes it blurrier (default 0). If set to 0, the kernel's preferred blur factor is used. Note that setting this too low (eg. 0.5) leads to bad results. It's generally recommended to stick to values between 0.8 and 1.2.
    /// </summary>
    public MpvOption<double> Blur => new(_mpv, _prefix + "-blur");

    /// <summary>
    /// Window scaling factor (also known as a blur factor). Decreasing this makes the result sharper, increasing it makes it blurrier (default 0). If set to 0, the kernel's preferred blur factor is used. Note that setting this too low (eg. 0.5) leads to bad results. It's generally recommended to stick to values between 0.8 and 1.2.
    /// </summary>
    public MpvOption<double> BlurW => new(_mpv, _prefix + "-wblur");

    /// <summary>
    /// Specifies a weight bias to multiply into negative coefficients. Specifying --scale-clamp=1 has the effect of removing negative weights completely, thus effectively clamping the value range to [0-1]. Values between 0.0 and 1.0 can be specified to apply only a moderate diminishment of negative weights. This is especially useful for --tscale, where it reduces excessive ringing artifacts in the temporal domain (which typically manifest themselves as short flashes or fringes of black, mostly around moving edges) in exchange for potentially adding more blur. The default for --tscale-clamp is 1.0, the others default to 0.0.
    /// </summary>
    public MpvOption<double> Clamp => new(_mpv, _prefix + "-clamp");

    /// <summary>
    /// Cut off the filter kernel prematurely once the value range drops below this threshold. Doing so allows more aggressive pruning of skippable coefficients by disregarding parts of the LUT which are effectively zeroed out by the window function. Only affects polar (EWA) filters. The default is 0.001 for each, which is perceptually transparent but provides a 10%-20% speedup, depending on the exact radius and filter kernel chosen.
    /// </summary>
    public MpvOption<double> Cutoff => new(_mpv, _prefix + "-cutoff");

    /// <summary>
    /// Kernel taper factor. Increasing this flattens the filter function. Value range is 0 to 1. A value of 0 (the default) means no flattening, a value of 1 makes the filter completely flat (equivalent to a box function). Values in between mean that some portion will be flat and the actual filter function will be squeezed into the space in between.
    /// </summary>
    public MpvOption<double> Taper => new(_mpv, _prefix + "-taper");

    /// <summary>
    /// Window taper factor. Increasing this flattens the filter function. Value range is 0 to 1. A value of 0 (the default) means no flattening, a value of 1 makes the filter completely flat (equivalent to a box function). Values in between mean that some portion will be flat and the actual filter function will be squeezed into the space in between.
    /// </summary>
    public MpvOption<double> TaperW => new(_mpv, _prefix + "-wtaper");

    /// <summary>
    /// Set radius for tunable filters, must be a float number between 0.5 and 16.0. Defaults to the filter's preferred radius if not specified. Doesn't work for every scaler and VO combination.
    /// Note that depending on filter implementation details and video scaling ratio, the radius that actually being used might be different(most likely being increased a bit).
    /// </summary>
    public MpvOption<double> Radius => new(_mpv, _prefix + "-radius");

    /// <summary>
    /// Set the antiringing strength. This tries to eliminate ringing, but can introduce other artifacts in the process. Must be a float number between 0.0 and 1.0. The default value of 0.0 disables antiringing entirely.
    /// Note that this doesn't affect the special filters bilinear and bicubic_fast, nor does it affect any polar (EWA) scalers.
    /// </summary>
    public MpvOption<double> Antiring => new(_mpv, _prefix + "-antiring");

    /// <summary>
    /// (Advanced users only) Choose a custom windowing function for the kernel. Defaults to the filter's preferred window if unset. Use --scale-window=help to get a list of supported windowing functions.
    /// </summary>
    public MpvOptionString WindowingFunction => new(_mpv, _prefix + "-window");

    /// <summary>
    /// (Advanced users only) Configure the parameter for the window function given by --scale-window etc. By default, these are set to the special string default, which maps to a window-specific default value. Ignored if the window is not tunable. Currently, this affects the following window parameters:
    /// </summary>
    public MpvOption<double> WindowFunctionParam => new(_mpv, _prefix + "-wparam");




}
