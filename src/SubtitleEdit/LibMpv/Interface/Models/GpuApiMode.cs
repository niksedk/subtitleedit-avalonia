namespace HanumanInstitute.LibMpv;

/// <summary>
/// Controls which type of graphics APIs will be accepted:
/// </summary>
public enum GpuApiMode
{
    /// <summary>
    /// Use any available API (default).
    /// </summary>
    Auto,
    /// <summary>
    /// Allow only OpenGL (requires OpenGL 2.1+ or GLES 2.0+)
    /// </summary>
    Opengl,
    /// <summary>
    /// Allow only Vulkan (requires a valid/working --spirv-compiler)
    /// </summary>
    Vulkan,
    /// <summary>
    /// Allow only --gpu-context=d3d11
    /// </summary>
    D3d11
}