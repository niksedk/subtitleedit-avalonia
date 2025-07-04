using HanumanInstitute.LibMpv.Core;

namespace HanumanInstitute.LibMpv;

public unsafe partial class MpvContextBase : IDisposable
{
    private bool _disposed;
    private readonly IEventLoop _eventLoop;
    protected MpvHandle* Ctx => _ctx == null ? InitCtx() :
        !_disposed ? _ctx : throw new ObjectDisposedException(nameof(MpvContextBase));
    private MpvHandle* _ctx;
    private MpvHandle* InitCtx()
    {
        _ctx = MpvApi.Create();
        if (_ctx == null)
        {
            throw new MpvException("Unable to create mpv_context. Currently, this can happen in the following situations - out of memory or LC_NUMERIC is not set to \"C\"");
        }
        return _ctx;
    }
    
    public MpvContextBase() : this(MpvEventLoop.Default)
    {
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public MpvContextBase(MpvEventLoop mpvEventLoop)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        _eventHandlers = new Dictionary<MpvEventId, MpvEventHandler>();

#if ANDROID
        InitAndroid.InitJvm();
#endif

        Initialize();

        //       SetOptionString("vo", "libmpv"); // Prevent mpv from creating its own window
        SetOptionString("keep-open", "always");
        SetOptionString("sid", "no");
        SetOptionString("force-window", "no");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            SetOptionString("vo", "gpu");
            SetOptionString("gpu-context", "wayland");
            SetOptionString("gpu-api", "opengl");
        }

        InitEventHandlers();

        _eventLoop = mpvEventLoop switch
        {
            MpvEventLoop.Default => new MpvSimpleEventLoop(Ctx, HandleEvent),
            MpvEventLoop.Thread => new MpvThreadEventLoop(Ctx, HandleEvent),
            _ => new MpvWeakEventLoop(Ctx, HandleEvent)
        };
        _eventLoop.Start();
    }

    ~MpvContextBase()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            StopRendering();
            _eventLoop.Stop();
            if (_eventLoop is IDisposable disposable)
            {
                disposable.Dispose();
            }
            MpvApi.TerminateDestroy(Ctx);
            _disposed = true;
        }
    }
}
