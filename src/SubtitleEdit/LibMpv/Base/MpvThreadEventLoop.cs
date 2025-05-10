using HanumanInstitute.LibMpv.Core;

namespace HanumanInstitute.LibMpv;

public unsafe class MpvThreadEventLoop : IEventLoop, IDisposable
{
    protected volatile bool IsEventLoopRunning;
    protected Thread? EvenLoopThread;
    private readonly MpvHandle* _context;
    private readonly Action<MpvEvent> _handleEvent;
    private bool _disposed;
    
    public MpvThreadEventLoop(MpvHandle* context, Action<MpvEvent> eventHandler)
    {
        this._context = context;
        _handleEvent = eventHandler;
    }


    public void Stop()
    {
        if (IsEventLoopRunning)
        {
            IsEventLoopRunning = false;
            MpvApi.Wakeup(_context);
            EvenLoopThread!.Join();
        }
    }

    public void Start()
    {
        EvenLoopThread = new Thread(ProcessEvents);
        IsEventLoopRunning = true;
        EvenLoopThread.Start();
    }

    private void ProcessEvents()
    {
        while (IsEventLoopRunning)
        {
            var eventPtr = MpvApi.WaitEvent(_context, -1);
            if (eventPtr != null)
            {
                var @event = MarshalHelper.PtrToStructure<MpvEvent>((nint)eventPtr);
                if (@event.EventId != MpvEventId.None)
                {
                    _handleEvent(@event);
                }
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _disposed = true;
        }
    }
}
