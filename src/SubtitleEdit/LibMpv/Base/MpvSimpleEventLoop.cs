using HanumanInstitute.LibMpv.Core;

namespace HanumanInstitute.LibMpv;

public unsafe class MpvSimpleEventLoop : IEventLoop, IDisposable
{
    protected volatile bool IsEventLoopRunning;
    protected Task? EventLoopTask;
    private readonly MpvHandle* _context;
    private readonly Action<MpvEvent> _handleEvent;
    private bool _disposed;

    public MpvSimpleEventLoop(MpvHandle* context, Action<MpvEvent> eventHandler)
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

            if (Task.CurrentId == EventLoopTask!.Id)
            {
                return;
            }
            EventLoopTask.Wait();
        }
    }

    public void Start()
    {
        EventLoopTask?.Dispose();
        EventLoopTask = new Task(ProcessEvents);
        IsEventLoopRunning = true;
        EventLoopTask.Start();
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
            EventLoopTask?.Dispose();
            _disposed = true;
        }
    }
}
