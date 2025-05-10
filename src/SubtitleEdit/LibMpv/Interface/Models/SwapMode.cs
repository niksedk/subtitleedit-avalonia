namespace HanumanInstitute.LibMpv;

/// <summary>
/// Controls the presentation mode of the vulkan swapchain.
/// </summary>
public enum SwapMode
{
    /// <summary>
    /// Use the preferred swapchain mode for the vulkan context. (Default)
    /// </summary>
    Auto,
    /// <summary>
    /// Non-tearing, vsync blocked. Similar to "VSync on".
    /// </summary>
    Fifo,
    /// <summary>
    /// Tearing, vsync blocked. Late frames will tear instead of stuttering.
    /// </summary>
    FifoRelaxed,
    /// <summary>
    /// Non-tearing, not vsync blocked. Similar to "triple buffering".
    /// </summary>
    Mailbox,
    /// <summary>
    /// Tearing, not vsync blocked. Similar to "VSync off".
    /// </summary>
    Immediate
}