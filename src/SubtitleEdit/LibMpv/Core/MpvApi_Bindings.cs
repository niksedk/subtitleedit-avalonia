using System.Security;

namespace HanumanInstitute.LibMpv.Core;

public static unsafe partial class MpvApi
{
    private static IFunctionResolver FunctionResolver => s_functionResolver ??= FunctionResolverFactory.Create();
    private static IFunctionResolver? s_functionResolver;

    private static T GetFunction<T>(string functionName)
        where T : Delegate =>
        FunctionResolver.GetFunctionDelegate<T>(DllName, functionName) ?? throw new NotSupportedException();
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AbortAsyncCommandDelegate(MpvHandle* ctx, ulong replyUserData);
    public static AbortAsyncCommandDelegate AbortAsyncCommand => s_abortAsyncCommandDelegate ??= GetFunction<AbortAsyncCommandDelegate>("mpv_abort_async_command");
    private static AbortAsyncCommandDelegate? s_abortAsyncCommandDelegate;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ulong ClientApiVersionDelegate();
    public static ClientApiVersionDelegate ClientApiVersion => s_clientApiVersion ??= GetFunction<ClientApiVersionDelegate>("mpv_client_api_version");
    private static ClientApiVersionDelegate? s_clientApiVersion;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate long ClientIdDelegate(MpvHandle* ctx);
    public static ClientIdDelegate ClientId => s_clientId ??= GetFunction<ClientIdDelegate>("mpv_client_id");
    private static ClientIdDelegate? s_clientId;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
    public delegate string ClientNameDelegate(MpvHandle* ctx);
    public static ClientNameDelegate ClientName => s_clientName ??= GetFunction<ClientNameDelegate>("mpv_client_name");
    private static ClientNameDelegate? s_clientName;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int CommandDelegate(MpvHandle* ctx, byte** args);
    public static CommandDelegate Command => s_command ??= GetFunction<CommandDelegate>("mpv_command");
    private static CommandDelegate? s_command;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int CommandAsyncDelegate(MpvHandle* ctx, ulong replyUserData, byte** args);
    public static CommandAsyncDelegate CommandAsync => s_commandAsync ??= GetFunction<CommandAsyncDelegate>("mpv_command_async");
    private static CommandAsyncDelegate? s_commandAsync;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int CommandNodeDelegate(MpvHandle* ctx, MpvNode* args, MpvNode* result);
    public static CommandNodeDelegate CommandNode => s_commandNode ??= GetFunction<CommandNodeDelegate>("mpv_command_node");
    private static CommandNodeDelegate? s_commandNode;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int CommandNodeAsyncDelegate(MpvHandle* ctx, ulong replyUserData, MpvNode* args);
    public static CommandNodeAsyncDelegate CommandNodeAsync => s_commandNodeAsync ??= GetFunction<CommandNodeAsyncDelegate>("mpv_command_node_async");
    private static CommandNodeAsyncDelegate? s_commandNodeAsync;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int CommandRetDelegate(MpvHandle* ctx, byte** args, MpvNode* result);
    public static CommandRetDelegate CommandRet => s_commandRet ??= GetFunction<CommandRetDelegate>("mpv_command_ret");
    private static CommandRetDelegate? s_commandRet;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int CommandStringDelegate(MpvHandle* ctx,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string args);
    public static CommandStringDelegate CommandString => s_commandString ??= GetFunction<CommandStringDelegate>("mpv_command_string");
    private static CommandStringDelegate? s_commandString;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate MpvHandle* CreateDelegate();
    public static CreateDelegate Create => s_create ??= GetFunction<CreateDelegate>("mpv_create");
    private static CreateDelegate? s_create;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate MpvHandle* CreateClientDelegate(MpvHandle* ctx,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string name);
    public static CreateClientDelegate CreateClient => s_createClient ??= GetFunction<CreateClientDelegate>("mpv_create_client");
    private static CreateClientDelegate? s_createClient;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate MpvHandle* CreateWeakClientDelegate(MpvHandle* ctx,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string name);
    public static CreateWeakClientDelegate CreateWeakClient => s_createWeakClient ??= GetFunction<CreateWeakClientDelegate>("mpv_create_weak_client");
    private static CreateWeakClientDelegate? s_createWeakClient;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelPropertyDelegate(MpvHandle* ctx,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string name);
    public static DelPropertyDelegate DelProperty => s_delProperty ??= GetFunction<DelPropertyDelegate>("mpv_del_property");
    private static DelPropertyDelegate? s_delProperty;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DestroyDelegate(MpvHandle* ctx);
    public static DestroyDelegate Destroy => s_destroy ??= GetFunction<DestroyDelegate>("mpv_destroy");
    private static DestroyDelegate? s_destroy;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
    public delegate string ErrorStringDelegate(int error);
    public static ErrorStringDelegate ErrorString => s_errorString ??= GetFunction<ErrorStringDelegate>("mpv_error_string");
    private static ErrorStringDelegate? s_errorString;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
    public delegate string? EventNameDelegate(MpvEventId eventId);
    public static EventNameDelegate EventName => s_eventName ??= GetFunction<EventNameDelegate>("mpv_event_name");
    private static EventNameDelegate? s_eventName;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int EventToNodeDelegate(MpvNode* dst, MpvEvent* src);
    public static EventToNodeDelegate EventToNode => s_eventToNode ??= GetFunction<EventToNodeDelegate>("mpv_event_to_node");
    private static EventToNodeDelegate? s_eventToNode;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FreeDelegate(void* data);
    public static FreeDelegate Free => s_free ??= GetFunction<FreeDelegate>("mpv_free");
    private static FreeDelegate? s_free;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FreeNodeContentsDelegate(MpvNode* node);
    public static FreeNodeContentsDelegate FreeNodeContents => s_freeNodeContents ??= GetFunction<FreeNodeContentsDelegate>("mpv_free_node_contents");
    private static FreeNodeContentsDelegate? s_freeNodeContents;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int GetPropertyDelegate(MpvHandle* ctx,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string name, MpvFormat format, void* data);
    public static GetPropertyDelegate GetProperty => s_getProperty ??= GetFunction<GetPropertyDelegate>("mpv_get_property");
    private static GetPropertyDelegate? s_getProperty;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int GetPropertyAsyncDelegate(MpvHandle* ctx, ulong replyUserData,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string name, MpvFormat format);
    public static GetPropertyAsyncDelegate GetPropertyAsync => s_getPropertyAsync ??= GetFunction<GetPropertyAsyncDelegate>("mpv_get_property_async");
    private static GetPropertyAsyncDelegate? s_getPropertyAsync;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate byte* GetPropertyOsdStringDelegate(MpvHandle* ctx,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string name);
    public static GetPropertyOsdStringDelegate GetPropertyOsdString => s_getPropertyOsdString ??= GetFunction<GetPropertyOsdStringDelegate>("mpv_get_property_osd_string");
    private static GetPropertyOsdStringDelegate? s_getPropertyOsdString;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate byte* GetPropertyStringDelegate(MpvHandle* ctx,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string name);
    public static GetPropertyStringDelegate GetPropertyString => s_getPropertyString ??= GetFunction<GetPropertyStringDelegate>("mpv_get_property_string");
    private static GetPropertyStringDelegate? s_getPropertyString;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate long GetTimeUsDelegate(MpvHandle* ctx);
    public static GetTimeUsDelegate GetTimeUs => s_getTimeUs ??= GetFunction<GetTimeUsDelegate>("mpv_get_time_us");
    private static GetTimeUsDelegate? s_getTimeUs;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int GetWakeupPipeDelegate(MpvHandle* ctx);
    public static GetWakeupPipeDelegate GetWakeupPipe => s_getWakeupPipe ??= GetFunction<GetWakeupPipeDelegate>("mpv_get_wakeup_pipe");
    private static GetWakeupPipeDelegate? s_getWakeupPipe;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int HookAddDelegate(MpvHandle* ctx, ulong replyUserData,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string name, int priority);
    public static HookAddDelegate HookAdd => s_hookAdd ??= GetFunction<HookAddDelegate>("mpv_hook_add");
    private static HookAddDelegate? s_hookAdd;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int HookContinueDelegate(MpvHandle* ctx, ulong id);
    public static HookContinueDelegate HookContinue => s_hookContinue ??= GetFunction<HookContinueDelegate>("mpv_hook_continue");
    private static HookContinueDelegate? s_hookContinue;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int InitializeDelegate(MpvHandle* ctx);
    public static InitializeDelegate Initialize => s_initialize ??= GetFunction<InitializeDelegate>("mpv_initialize");
    private static InitializeDelegate? s_initialize;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int LoadConfigFileDelegate(MpvHandle* ctx,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string filename);
    public static LoadConfigFileDelegate LoadConfigFile => s_loadConfigFile ??= GetFunction<LoadConfigFileDelegate>("mpv_load_config_file");
    private static LoadConfigFileDelegate? s_loadConfigFile;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int ObservePropertyDelegate(MpvHandle* mpv, ulong replyUserData,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string name, MpvFormat format);
    public static ObservePropertyDelegate ObserveProperty => s_observeProperty ??= GetFunction<ObservePropertyDelegate>("mpv_observe_property");
    private static ObservePropertyDelegate? s_observeProperty;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int RenderContextCreateDelegate(MpvRenderContext** res, MpvHandle* mpv, MpvRenderParam* @params);
    public static RenderContextCreateDelegate RenderContextCreate => s_renderContextCreate ??= GetFunction<RenderContextCreateDelegate>("mpv_render_context_create");
    private static RenderContextCreateDelegate? s_renderContextCreate;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RenderContextFreeDelegate(MpvRenderContext* ctx);
    public static RenderContextFreeDelegate RenderContextFree => s_renderContextFree ??= GetFunction<RenderContextFreeDelegate>("mpv_render_context_free");
    private static RenderContextFreeDelegate? s_renderContextFree;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int RenderContextGetInfoDelegate(MpvRenderContext* ctx, MpvRenderParam param);
    public static RenderContextGetInfoDelegate RenderContextGetInfo => s_renderContextGetInfo ??= GetFunction<RenderContextGetInfoDelegate>("mpv_render_context_get_info");
    private static RenderContextGetInfoDelegate? s_renderContextGetInfo;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int RenderContextRenderDelegate(MpvRenderContext* ctx, MpvRenderParam* @params);
    public static RenderContextRenderDelegate RenderContextRender => s_renderContextRender ??= GetFunction<RenderContextRenderDelegate>("mpv_render_context_render");
    private static RenderContextRenderDelegate? s_renderContextRender;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RenderContextReportSwapDelegate(MpvRenderContext* ctx);
    public static RenderContextReportSwapDelegate RenderContextReportSwap => s_renderContextReportSwap ??= GetFunction<RenderContextReportSwapDelegate>("mpv_render_context_report_swap");
    private static RenderContextReportSwapDelegate? s_renderContextReportSwap;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int RenderContextSetParameterDelegate(MpvRenderContext* ctx, MpvRenderParam param);
    public static RenderContextSetParameterDelegate RenderContextSetParameter => s_renderContextSetParameter ??= GetFunction<RenderContextSetParameterDelegate>("mpv_render_context_set_parameter");
    private static RenderContextSetParameterDelegate? s_renderContextSetParameter;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RenderContextSetUpdateCallbackDelegate(MpvRenderContext* ctx, MpvRenderContextSetUpdateCallbackCallbackFunc callback, void* callbackCtx);
    public static RenderContextSetUpdateCallbackDelegate RenderContextSetUpdateCallback => s_renderContextSetUpdateCallback ??= GetFunction<RenderContextSetUpdateCallbackDelegate>("mpv_render_context_set_update_callback");
    private static RenderContextSetUpdateCallbackDelegate? s_renderContextSetUpdateCallback;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ulong RenderContextUpdateDelegate(MpvRenderContext* ctx);
    public static RenderContextUpdateDelegate RenderContextUpdate => s_renderContextUpdate ??= GetFunction<RenderContextUpdateDelegate>("mpv_render_context_update");
    private static RenderContextUpdateDelegate? s_renderContextUpdate;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int RequestEventDelegate(MpvHandle* ctx, MpvEventId eventId, int enable);
    public static RequestEventDelegate RequestEvent => s_requestEvent ??= GetFunction<RequestEventDelegate>("mpv_request_event");
    private static RequestEventDelegate? s_requestEvent;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int RequestLogMessagesDelegate(MpvHandle* ctx,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string minLevel);
    public static RequestLogMessagesDelegate RequestLogMessages => s_requestLogMessages ??= GetFunction<RequestLogMessagesDelegate>("mpv_request_log_messages");
    private static RequestLogMessagesDelegate? s_requestLogMessages;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int SetOptionDelegate(MpvHandle* ctx,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string name, MpvFormat format, void* data);
    public static SetOptionDelegate SetOption => s_setOption ??= GetFunction<SetOptionDelegate>("mpv_set_option");
    private static SetOptionDelegate? s_setOption;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int SetOptionStringDelegate(MpvHandle* ctx,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string name,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string data);
    public static SetOptionStringDelegate SetOptionString => s_setOptionString ??= GetFunction<SetOptionStringDelegate>("mpv_set_option_string");
    private static SetOptionStringDelegate? s_setOptionString;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int SetPropertyDelegate(MpvHandle* ctx,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string name, MpvFormat format, void* data);
    public static SetPropertyDelegate SetProperty => s_setProperty ??= GetFunction<SetPropertyDelegate>("mpv_set_property");
    private static SetPropertyDelegate? s_setProperty;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int SetPropertyAsyncDelegate(MpvHandle* ctx, ulong replyUserData,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string name, MpvFormat format, void* data);
    public static SetPropertyAsyncDelegate SetPropertyAsync => s_setPropertyAsync ??= GetFunction<SetPropertyAsyncDelegate>("mpv_set_property_async");
    private static SetPropertyAsyncDelegate? s_setPropertyAsync;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int SetPropertyStringDelegate(MpvHandle* ctx,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string name,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string data);
    public static SetPropertyStringDelegate SetPropertyString => s_setPropertyString ??= GetFunction<SetPropertyStringDelegate>("mpv_set_property_string");
    private static SetPropertyStringDelegate? s_setPropertyString;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SetWakeupCallbackDelegate(MpvHandle* ctx, MpvSetWakeupCallbackCbFunc cb, void* d);
    public static SetWakeupCallbackDelegate SetWakeupCallback => s_setWakeupCallback ??= GetFunction<SetWakeupCallbackDelegate>("mpv_set_wakeup_callback");
    private static SetWakeupCallbackDelegate? s_setWakeupCallback;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int StreamCbAddRoDelegate(MpvHandle* ctx,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string protocol, void* userData, MpvStreamCbAddRoOpenFnFunc openFn);
    public static StreamCbAddRoDelegate StreamCbAddRo => s_streamCbAddRo ??= GetFunction<StreamCbAddRoDelegate>("mpv_stream_cb_add_ro");
    private static StreamCbAddRoDelegate? s_streamCbAddRo;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void TerminateDestroyDelegate(MpvHandle* ctx);
    public static TerminateDestroyDelegate TerminateDestroy => s_terminateDestroy ??= GetFunction<TerminateDestroyDelegate>("mpv_terminate_destroy");
    private static TerminateDestroyDelegate? s_terminateDestroy;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int UnobservePropertyDelegate(MpvHandle* mpv, ulong registeredReplyUserData);
    public static UnobservePropertyDelegate UnobserveProperty => s_unobserveProperty ??= GetFunction<UnobservePropertyDelegate>("mpv_unobserve_property");
    private static UnobservePropertyDelegate? s_unobserveProperty;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WaitAsyncRequestsDelegate(MpvHandle* ctx);
    public static WaitAsyncRequestsDelegate WaitAsyncRequests => s_waitAsyncRequests ??= GetFunction<WaitAsyncRequestsDelegate>("mpv_wait_async_requests");
    private static WaitAsyncRequestsDelegate? s_waitAsyncRequests;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate MpvEvent* WaitEventDelegate(MpvHandle* ctx, double timeout);
    public static WaitEventDelegate WaitEvent => s_waitEvent ??= GetFunction<WaitEventDelegate>("mpv_wait_event");
    private static WaitEventDelegate? s_waitEvent;
    
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WakeupDelegate(MpvHandle* ctx);
    public static WakeupDelegate Wakeup => s_wakeup ??= GetFunction<WakeupDelegate>("mpv_wakeup");
    private static WakeupDelegate? s_wakeup;
}
