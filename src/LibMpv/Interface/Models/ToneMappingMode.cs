using System.Text.Json.Serialization;

namespace HanumanInstitute.LibMpv;

/// <summary>
/// Specifies the algorithm used for tone-mapping images onto the target display.
/// </summary>
public enum ToneMappingMode
{
    /// <summary>
    /// Hard-clip any out-of-range values. Use this when you care about perfect color accuracy for in-range values at the cost of completely distorting out-of-range values. Not generally recommended.
    /// </summary>
    Clip,
    /// <summary>
    /// Generalization of Reinhard to a Möbius transform with linear section. Smoothly maps out-of-range values while retaining contrast and colors for in-range material as much as possible. Use this when you care about color accuracy more than detail preservation. This is somewhere in between clip and reinhard, depending on the value of --tone-mapping-param.
    /// </summary>
    Mobius,
    /// <summary>
    /// Reinhard tone mapping algorithm. Very simple continuous curve. Preserves overall image brightness but uses nonlinear contrast, which results in flattening of details and degradation in color accuracy.
    /// </summary>
    Reinhard,
    /// <summary>
    /// Similar to reinhard but preserves both dark and bright details better (slightly sigmoidal), at the cost of slightly darkening / desaturating everything. Developed by John Hable for use in video games. Use this when you care about detail preservation more than color/brightness accuracy. This is roughly equivalent to --tone-mapping=reinhard --tone-mapping-param=0.24. If possible, you should also enable --hdr-compute-peak for the best results.
    /// </summary>
    Hable,
    /// <summary>
    /// Perceptual tone mapping curve (EETF) specified in ITU-R Report BT.2390. This is the recommended curve to use for typical HDR-mastered content. (Default)
    /// </summary>
    [JsonPropertyName("bt.2390")]
    Bt2390,
    /// <summary>
    /// Fits a logarithmic transfer between the tone curves.
    /// </summary>
    Gamma,
    /// <summary>
    /// Linearly stretches the entire reference gamut to (a linear multiple of) the display.
    /// </summary>
    Linear
}