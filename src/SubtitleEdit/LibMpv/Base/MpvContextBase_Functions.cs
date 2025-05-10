// ReSharper disable InvalidXmlDocComment

using HanumanInstitute.LibMpv.Core;

namespace HanumanInstitute.LibMpv;

public unsafe partial class MpvContextBase
{
    /// <summary>Signal to all async requests with the matching ID to abort.</summary>
    /// <param name="requestId">ID of the request to be aborted.</param>
    public void AbortAsyncCommand(ulong requestId) => MpvApi.AbortAsyncCommand(Ctx, requestId);
    
    /// <summary>Return the MPV_CLIENT_API_VERSION the mpv source has been compiled with.</summary>
    public ulong ClientApiVersion() => MpvApi.ClientApiVersion();
    
    /// <summary>Return the ID of this client handle. Every client has its own unique ID. This ID is never reused by the core, even if the mpv_handle at hand gets destroyed and new handles get allocated.</summary>
    /// <returns>The client ID.</returns>
    public long ClientId() => MpvApi.ClientId(Ctx);
    
    /// <summary>Return the name of this client handle. Every client has its own unique name, which is mostly used for user interface purposes.</summary>
    /// <returns>The client name. The string is read-only and is valid until the mpv_handle is destroyed.</returns>
    public string ClientName() => MpvApi.ClientName(Ctx);

    /// <summary>Send a command to the player. Commands are the same as those used in input.conf, except that this function takes parameters in a pre-split form.</summary>
    /// <param name="args">List of strings. Usually, the first item is the command, and the following items are arguments.</param>
    public void RunCommand(MpvCommandOptions? options, params object?[] args)
    {
        var cmd = AddCommandPrefixes(options, args);
        using var helper = new MarshalHelper();
        var val = (byte**)helper.CStringArrayForManagedUtf8StringArray(cmd);
        MpvApi.Command(Ctx, val).CheckCode();
    }

    /// <summary>Send a command to the player. Commands are the same as those used in input.conf, except that this function takes parameters in a pre-split form.</summary>
    /// <param name="args">List of strings. Usually, the first item is the command, and the following items are arguments.</param>
    public T? RunCommandRet<T>(MpvCommandOptions? options, params object?[] args)
    {
        var cmd = AddCommandPrefixes(options, args);
        cmd = cmd.Select(x => x.ToStringInvariant()).ToArray();
        using var helper = new MarshalHelper();
        var val = (byte**)helper.CStringArrayForManagedUtf8StringArray(cmd);
        var ret = new MpvNode();
        MpvApi.CommandRet(Ctx, val, &ret).CheckCode();
        return ret.Parse<T>();
    }

    /// <summary>Same as mpv_command, but run the command asynchronously.</summary>
    /// <param name="requestId">the value mpv_event.requestId of the reply will be set to (see section about asynchronous calls)</param>
    /// <param name="args">List of strings. Usually, the first item is the command, and the following items are arguments.</param>
    public void RunCommandAsync(ulong requestId, string[] args)
    {
        using var helper = new MarshalHelper();
        var val = (byte**)helper.CStringArrayForManagedUtf8StringArray(args);
        MpvApi.CommandAsync(Ctx, requestId, val).CheckCode();
    }

    /// <summary>Same as mpv_command(), but allows passing structured data in any format. In particular, calling mpv_command() is exactly like calling mpv_command_node() with the format set to MPV_FORMAT_NODE_ARRAY, and every arg passed in order as MPV_FORMAT_STRING.</summary>
    /// <param name="args">mpv_node with format set to one of the values documented above (see there for details)</param>
    /// <param name="returnData">Whether to return data from the command. If true and the command succeeds, it must be freed with MpvFreeNodeContents.</param>
    /// <returns>Filled if returnData is true and command succeeds. You must call MpvFreeNodeContents to free it.</returns>
    public MpvNode? RunCommandNode(MpvNode args, bool returnData = false)
    {
        if (returnData)
        {
            var ret = new MpvNode();
            MpvApi.CommandNode(Ctx, &args, &ret).CheckCode();
            return ret;
        }
        MpvApi.CommandNode(Ctx, &args, null).CheckCode();
        return null;
    }
    
    /// <summary>Same as mpv_command_node(), but run it asynchronously. Basically, this function is to mpv_command_node() what mpv_command_async() is to mpv_command().</summary>
    /// <param name="requestId">the value mpv_event.requestId of the reply will be set to (see section about asynchronous calls)</param>
    /// <param name="args">as in mpv_command_node()</param>
    public void RunCommandNodeAsync(ulong requestId, MpvNode args) => MpvApi.CommandNodeAsync(Ctx, requestId, &args).CheckCode();
    
    /// <summary>Same as mpv_command, but use input.conf parsing for splitting arguments. This is slightly simpler, but also more error prone, since arguments may need quoting/escaping.</summary>
    public int RunCommandString(string args) => MpvApi.CommandString(Ctx, args).CheckCode();
    
    /// <summary>Create a new client handle connected to the same player core as Ctx. This context has its own event queue, its own mpv_request_event() state, its own mpv_request_log_messages() state, its own set of observed properties, and its own state for asynchronous operations. Otherwise, everything is shared.</summary>
    /// <param name="name">The client name. This will be returned by mpv_client_name(). If the name is already in use, or contains non-alphanumeric characters (other than '_'), the name is modified to fit. If NULL, an arbitrary name is automatically chosen.</param>
    /// <returns>a new handle, or NULL on error</returns>
    public MpvHandle* CreateClient(string name) => MpvApi.CreateClient(Ctx, name);
    
    /// <summary>This is the same as mpv_create_client(), but the created mpv_handle is treated as a weak reference. If all mpv_handles referencing a core are weak references, the core is automatically destroyed. (This still goes through normal uninit of course. Effectively, if the last non-weak mpv_handle is destroyed, then the weak mpv_handles receive MPV_EVENT_SHUTDOWN and are asked to terminate as well.)</summary>
    /// <param name="name">The client name. This will be returned by mpv_client_name(). If the name is already in use, or contains non-alphanumeric characters (other than '_'), the name is modified to fit. If NULL, an arbitrary name is automatically chosen.</param>
    /// <returns>a new handle, or NULL on error</returns>
    public MpvHandle* CreateWeakClient(string name) => MpvApi.CreateWeakClient(Ctx, name);
    
    /// <summary>Convenience function to delete a property.</summary>
    /// <param name="name">The property name. See input.rst for a list of properties.</param>
    public void DelProperty(string name) => MpvApi.DelProperty(Ctx, name).CheckCode();
    
    /// <summary>Disconnect and destroy the mpv_handle. Ctx will be deallocated with this API call.</summary>
    public void Destroy(MpvHandle* ctx) => MpvApi.Destroy(ctx);
    
    /// <summary>Return a string describing the error. For unknown errors, the string "unknown error" is returned.</summary>
    /// <param name="error">error number, see enum mpv_error</param>
    /// <returns>A string describing the error. The string is completely static, i.e. doesn't need to be deallocated, and is valid forever.</returns>
    public string ErrorString(int error) => MpvApi.ErrorString(error);
    
    /// <summary>Return a string describing the event. For unknown events, NULL is returned.</summary>
    /// <param name="eventId">event ID, see see enum mpv_event_id</param>
    /// <returns>A string giving a short symbolic name of the event. It consists of lower-case alphanumeric characters and can include "-" characters. This string is suitable for use in e.g. scripting interfaces. The string is completely static, i.e. doesn't need to be deallocated, and is valid forever.</returns>
    public string? EventName(MpvEventId eventId) => MpvApi.EventName(eventId);
    
    /// <summary>General function to deallocate memory returned by some of the API functions. Call this only if it's explicitly documented as allowed. Calling this on mpv memory not owned by the caller will lead to undefined behavior.</summary>
    /// <param name="data">A valid pointer returned by the API, or NULL.</param>
    public void Free(void* data) => MpvApi.Free(data);
    
    /// <summary>Frees any data referenced by the node. It doesn't free the node itself. Call this only if the mpv client API set the node. If you constructed the node yourself (manually), you have to free it yourself.</summary>
    public void FreeNodeContents(MpvNode* node) => MpvApi.FreeNodeContents(node);
    
    /// <summary>Read the value of the given property.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="format">see enum mpv_format.</param>
    /// <param name="data">Pointer to the variable holding the option value. On success, the variable will be set to a copy of the option value. For formats that require dynamic memory allocation, you can free the value with mpv_free() (strings) or mpv_free_node_contents() (MPV_FORMAT_NODE).</param>
    /// <returns>error code</returns>
    public int GetProperty(string name, MpvFormat format, void* data) => MpvApi.GetProperty(Ctx, name, format, data);

    /// <summary>Read the value of the given property.</summary>
    /// <param name="name">The property name.</param>
    /// <typeparam name="T">The data type of the property to read.</typeparam>
    public T GetProperty<T>(string name) => MpvFormatter.GetProperty<T>(Ctx, name);

    /// <summary>Return the property as "OSD" formatted string. This is the same as mpv_get_property_string, but using MPV_FORMAT_OSD_STRING.</summary>
    /// <returns>Property value, or NULL if the property can't be retrieved. Free the string with mpv_free().</returns>
    public string? GetPropertyOsdString(string name)
    {
        var value = MpvApi.GetPropertyOsdString(Ctx, name);
        return value != null ? Utf8Marshaler.FromNative(value, Encoding.UTF8) : null;
    }

    /// <summary>Return the value of the property with the given name as string. This is equivalent to mpv_get_property() with MPV_FORMAT_STRING.</summary>
    /// <param name="name">The property name.</param>
    /// <returns>Property value, or NULL if the property can't be retrieved. Free the string with mpv_free().</returns>
    public string? GetPropertyString(string name)
    {
        var value = MpvApi.GetPropertyString(Ctx, name);
        return value != null ? Utf8Marshaler.FromNative(value, Encoding.UTF8) : null;
    }
    
    /// <summary>Get a property asynchronously. You will receive the result of the operation as well as the property data with the MPV_EVENT_GET_PROPERTY_REPLY event. You should check the mpv_event.error field on the reply event.</summary>
    /// <param name="requestId">see section about asynchronous calls</param>
    /// <param name="name">The property name.</param>
    /// <param name="format">see enum mpv_format.</param>
    protected int GetPropertyAsync(ulong requestId, string name, MpvFormat format) => MpvApi.GetPropertyAsync(Ctx, requestId, name, format);

    /// <summary>Return the internal time in microseconds. This has an arbitrary start offset, but will never wrap or go backwards.</summary>
    public long GetTimeUs() => MpvApi.GetTimeUs(Ctx);
    
    /// <summary>A hook is like a synchronous event that blocks the player. You register a hook handler with this function. You will get an event, which you need to handle, and once things are ready, you can let the player continue with mpv_hook_continue().</summary>
    /// <param name="requestId">This will be used for the mpv_event.requestId field for the received MPV_EVENT_HOOK events. If you have no use for this, pass 0.</param>
    /// <param name="name">The hook name. This should be one of the documented names. But if the name is unknown, the hook event will simply be never raised.</param>
    /// <param name="priority">See remarks above. Use 0 as a neutral default.</param>
    public void HookAdd(ulong requestId, string name, int priority) => MpvApi.HookAdd(Ctx, requestId, name, priority).CheckCode();
    
    /// <summary>Respond to a MPV_EVENT_HOOK event. You must call this after you have handled the event. There is no way to "cancel" or "stop" the hook.</summary>
    /// <param name="id">This must be the value of the mpv_event_hook.id field for the corresponding MPV_EVENT_HOOK.</param>
    public void HookContinue(ulong id) => MpvApi.HookContinue(Ctx, id).CheckCode();

    /// <summary>Initialize an uninitialized mpv instance. If the mpv instance is already running, an error is returned.</summary>
    public void Initialize() => MpvApi.Initialize(Ctx).CheckCode();

    /// <summary>Load a config file. This loads and parses the file, and sets every entry in the config file's default section as if mpv_set_option_string() is called.</summary>
    /// <param name="filename">absolute path to the config file on the local filesystem</param>
    public void LoadConfigFile(string filename) => MpvApi.LoadConfigFile(Ctx, filename).CheckCode();
    
    /// <summary>Get a notification whenever the given property changes. You will receive updates as MPV_EVENT_PROPERTY_CHANGE. Note that this is not very precise: for some properties, it may not send updates even if the property changed. This depends on the property, and it's a valid feature request to ask for better update handling of a specific property. (For some properties, like ``clock``, which shows the wall clock, this mechanism doesn't make too much sense anyway.)</summary>
    /// <param name="requestId">This will be used for the mpv_event.requestId field for the received MPV_EVENT_PROPERTY_CHANGE events. (Also see section about asynchronous calls, although this function is somewhat different from actual asynchronous calls.) If you have no use for this, pass 0. Also see mpv_unobserve_property().</param>
    /// <param name="name">The property name.</param>
    /// <param name="format">see enum mpv_format. Can be MPV_FORMAT_NONE to omit values from the change events.</param>
    public void ObserveProperty(ulong requestId, string name, MpvFormat format) => MpvApi.ObserveProperty(Ctx, requestId, name, format).CheckCode();
    
    /// <summary>Initialize the renderer state. Depending on the backend used, this will access the underlying GPU API and initialize its own objects.</summary>
    /// <param name="params">an array of parameters, terminated by type==0. It's left unspecified what happens with unknown parameters. At least MPV_RENDER_PARAM_API_TYPE is required, and most backends will require another backend-specific parameter.</param>
    /// <returns>Set to the context (on success) or NULL (on failure).</returns>
    public void RenderContextCreate(MpvRenderParam* @params)
    {
        MpvRenderContext* result = null;
        MpvApi.RenderContextCreate(&result, Ctx, @params).CheckCode();
        _renderContext = result;
    }

    /// <summary>Destroy the mpv renderer state.</summary>
    public void RenderContextFree()
    {
        if (_renderContext != null)
        {
            MpvApi.RenderContextFree(_renderContext);
            _renderContext = null;
        }
    }

    /// <summary>Retrieve information from the render context. This is NOT a counterpart to mpv_render_context_set_parameter(), because you generally can't read parameters set with it, and this function is not meant for this purpose. Instead, this is for communicating information from the renderer back to the user. See mpv_render_param_type; entries which support this function explicitly mention it, and for other entries you can assume it will fail.</summary>
    /// <param name="param">the parameter type and data that should be retrieved</param>
    public void RenderContextGetInfo(MpvRenderParam param) => MpvApi.RenderContextGetInfo(_renderContext, param).CheckCode();
    
    /// <summary>Render video.</summary>
    /// <param name="params">an array of parameters, terminated by type==0. Which parameters are required depends on the backend. It's left unspecified what happens with unknown parameters.</param>
    public void RenderContextRender(MpvRenderParam* @params) => MpvApi.RenderContextRender(_renderContext, @params).CheckCode();
    
    /// <summary>Tell the renderer that a frame was flipped at the given time. This is optional, but can help the player to achieve better timing.</summary>
    public void RenderContextReportSwap() => MpvApi.RenderContextReportSwap(_renderContext);
    
    /// <summary>Attempt to change a single parameter. Not all backends and parameter types support all kinds of changes.</summary>
    /// <param name="param">the parameter type and data that should be set</param>
    public void RenderContextSetParameter(MpvRenderParam param) => MpvApi.RenderContextSetParameter(_renderContext, param).CheckCode();
    
    /// <summary>Set the callback that notifies you when a new video frame is available, or if the video display configuration somehow changed and requires a redraw. Similar to mpv_set_wakeup_callback(), you must not call any mpv API from the callback, and all the other listed restrictions apply (such as not exiting the callback by throwing exceptions).</summary>
    /// <param name="callback">callback(callbackCtx) is called if the frame should be redrawn</param>
    /// <param name="callbackCtx">opaque argument to the callback</param>
    public void RenderContextSetUpdateCallback(MpvRenderContextSetUpdateCallbackCallbackFunc callback, void* callbackCtx = null) => MpvApi.RenderContextSetUpdateCallback(_renderContext, callback, callbackCtx);
    
    /// <summary>The API user is supposed to call this when the update callback was invoked (like all mpv_render_* functions, this has to happen on the render thread, and _not_ from the update callback itself).</summary>
    /// <returns>a bitset of mpv_render_update_flag values (i.e. multiple flags are combined with bitwise or). Typically, this will tell the API user what should happen next. E.g. if the MPV_RENDER_UPDATE_FRAME flag is set, mpv_render_context_render() should be called. If flags unknown to the API user are set, or if the return value is 0, nothing needs to be done.</returns>
    public ulong RenderContextUpdate() => MpvApi.RenderContextUpdate(_renderContext);
    
    /// <summary>Enable or disable the given event.</summary>
    /// <param name="eventId">See enum mpv_event_id.</param>
    /// <param name="enable">True to enable receiving this event, False to disable it.</param>
    public void RequestEvent(MpvEventId eventId, bool enable) => MpvApi.RequestEvent(Ctx, eventId, enable ? 1 : 0).CheckCode();
    
    /// <summary>Enable or disable receiving of log messages. These are the messages the command line player prints to the terminal. This call sets the minimum required log level for a message to be received with MPV_EVENT_LOG_MESSAGE.</summary>
    /// <param name="minLevel">Minimal log level as string. Valid log levels: no fatal error warn info v debug trace The value "no" disables all messages. This is the default. An exception is the value "terminal-default", which uses the log level as set by the "--msg-level" option. This works even if the terminal is disabled. (Since API version 1.19.) Also see mpv_log_level.</param>
    public void RequestLogMessages(string minLevel) => MpvApi.RequestLogMessages(Ctx, minLevel).CheckCode();
    
    /// <summary>Set an option. Note that you can't normally set options during runtime. It works in uninitialized state (see mpv_create()), and in some cases in at runtime.</summary>
    /// <param name="name">Option name. This is the same as on the mpv command line, but without the leading "--".</param>
    /// <param name="format">see enum mpv_format.</param>
    /// <param name="data">Option value (according to the format).</param>
    public void SetOption(string name, MpvFormat format, void* data) => MpvApi.SetOption(Ctx, name, format, data).CheckCode();
    
    /// <summary>Convenience function to set an option to a string value. This is like calling mpv_set_option() with MPV_FORMAT_STRING.</summary>
    public void SetOptionString(string name, string data) => MpvApi.SetOptionString(Ctx, name, data).CheckCode();
    
    /// <summary>Set a property to a given value. Properties are essentially variables which can be queried or set at runtime. For example, writing to the pause property will actually pause or unpause playback.</summary>
    /// <param name="name">The property name. See input.rst for a list of properties.</param>
    /// <param name="format">see enum mpv_format.</param>
    /// <param name="data">Option value.</param>
    public void SetProperty(string name, MpvFormat format, void* data) => MpvApi.SetProperty(Ctx, name, format, data).CheckCode();
    
    /// <summary>Set a property asynchronously. You will receive the result of the operation as MPV_EVENT_SET_PROPERTY_REPLY event. The mpv_event.error field will contain the result status of the operation. Otherwise, this function is similar to mpv_set_property().</summary>
    /// <param name="requestId">see section about asynchronous calls</param>
    /// <param name="name">The property name.</param>
    /// <param name="format">see enum mpv_format.</param>
    /// <param name="data">Option value. The value will be copied by the function. It will never be modified by the client API.</param>
    protected int SetPropertyAsync(ulong requestId, string name, MpvFormat format, void* data) => MpvApi.SetPropertyAsync(Ctx, requestId, name, format, data);

    public void SetProperty<T>(string name, T value) => MpvFormatter.SetProperty(Ctx, name, value);

    /// <summary>Convenience function to set a property to a string value.</summary>
    public void SetPropertyString(string name, string newValue)
    {
        MpvApi.SetPropertyString(Ctx, name, newValue).CheckCode();
    }
    
    public void SetPropertyFlag(string name, bool newValue)
    {
        var val = newValue ? 1 : 0;
        MpvApi.SetProperty(Ctx, name, MpvFormat.Flag, &val).CheckCode();
    }

    public void SetPropertyLong(string name, long newValue)
    {
        MpvApi.SetProperty(Ctx, name, MpvFormat.Int64, &newValue).CheckCode();
    }

    public void SetPropertyDouble(string name, double newValue)
    {
        MpvApi.SetProperty(Ctx, name, MpvFormat.Double, &newValue).CheckCode();
    }

    /// <summary>Set a custom function that should be called when there are new events. Use this if blocking in mpv_wait_event() to wait for new events is not feasible.</summary>
    /// <param name="cb">function that should be called if a wakeup is required</param>
    /// <param name="d">arbitrary userdata passed to cb</param>
    public void SetWakeupCallback(MpvSetWakeupCallbackCbFunc cb, void* d) => MpvApi.SetWakeupCallback(Ctx, cb, d);
    
    /// <summary>Add a custom stream protocol. This will register a protocol handler under the given protocol prefix, and invoke the given callbacks if an URI with the matching protocol prefix is opened.</summary>
    /// <param name="protocol">protocol prefix, for example "foo" for "foo://" URIs</param>
    /// <param name="userData">opaque pointer passed into the mpv_stream_cb_open_fn callback.</param>
    public void StreamCbAddRo(string protocol, void* userData, MpvStreamCbAddRoOpenFnFunc openFn) => MpvApi.StreamCbAddRo(Ctx, protocol, userData, openFn).CheckCode();
    
    /// <summary>Similar to mpv_destroy(), but brings the player and all clients down as well, and waits until all of them are destroyed. This function blocks. The advantage over mpv_destroy() is that while mpv_destroy() merely detaches the client handle from the player, this function quits the player, waits until all other clients are destroyed (i.e. all mpv_handles are detached), and also waits for the final termination of the player.</summary>
    public void TerminateDestroy() => MpvApi.TerminateDestroy(Ctx);
    
    /// <summary>Undo mpv_observe_property(). This will remove all observed properties for which the given number was passed as requestId to mpv_observe_property.</summary>
    /// <param name="registeredReplyUserData">ID that was passed to mpv_observe_property</param>
    /// <returns>negative value is an error code, >=0 is number of removed properties on success (includes the case when 0 were removed)</returns>
    public int UnobserveProperty(ulong registeredReplyUserData) => MpvApi.UnobserveProperty(Ctx, registeredReplyUserData).CheckCode();
    
    /// <summary>Block until all asynchronous requests are done. This affects functions like mpv_command_async(), which return immediately and return their result as events.</summary>
    public void WaitAsyncRequests() => MpvApi.WaitAsyncRequests(Ctx);
    
    /// <summary>Wait for the next event, or until the timeout expires, or if another thread makes a call to mpv_wakeup(). Passing 0 as timeout will never wait, and is suitable for polling.</summary>
    /// <param name="timeout">Timeout in seconds, after which the function returns even if no event was received. A MPV_EVENT_NONE is returned on timeout. A value of 0 will disable waiting. Negative values will wait with an infinite timeout.</param>
    /// <returns>A struct containing the event ID and other data. The pointer (and fields in the struct) stay valid until the next mpv_wait_event() call, or until the mpv_handle is destroyed. You must not write to the struct, and all memory referenced by it will be automatically released by the API on the next mpv_wait_event() call, or when the context is destroyed. The return value is never NULL.</returns>
    public MpvEvent* WaitEvent(double timeout) => MpvApi.WaitEvent(Ctx, timeout);
    
    /// <summary>Interrupt the current mpv_wait_event() call. This will wake up the thread currently waiting in mpv_wait_event(). If no thread is waiting, the next mpv_wait_event() call will return immediately (this is to avoid lost wake-ups).</summary>
    public void Wakeup() => MpvApi.Wakeup(Ctx);
}
