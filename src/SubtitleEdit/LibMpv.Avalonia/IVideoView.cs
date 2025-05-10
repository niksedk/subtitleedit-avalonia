namespace HanumanInstitute.LibMpv.Avalonia;

public interface IVideoView : IDisposable
{
    MpvContext? MpvContext { get; }
}
