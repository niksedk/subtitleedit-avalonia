namespace HanumanInstitute.LibMpv.Core;

public partial class MpvApi
{
    /// <summary>
    ///     Gets or sets the root path for loading libraries.
    /// </summary>
    /// <value>The root path.</value>
    public static string RootPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
    
    /// <summary>
    /// Gets or sets the name of the DLL to load.
    /// </summary>
    public static string DllName { get; set; } = "libmpv";

    // Libraries
    public static Dictionary<string, int> LibraryVersionMap = new()
    {
        {"libmpv", 2}
    };
    
    // Macros
    internal const int MpvEnableDeprecated = 0x1;
    /// <summary>MPV_RENDER_API_TYPE_OPENGL = "opengl"</summary>
    internal const string MpvRenderApiTypeOpenGl = "opengl";
    /// <summary>MPV_RENDER_API_TYPE_SW = "sw"</summary>
    internal const string MpvRenderApiTypeSw = "sw";
    /// <summary>MPV_RENDER_PARAM_DRM_OSD_SIZE = 15</summary>
    internal const int MpvRenderParamDrmOsdSize = 0xf;
}
