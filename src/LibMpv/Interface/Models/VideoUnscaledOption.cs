namespace HanumanInstitute.LibMpv;

/// <summary>
/// Disable scaling of the video. If the window is larger than the video, black bars are added. Otherwise, the video is cropped, unless the option is set to downscale-big, in which case the video is fit to window. The video still can be influenced by the other --video-... options. This option disables the effect of --panscan.
/// </summary>
public enum VideoUnscaledOption
{
    Yes,
    No,
    DownscaleBig
}