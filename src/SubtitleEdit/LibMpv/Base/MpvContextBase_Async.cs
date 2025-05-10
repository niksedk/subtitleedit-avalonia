using System.Diagnostics;
using System.Globalization;
using HanumanInstitute.LibMpv.Core;

namespace HanumanInstitute.LibMpv;

/// <summary>
/// Handles basic communication protocols with MPV via IPC named pipe.
/// </summary>
public partial class MpvContextBase
{
    private ulong _requestId = 1;
    private readonly List<MpvEventArgs> _responses = new();
    private bool _logEnabled;
    private readonly object _lockLogEnabled = new object();
    private const bool DefaultWaitForResponse = true;
    private const int DefaultResponseTimeout = 3000;
    private const bool DefaultThrowOnError = false;
    private readonly ManualResetEvent _waitResponse = new(true);

    /// <summary>
    /// Returns a unique incremental request ID to send to MPV API.
    /// </summary>
    /// <returns>A unique ID</returns>
    public ulong UniqueRequestId() => _requestId++;

    /// <summary>
    /// Gets or sets the default options for all requests passing through this controller.
    /// </summary>
    public MpvCommandOptions DefaultOptions { get; } = new MpvCommandOptions()
    {
        WaitForResponse = DefaultWaitForResponse,
        ResponseTimeout = DefaultResponseTimeout,
        ThrowOnError = DefaultThrowOnError
    };

    /// <summary>
    /// Gets whether to wait for response, first taking the value in options, if null taking the value in DefaultOptions, and if null taking a default value.
    /// </summary>
    /// <param name="options">Optional command options, may be null.</param>
    public bool GetWaitForResponseOption(MpvAsyncOptions? options) => options?.WaitForResponse ?? DefaultOptions.WaitForResponse ?? DefaultWaitForResponse;

    /// <summary>
    /// Gets the response timeout, first taking the value in options, if null taking the value in DefaultOptions, and if null taking a default value.
    /// </summary>
    /// <param name="options">Optional command options, may be null.</param>
    public int GetResponseTimeoutOption(MpvAsyncOptions? options) => options?.ResponseTimeout ?? DefaultOptions.ResponseTimeout ?? DefaultResponseTimeout;

    /// <summary>
    /// Gets whether to throw an exception on error, first taking the value in options, if null taking the value in DefaultOptions, and if null taking a default value.
    /// </summary>
    /// <param name="options">Optional command options, may be null.</param>
    public bool GetThrowOnErrorOption(MpvAsyncOptions? options) => options?.ThrowOnError ?? DefaultOptions.ThrowOnError ?? DefaultThrowOnError;

    /// <summary>
    /// Gets a text log of communication data from both directions.
    /// </summary>
    public StringBuilder? Log { get; private set; }

    /// <summary>
    /// Gets or sets whether to keep a log of communication data.
    /// </summary>
    public bool LogEnabled
    {
        get => _logEnabled;
        set
        {
            lock (_lockLogEnabled)
            {
                if (value && !_logEnabled)
                {
                    Log = new StringBuilder();
                }
                else if (!value && _logEnabled)
                {
                    Log = null;
                }
                _logEnabled = value;
            }
        }
    }

    /// <summary>
    /// Occurs when a full message has been received.
    /// </summary>
    /// <param name="e">The message received from the player.</param>
    private void Command_Reply(MpvCommandReplyEventArgs e)
    {
        if (e.ErrorCode != 0) { return; }
        
        if (e.RequestId > 0)
        {
            // Add to list of responses to be retrieved by QueryId.
            lock (_responses)
            {
                _responses.Add(e);
            }
            _waitResponse.Set();
        }
    }
    
    /// <summary>
    /// Occurs when a full message has been received.
    /// </summary>
    /// <param name="e">The message received from the player.</param>
    private void GetProperty_Reply(MpvPropertyEventArgs e)
    {
        if (e.ErrorCode != 0) { return; }
        
        if (e.RequestId > 0)
        {
            // Add to list of responses to be retrieved by QueryId.
            lock (_responses)
            {
                _responses.Add(e);
            }
            _waitResponse.Set();
        }
    }
    
    /// <summary>
    /// Occurs when a full message has been received.
    /// </summary>
    /// <param name="e">The message received from the player.</param>
    private void SetProperty_Reply(MpvEventArgs e)
    {
        if (e.ErrorCode != 0) { return; }
        
        if (e.RequestId > 0)
        {
            // Add to list of responses to be retrieved by QueryId.
            lock (_responses)
            {
                _responses.Add(e);
            }
            _waitResponse.Set();
        }
    }

    protected string[] AddCommandPrefixes(MpvCommandOptions? options, object?[] cmd)
    {
        // Append prefixes and remove null values at the end.
        var cmdLength = cmd.Length;
        var prefixes = options != null ? options.GetPrefixes() : DefaultOptions.GetPrefixes();
        var prefixCount = prefixes.Count;
        for (var i = cmd.Length - 1; i >= 0; i--)
        {
            if (cmd[i] == null)
            {
                cmdLength--;
            }
            else
            {
                break;
            }
        }
        if (cmdLength != cmd.Length || prefixCount > 0)
        {
            var cmd2 = cmd;
            cmd = new object[cmdLength + prefixCount];
            for (var i = 0; i < prefixCount; i++)
            {
                cmd[i] = prefixes![i];
            }
            Array.Copy(cmd2, 0, cmd, prefixCount, cmdLength);
        }
        return cmd.Select(x => x.ToStringInvariant()).ToArray();
    }

    /// <summary>
    /// Sends specified message to MPV and returns the response as string.
    /// </summary>
    /// <param name="options">Additional command options.</param>
    /// <param name="cmd">The command values to send.</param>
    /// <returns>The server's response to the command.</returns>
    /// <exception cref="InvalidOperationException">The response contained an error and ThrowOnError is True.</exception>
    /// <exception cref="TimeoutException">A response from MPV was not received before timeout.</exception>
    /// <exception cref="FormatException">The data returned by the server could not be parsed.</exception>
    /// <exception cref="ObjectDisposedException">The underlying connection was disposed.</exception>
    public async Task<T?> CommandAsync<T>(MpvCommandOptions? options, params object?[] args)
    {
        args.CheckNotNullOrEmpty(nameof(args));

        var cmd = AddCommandPrefixes(options, args);
        
        // Prepare the request.
        var requestId = GetWaitForResponseOption(options) ? UniqueRequestId() : 0;

        // Send the request.
        RunCommandAsync(requestId, cmd);

        if (requestId > 0 && 
            await WaitForResponseAsync(requestId, cmd[0] ?? string.Empty, options) is MpvCommandReplyEventArgs result)
        {
            return result.Data.Parse<T>();
        }
        return default;
    }

    /// <summary>
    /// Sends specified message to MPV and returns the response.
    /// </summary>
    /// <param name="name">The property to get.</param>
    /// <param name="options">Additional command options.</param>
    /// <returns>The server's response to the command.</returns>
    /// <exception cref="InvalidOperationException">The response contained an error and ThrowOnError is True.</exception>
    /// <exception cref="TimeoutException">A response from MPV was not received before timeout.</exception>
    /// <exception cref="FormatException">The data returned by the server could not be parsed.</exception>
    /// <exception cref="ObjectDisposedException">The underlying connection was disposed.</exception>
    public async Task<T?> GetPropertyAsync<T>(string name, MpvAsyncOptions? options)
    {
        // Prepare the request.
        var requestId = GetWaitForResponseOption(options) ? UniqueRequestId() : 0;

        // Send the request.
        var format = MpvFormatter.GetMpvFormat<T?>();
        GetPropertyAsync(requestId, name, format);

        if (requestId > 0 && 
            await WaitForResponseAsync(requestId, name, options) is MpvPropertyEventArgs result && 
            typeof(T) != typeof(object))
        {
            return MpvFormatter.ParseData<T>(result.Data);
        }
        return default;
    }
    
    public async Task SetPropertyAsync<T>(string name, T newValue, MpvAsyncOptions? options = null)
    {
        var requestId = GetWaitForResponseOption(options) ? UniqueRequestId() : 0;
        unsafe
        {
            MpvFormatter.SetPropertyAsync(Ctx, name, newValue, requestId);
        }
        if (requestId > 0)
        {
            await WaitForResponseAsync(requestId, name, options);
        }
    }

    /// <summary>
    /// Waits for a response with specified request ID.
    /// </summary>
    /// <param name="requestId">The request ID to wait for.</param>
    /// <param name="commandName">The name of the command being executed.</param>
    /// <param name="options">Additional command options.</param>
    private async Task<MpvEventArgs?> WaitForResponseAsync(ulong requestId, string commandName, MpvAsyncOptions? options = null, CancellationToken? cancelToken = null)
    {
        // Wait for response with matching RequestId.
        var watch = new Stopwatch();
        watch.Start();
        var response = FindResponse(requestId);
        var maxTimeout = GetResponseTimeoutOption(options);
        while (response == null && (maxTimeout < 0 || watch.ElapsedMilliseconds < maxTimeout) && cancelToken?.IsCancellationRequested != true)
        {
            // Calculate wait timeout.
            var timeout = -1;
            if (maxTimeout > -1)
            {
                timeout = (int)(maxTimeout - watch.ElapsedMilliseconds);
                timeout = timeout < 0 ? 0 : timeout > 1000 ? 1000 : timeout;
            }

            // Wait until any message is received.
            _waitResponse.Reset();
            await _waitResponse.WaitOneAsync(timeout, cancelToken);
            response = FindResponse(requestId);
        }

        // Timeout.
        if (response == null)
        {
            throw new TimeoutException($"A response from MPV to request_id={requestId} was not received before timeout.");
        }
        else
        {
            // Remove response from list.
            lock (_responses)
            {
                _responses.Remove(response);
            }
        }

        if (GetThrowOnErrorOption(options) && response.ErrorCode < 0)
        {
            throw MpvException.FromCode(response.ErrorCode); 
        }
        return response.ErrorCode >= 0 ? response : null;
    }

    /// <summary>
    /// Returns the response with specified ID from the list of received responses.
    /// </summary>
    /// <param name="requestId">The request id to look for.</param>
    /// <returns>The response with matching id.</returns>
    private MpvEventArgs? FindResponse(ulong requestId)
    {
        lock (_responses)
        {
            return _responses.FirstOrDefault(x => x.RequestId == requestId);
        }
    }
}
