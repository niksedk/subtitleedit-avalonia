using System.Diagnostics;
using HanumanInstitute.LibMpv.Core;

namespace HanumanInstitute.LibMpv;

public unsafe partial class MpvContextBase
{
    private Dictionary<MpvEventId, MpvEventHandler> _eventHandlers;
    private delegate void MpvEventHandler(MpvEvent e);

    public event EventHandler? Shutdown;
    public event EventHandler<MpvStartFileEventArgs>? StartFile;
    public event EventHandler<MpvEndFileEventArgs>? EndFile;
    public event EventHandler? FileLoaded;
    public event EventHandler? Idle;
    public event EventHandler? Tick;
    public event EventHandler? VideoReconfig;
    public event EventHandler? AudioReconfig;
    public event EventHandler? SeekRaised;
    public event EventHandler? PlaybackRestart;
    public event EventHandler? QueueOverflow;
    public event EventHandler<MpvPropertyEventArgs>? PropertyChanged;
    public event EventHandler<MpvLogMessageEventArgs>? LogMessage;

    private void InitEventHandlers()
    {
        _eventHandlers = new Dictionary<MpvEventId, MpvEventHandler>()
        {
            { MpvEventId.None, TraceHandler },
            { MpvEventId.Shutdown, ShutdownHandler },
            { MpvEventId.LogMessage, LogMessageHandler },
            { MpvEventId.GetPropertyReply, AsyncGetPropertyHandler },
            { MpvEventId.SetPropertyReply, AsyncSetPropertyHandler },
            { MpvEventId.CommandReply, AsyncCommandReplyHandler },
            { MpvEventId.StartFile, StartFileHandler },
            { MpvEventId.EndFile, EndFileHandler },
            { MpvEventId.FileLoaded, FileLoadedHandler },
            { MpvEventId.EventIdle, IdleHandler },
            { MpvEventId.EventTick, TickHandler },
            { MpvEventId.EventClientMessage, TraceHandler },
            { MpvEventId.EventVideoReconfig, VideoReconfigHandler },
            { MpvEventId.EventAudioReconfig, AudioReconfigHandler },
            { MpvEventId.Seek, SeekHandler },
            { MpvEventId.PlaybackRestart, PlaybackRestartHandler },
            { MpvEventId.PropertyChange, PropertyChangedHandler },
            { MpvEventId.QueueOverflow, QueueOverflowHandler },
            { MpvEventId.Hook, TraceHandler },
        };
    }

    private void LogMessageHandler(MpvEvent e)
    {
        if (e.Data != null && LogMessage != null)
        {
            LogMessage?.Invoke(this, ToLogMessageEventArgs(e));
        }
    }

    private void AsyncSetPropertyHandler(MpvEvent e)
    {
        SetProperty_Reply(ToPropertyChangedEventArgs(e));
    }

    private void AsyncGetPropertyHandler(MpvEvent e)
    {
        GetProperty_Reply(ToPropertyChangedEventArgs(e));
    }

    private void AsyncCommandReplyHandler(MpvEvent e)
    {
        if (e.Data != null)
        {
            Command_Reply(ToCommandEventArgs(e));
        }
    }

    private void PropertyChangedHandler(MpvEvent e)
    {
        if (e.Data != null && PropertyChanged != null)
        {
            PropertyChanged?.Invoke(this, ToPropertyChangedEventArgs(e));
        }
    }

    private void QueueOverflowHandler(MpvEvent e)
    {
        QueueOverflow?.Invoke(this, EventArgs.Empty);
    }

    private void PlaybackRestartHandler(MpvEvent e)
    {
        PlaybackRestart?.Invoke(this, EventArgs.Empty);
    }

    private void SeekHandler(MpvEvent e)
    {
        SeekRaised?.Invoke(this, EventArgs.Empty);
    }

    private void AudioReconfigHandler(MpvEvent e)
    {
        AudioReconfig?.Invoke(this, EventArgs.Empty);
    }

    private void VideoReconfigHandler(MpvEvent e)
    {
        VideoReconfig?.Invoke(this, EventArgs.Empty);
    }

    private void TickHandler(MpvEvent e)
    {
        Tick?.Invoke(this, EventArgs.Empty);
    }

    private void IdleHandler(MpvEvent e)
    {
        Idle?.Invoke(this, EventArgs.Empty);
    }

    private void FileLoadedHandler(MpvEvent mpvEvent)
    {
        FileLoaded?.Invoke(this, EventArgs.Empty);
    }

    private void EndFileHandler(MpvEvent e)
    {
        if (e.Data != null && EndFile != null)
        {
            var endFile = MarshalHelper.PtrToStructure<MpvEventEndFile>((nint) e.Data);
            EndFile?.Invoke(this, new MpvEndFileEventArgs(endFile.Reason, endFile.Error, endFile.PlaylistEntryId));
        }
    }

    private void StartFileHandler(MpvEvent e)
    {
        if (e.Data != null && StartFile != null)
        {
            var startFile = MarshalHelper.PtrToStructure<MpvEventStartFile>((nint) e.Data);
            StartFile?.Invoke(this, new MpvStartFileEventArgs(startFile.PlaylistEntryId));
        }
    }

    private void ShutdownHandler(MpvEvent e)
    {
        Shutdown?.Invoke(this, EventArgs.Empty);
    }

    private void TraceHandler(MpvEvent e)
    {
        Debug.WriteLine($"Unhandled MPV Event: {Enum.GetName(typeof(MpvEventId), e.EventId)}");
    }

    private void HandleEvent(MpvEvent e)
    {
        if (_eventHandlers.TryGetValue(e.EventId, out var eventHandler))
        {
            eventHandler.Invoke(e);
        }
    }

    private MpvLogMessageEventArgs ToLogMessageEventArgs(MpvEvent e)
    {
        var logMessage = MarshalHelper.PtrToStructure<MpvEventLogMessage>((nint) e.Data);
        return new MpvLogMessageEventArgs(
            MarshalHelper.PtrToStringUtf8OrEmpty((nint) logMessage.Prefix),
            MarshalHelper.PtrToStringUtf8OrEmpty((nint) logMessage.Level),
            MarshalHelper.PtrToStringUtf8OrEmpty((nint) logMessage.Text),
            logMessage.LogLevel
        );
    }

    private MpvCommandReplyEventArgs ToCommandEventArgs(MpvEvent e)
    {
        var command = MarshalHelper.PtrToStructure<MpvEventCommand>((nint)e.Data);
        return new MpvCommandReplyEventArgs(command.Result, e);
    }

    private MpvPropertyEventArgs ToPropertyChangedEventArgs(MpvEvent e)
    {
        e.Error.CheckCode();
        var property = e.Data != null ? MarshalHelper.PtrToStructure<MpvEventProperty>((nint) e.Data) : new MpvEventProperty();
        var name = MarshalHelper.PtrToStringUtf8OrEmpty((nint) property.Name);
        return new MpvPropertyEventArgs(property.Format, name, (nint)property.Data, e);
    }
}
