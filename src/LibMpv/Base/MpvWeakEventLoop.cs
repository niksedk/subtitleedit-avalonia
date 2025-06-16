using HanumanInstitute.LibMpv.Core;

namespace HanumanInstitute.LibMpv;

public unsafe class MpvWeakEventLoop: IEventLoop
{
    protected readonly object WakeUpLock = new();
    protected volatile bool IsEventLoopRunning;
    private readonly MpvHandle* _context;
    private readonly Action<MpvEvent> _handleEvent;
    private readonly MpvSetWakeupCallbackCb _wakeupCallback;
    
    public MpvWeakEventLoop(MpvHandle* context, Action<MpvEvent> eventHandler)
    {
        this._context = context;
        _handleEvent = eventHandler;
        _wakeupCallback = WakeupCallback;
    }

    private void WakeupHandleEvent()
    {
        lock (WakeUpLock)
        {
            while (IsEventLoopRunning)
            {
                var eventPtr = MpvApi.WaitEvent(_context, 0);
                if (eventPtr != null)
                {
                    var @event = MarshalHelper.PtrToStructure<MpvEvent>((nint)eventPtr);
                    if (@event.EventId != MpvEventId.None)
                    {
                        _handleEvent(@event);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }


    private void WakeupCallback(void* _)
    {
        Console.WriteLine("WakeupCallback");
        if (IsEventLoopRunning)
        {
            Task.Run(() =>
            {
                WakeupHandleEvent();
                return Task.CompletedTask;
            });
        }
    }

    public void Stop()
    {
        IsEventLoopRunning = false;
    }

    public void Start()
    {
        IsEventLoopRunning = true;
        MpvApi.SetWakeupCallback(_context, _wakeupCallback, null);
    }
}

