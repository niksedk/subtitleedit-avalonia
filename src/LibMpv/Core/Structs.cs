// ReSharper disable UnassignedField.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.LibMpv.Core;

/// <summary>(see mpv_node)</summary>
public unsafe struct MpvByteArray
{
    /// <summary>Pointer to the data. In what format the data is stored is up to whatever uses MPV_FORMAT_BYTE_ARRAY.</summary>
    public void* Data;
    /// <summary>Size of the data pointed to by ptr.</summary>
    public ulong Size;
}

public unsafe struct MpvEvent
{
    /// <summary>One of mpv_event. Keep in mind that later ABI compatible releases might add new event types. These should be ignored by the API user.</summary>
    public MpvEventId EventId;
    /// <summary>This is mainly used for events that are replies to (asynchronous) requests. It contains a status code, which is >= 0 on success, or &lt; 0 on error (a mpv_error value). Usually, this will be set if an asynchronous request fails. Used for: MPV_EVENT_GET_PROPERTY_REPLY MPV_EVENT_SET_PROPERTY_REPLY MPV_EVENT_COMMAND_REPLY</summary>
    public int Error;
    /// <summary>If the event is in reply to a request (made with this API and this API handle), this is set to the replyUserData parameter of the request call. Otherwise, this field is 0. Used for: MPV_EVENT_GET_PROPERTY_REPLY MPV_EVENT_SET_PROPERTY_REPLY MPV_EVENT_COMMAND_REPLY MPV_EVENT_PROPERTY_CHANGE MPV_EVENT_HOOK</summary>
    public ulong ReplyUserData;
    /// <summary>The meaning and contents of the data member depend on the event_id: MPV_EVENT_GET_PROPERTY_REPLY: mpv_event_property* MPV_EVENT_PROPERTY_CHANGE: mpv_event_property* MPV_EVENT_LOG_MESSAGE: mpv_event_log_message* MPV_EVENT_CLIENT_MESSAGE: mpv_event_client_message* MPV_EVENT_START_FILE: mpv_event_start_file* (since v1.108) MPV_EVENT_END_FILE: mpv_event_end_file* MPV_EVENT_HOOK: mpv_event_hook* MPV_EVENT_COMMAND_REPLY* mpv_event_command* other: NULL</summary>
    public void* Data;
}

public unsafe struct MpvEventClientMessage
{
    /// <summary>Arbitrary arguments chosen by the sender of the message. If num_args > 0, you can access args[0] through args[num_args - 1] (inclusive). What these arguments mean is up to the sender and receiver. None of the valid items are NULL.</summary>
    public int NumArgs;
    public byte** Args;
}

public struct MpvEventCommand
{
    /// <summary>Result data of the command. Note that success/failure is signaled separately via mpv_event.error. This field is only for result data in case of success. Most commands leave it at MPV_FORMAT_NONE. Set to MPV_FORMAT_NONE on failure.</summary>
    public MpvNode Result;
}

public struct MpvEventEndFile
{
    /// <summary>Corresponds to the values in enum mpv_end_file_reason.</summary>
    public MpvEndFileReason Reason;
    /// <summary>If reason==MPV_END_FILE_REASON_ERROR, this contains a mpv error code (one of MPV_ERROR_...) giving an approximate reason why playback failed. In other cases, this field is 0 (no error). Since API version 1.9.</summary>
    public int Error;
    /// <summary>Playlist entry ID of the file that was being played or attempted to be played. This has the same value as the playlist_entry_id field in the corresponding mpv_event_start_file event. Since API version 1.108.</summary>
    public long PlaylistEntryId;
    /// <summary>If loading ended, because the playlist entry to be played was for example a playlist, and the current playlist entry is replaced with a number of other entries. This may happen at least with MPV_END_FILE_REASON_REDIRECT (other event types may use this for similar but different purposes in the future). In this case, playlist_insert_id will be set to the playlist entry ID of the first inserted entry, and playlist_insert_num_entries to the total number of inserted playlist entries. Note this in this specific case, the ID of the last inserted entry is playlist_insert_id+num-1. Beware that depending on circumstances, you may observe the new playlist entries before seeing the event (e.g. reading the "playlist" property or getting a property change notification before receiving the event). Since API version 1.108.</summary>
    public long PlaylistInsertId;
    /// <summary>See playlist_insert_id. Only non-0 if playlist_insert_id is valid. Never negative. Since API version 1.108.</summary>
    public int PlaylistInsertNumEntries;
}

public unsafe struct MpvEventHook
{
    /// <summary>The hook name as passed to mpv_hook_add().</summary>
    public byte* Name;
    /// <summary>Internal ID that must be passed to mpv_hook_continue().</summary>
    public ulong Id;
}

public unsafe struct MpvEventLogMessage
{
    /// <summary>The module prefix, identifies the sender of the message. As a special case, if the message buffer overflows, this will be set to the string "overflow" (which doesn't appear as prefix otherwise), and the text field will contain an informative message.</summary>
    public byte* Prefix;
    /// <summary>The log level as string. See mpv_request_log_messages() for possible values. The level "no" is never used here.</summary>
    public byte* Level;
    /// <summary>The log message. It consists of 1 line of text, and is terminated with a newline character. (Before API version 1.6, it could contain multiple or partial lines.)</summary>
    public byte* Text;
    /// <summary>The same contents as the level field, but as a numeric ID. Since API version 1.6.</summary>
    public MpvLogLevel LogLevel;
}

public unsafe struct MpvEventProperty
{
    /// <summary>Name of the property.</summary>
    public byte* Name;
    /// <summary>Format of the data field in the same struct. See enum mpv_format. This is always the same format as the requested format, except when the property could not be retrieved (unavailable, or an error happened), in which case the format is MPV_FORMAT_NONE.</summary>
    public MpvFormat Format;
    /// <summary>Received property value. Depends on the format. This is like the pointer argument passed to mpv_get_property().</summary>
    public void* Data;
}

/// <summary>Since API version 1.108.</summary>
public struct MpvEventStartFile
{
    /// <summary>Playlist entry ID of the file being loaded now.</summary>
    public long PlaylistEntryId;
}

/// <summary>Generic data storage.</summary>
public unsafe struct MpvNode
{
    public MpvNodeU U { get; set; }
    /// <summary>Type of the data stored in this struct. This value rules what members in the given union can be accessed. The following formats are currently defined to be allowed in mpv_node:</summary>
    public MpvFormat Format { get; set; }

    public override string ToString() => Format switch
    {
        MpvFormat.Double => U.Double.ToStringInvariant(),
        MpvFormat.Flag => U.Flag.ToStringInvariant(),
        MpvFormat.Int64 => U.Int64.ToStringInvariant(),
        MpvFormat.String => Utf8Marshaler.FromNative(U.String, Encoding.UTF8),
        MpvFormat.OsdString => Utf8Marshaler.FromNative(U.String, Encoding.UTF8),
        _ => Format.ToString()
    } ?? string.Empty;
}

/// <summary>(see mpv_node)</summary>
public unsafe struct MpvNodeList
{
    /// <summary>Number of entries. Negative values are not allowed.</summary>
    public int Num;
    /// <summary>MPV_FORMAT_NODE_ARRAY: values[N] refers to value of the Nth item</summary>
    public MpvNode* Values;
    /// <summary>MPV_FORMAT_NODE_ARRAY: unused (typically NULL), access is not allowed</summary>
    public byte** Keys;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct MpvNodeU
{
    [FieldOffset(0)]
    public byte* String;
    /// <summary>valid if format==MPV_FORMAT_STRING</summary>
    [FieldOffset(0)]
    public int Flag;
    /// <summary>valid if format==MPV_FORMAT_FLAG</summary>
    [FieldOffset(0)]
    public long Int64;
    /// <summary>valid if format==MPV_FORMAT_INT64</summary>
    [FieldOffset(0)]
    public double Double;
    /// <summary>valid if format==MPV_FORMAT_DOUBLE</summary>
    [FieldOffset(0)]
    public MpvNodeList* List;
    /// <summary>valid if format==MPV_FORMAT_BYTE_ARRAY</summary>
    [FieldOffset(0)]
    public MpvByteArray* ByteArray;
}

/// <summary>For MPV_RENDER_PARAM_DRM_DRAW_SURFACE_SIZE.</summary>
public struct MpvOpenglDrmDrawSurfaceSize
{
    /// <summary>size of the draw plane surface in pixels.</summary>
    public int Width;
    /// <summary>size of the draw plane surface in pixels.</summary>
    public int Height;
}

/// <summary>Deprecated. For MPV_RENDER_PARAM_DRM_DISPLAY.</summary>
public unsafe struct MpvOpenglDrmParams
{
    public int Fd;
    public int CrtcId;
    public int ConnectorId;
    public DrmModeAtomicReq** AtomicRequestPtr;
    public int RenderFd;
}

/// <summary>For MPV_RENDER_PARAM_DRM_DISPLAY_V2.</summary>
public unsafe struct MpvOpenglDrmParamsV2
{
    /// <summary>DRM fd (int). Set to -1 if invalid.</summary>
    public int Fd;
    /// <summary>Currently used crtc id</summary>
    public int CrtcId;
    /// <summary>Currently used connector id</summary>
    public int ConnectorId;
    /// <summary>Pointer to a drmModeAtomicReq pointer that is being used for the renderloop. This pointer should hold a pointer to the atomic request pointer The atomic request pointer is usually changed at every renderloop.</summary>
    public DrmModeAtomicReq** AtomicRequestPtr;
    /// <summary>DRM render node. Used for VAAPI interop. Set to -1 if invalid.</summary>
    public int RenderFd;
}

/// <summary>For MPV_RENDER_PARAM_OPENGL_FBO.</summary>
public struct MpvOpenglFbo
{
    /// <summary>Framebuffer object name. This must be either a valid FBO generated by glGenFramebuffers() that is complete and color-renderable, or 0. If the value is 0, this refers to the OpenGL default framebuffer.</summary>
    public int Fbo;
    /// <summary>Valid dimensions. This must refer to the size of the framebuffer. This must always be set.</summary>
    public int W;
    /// <summary>Valid dimensions. This must refer to the size of the framebuffer. This must always be set.</summary>
    public int H;
    /// <summary>Underlying texture internal format (e.g. GL_RGBA8), or 0 if unknown. If this is the default framebuffer, this can be an equivalent.</summary>
    public int InternalFormat;
}

/// <summary>For initializing the mpv OpenGL state via MPV_RENDER_PARAM_OPENGL_INIT_PARAMS.</summary>
public unsafe struct MpvOpenglInitParams
{
    /// <summary>This retrieves OpenGL function pointers, and will use them in subsequent operation. Usually, you can simply call the GL context APIs from this callback (e.g. glXGetProcAddressARB or wglGetProcAddress), but some APIs do not always return pointers for all standard functions (even if present); in this case you have to compensate by looking up these functions yourself when libmpv wants to resolve them through this callback. libmpv will not normally attempt to resolve GL functions on its own, nor does it link to GL libraries directly.</summary>
    public MpvOpenglInitParamsGetProcAddressFunc GetProcAddress;
    /// <summary>Value passed as ctx parameter to get_proc_address().</summary>
    public void* GetProcAddressCtx;
}

/// <summary>Information about the next video frame that will be rendered. Can be retrieved with MPV_RENDER_PARAM_NEXT_FRAME_INFO.</summary>
public struct MpvRenderFrameInfo
{
    /// <summary>A bitset of mpv_render_frame_info_flag values (i.e. multiple flags are combined with bitwise or).</summary>
    public ulong Flags;
    /// <summary>Absolute time at which the frame is supposed to be displayed. This is in the same unit and base as the time returned by mpv_get_time_us(). For frames that are redrawn, or if vsync locked video timing is used (see "video-sync" option), then this can be 0. The "video-timing-offset" option determines how much "headroom" the render thread gets (but a high enough frame rate can reduce it anyway). mpv_render_context_render() will normally block until the time is elapsed, unless you pass it MPV_RENDER_PARAM_BLOCK_FOR_TARGET_TIME = 0.</summary>
    public long TargetTime;
}

/// <summary>Used to pass arbitrary parameters to some mpv_render_* functions. The meaning of the data parameter is determined by the type, and each MPV_RENDER_PARAM_* documents what type the value must point to.</summary>
public unsafe struct MpvRenderParam
{
    public MpvRenderParamType Type;
    public void* Data;
}

/// <summary>See mpv_stream_cb_open_ro_fn callback.</summary>
public unsafe struct MpvStreamCbInfo
{
    /// <summary>Opaque user-provided value, which will be passed to the other callbacks. The close callback will be called to release the cookie. It is not interpreted by mpv. It doesn't even need to be a valid pointer.</summary>
    public void* Cookie;
    /// <summary>Callbacks set by the user in the mpv_stream_cb_open_ro_fn callback. Some of them are optional, and can be left unset.</summary>
    public MpvStreamCbInfoReadFnFunc ReadFn;
    public MpvStreamCbInfoSeekFnFunc SeekFn;
    public MpvStreamCbInfoSizeFnFunc SizeFn;
    public MpvStreamCbInfoCloseFnFunc CloseFn;
    public MpvStreamCbInfoCancelFnFunc CancelFn;
}

/// <remarks>This struct is incomplete.</remarks>
public struct DrmModeAtomicReq
{
}

/// <summary>Client context used by the client API. Every client has its own private handle.</summary>
/// <remarks>This struct is incomplete.</remarks>
public struct MpvHandle
{
}

/// <summary>Opaque context, returned by mpv_render_context_create().</summary>
/// <remarks>This struct is incomplete.</remarks>
public struct MpvRenderContext
{
}

