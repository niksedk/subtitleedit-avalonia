namespace HanumanInstitute.LibMpv.Core;

/// <summary>Since API version 1.9.</summary>
public enum MpvEndFileReason
{
    /// <summary>The end of file was reached. Sometimes this may also happen on incomplete or corrupted files, or if the network connection was interrupted when playing a remote file. It also happens if the playback range was restricted with --end or --frames or similar.</summary>
    EndOfFile = 0,
    /// <summary>Playback was stopped by an external action (e.g. playlist controls).</summary>
    Stop = 2,
    /// <summary>Playback was stopped by the quit command or player shutdown.</summary>
    Quit = 3,
    /// <summary>Some kind of error happened that lead to playback abort. Does not necessarily happen on incomplete or broken files (in these cases, both MPV_END_FILE_REASON_ERROR or MPV_END_FILE_REASON_EOF are possible).</summary>
    Error = 4,
    /// <summary>The file was a playlist or similar. When the playlist is read, its entries will be appended to the playlist after the entry of the current file, the entry of the current file is removed, and a MPV_EVENT_END_FILE event is sent with reason set to MPV_END_FILE_REASON_REDIRECT. Then playback continues with the playlist contents. Since API version 1.18.</summary>
    Redirect = 5,
}

/// <summary>List of error codes than can be returned by API functions. 0 and positive return values always mean success, negative values are always errors.</summary>
public enum MpvError
{
    /// <summary>No error happened (used to signal successful operation). Keep in mind that many API functions returning error codes can also return positive values, which also indicate success. API users can hardcode the fact that ">= 0" means success.</summary>
    Success = 0,
    /// <summary>The event ring buffer is full. This means the client is choked, and can't receive any events. This can happen when too many asynchronous requests have been made, but not answered. Probably never happens in practice, unless the mpv core is frozen for some reason, and the client keeps making asynchronous requests. (Bugs in the client API implementation could also trigger this, e.g. if events become "lost".)</summary>
    EventQueueFull = -1,
    /// <summary>Memory allocation failed.</summary>
    NoMemory = -2,
    /// <summary>The mpv core wasn't configured and initialized yet. See the notes in mpv_create().</summary>
    Uninitialized = -3,
    /// <summary>Generic catch-all error if a parameter is set to an invalid or unsupported value. This is used if there is no better error code.</summary>
    InvalidParameter = -4,
    /// <summary>Trying to set an option that doesn't exist.</summary>
    OptionNotFound = -5,
    /// <summary>Trying to set an option using an unsupported MPV_FORMAT.</summary>
    OptionFormat = -6,
    /// <summary>Setting the option failed. Typically this happens if the provided option value could not be parsed.</summary>
    OptionError = -7,
    /// <summary>The accessed property doesn't exist.</summary>
    PropertyNotFound = -8,
    /// <summary>Trying to set or get a property using an unsupported MPV_FORMAT.</summary>
    PropertyFormat = -9,
    /// <summary>The property exists, but is not available. This usually happens when the associated subsystem is not active, e.g. querying audio parameters while audio is disabled.</summary>
    PropertyUnavailable = -10,
    /// <summary>Error setting or getting a property.</summary>
    PropertyError = -11,
    /// <summary>General error when running a command with mpv_command and similar.</summary>
    Command = -12,
    /// <summary>Generic error on loading (usually used with mpv_event_end_file.error).</summary>
    LoadingFailed = -13,
    /// <summary>Initializing the audio output failed.</summary>
    AudioOutputInitFailed = -14,
    /// <summary>Initializing the video output failed.</summary>
    VideoOutputInitFailed = -15,
    /// <summary>There was no audio or video data to play. This also happens if the file was recognized, but did not contain any audio or video streams, or no streams were selected.</summary>
    NothingToDisplay = -16,
    /// <summary>When trying to load the file, the file format could not be determined, or the file was too broken to open it.</summary>
    UnknownFormat = -17,
    /// <summary>Generic error for signaling that certain system requirements are not fulfilled.</summary>
    Unsupported = -18,
    /// <summary>The API function which was called is a stub only.</summary>
    NotImplemented = -19,
    /// <summary>Unspecified error.</summary>
    Generic = -20,
}

public enum MpvEventId
{
    /// <summary>Nothing happened. Happens on timeouts or sporadic wake-ups.</summary>
    None = 0,
    /// <summary>Happens when the player quits. The player enters a state where it tries to disconnect all clients. Most requests to the player will fail, and the client should react to this and quit with mpv_destroy() as soon as possible.</summary>
    Shutdown = 1,
    /// <summary>See mpv_request_log_messages().</summary>
    LogMessage = 2,
    /// <summary>Reply to a mpv_get_property_async() request. See also mpv_event and mpv_event_property.</summary>
    GetPropertyReply = 3,
    /// <summary>Reply to a mpv_set_property_async() request. (Unlike MPV_EVENT_GET_PROPERTY, mpv_event_property is not used.)</summary>
    SetPropertyReply = 4,
    /// <summary>Reply to a mpv_command_async() or mpv_command_node_async() request. See also mpv_event and mpv_event_command.</summary>
    CommandReply = 5,
    /// <summary>Notification before playback start of a file (before the file is loaded). See also mpv_event and mpv_event_start_file.</summary>
    StartFile = 6,
    /// <summary>Notification after playback end (after the file was unloaded). See also mpv_event and mpv_event_end_file.</summary>
    EndFile = 7,
    /// <summary>Notification when the file has been loaded (headers were read etc.), and decoding starts.</summary>
    FileLoaded = 8,
    /// <summary>Idle mode was entered. In this mode, no file is played, and the playback core waits for new commands. (The command line player normally quits instead of entering idle mode, unless --idle was specified. If mpv was started with mpv_create(), idle mode is enabled by default.)</summary>
    EventIdle = 11,
    /// <summary>Sent every time after a video frame is displayed. Note that currently, this will be sent in lower frequency if there is no video, or playback is paused - but that will be removed in the future, and it will be restricted to video frames only.</summary>
    EventTick = 14,
    /// <summary>Triggered by the script-message input command. The command uses the first argument of the command as client name (see mpv_client_name()) to dispatch the message, and passes along all arguments starting from the second argument as strings. See also mpv_event and mpv_event_client_message.</summary>
    EventClientMessage = 16,
    /// <summary>Happens after video changed in some way. This can happen on resolution changes, pixel format changes, or video filter changes. The event is sent after the video filters and the VO are reconfigured. Applications embedding a mpv window should listen to this event in order to resize the window if needed. Note that this event can happen sporadically, and you should check yourself whether the video parameters really changed before doing something expensive.</summary>
    EventVideoReconfig = 17,
    /// <summary>Similar to MPV_EVENT_VIDEO_RECONFIG. This is relatively uninteresting, because there is no such thing as audio output embedding.</summary>
    EventAudioReconfig = 18,
    /// <summary>Happens when a seek was initiated. Playback stops. Usually it will resume with MPV_EVENT_PLAYBACK_RESTART as soon as the seek is finished.</summary>
    Seek = 20,
    /// <summary>There was a discontinuity of some sort (like a seek), and playback was reinitialized. Usually happens on start of playback and after seeking. The main purpose is allowing the client to detect when a seek request is finished.</summary>
    PlaybackRestart = 21,
    /// <summary>Event sent due to mpv_observe_property(). See also mpv_event and mpv_event_property.</summary>
    PropertyChange = 22,
    /// <summary>Happens if the internal per-mpv_handle ring buffer overflows, and at least 1 event had to be dropped. This can happen if the client doesn't read the event queue quickly enough with mpv_wait_event(), or if the client makes a very large number of asynchronous calls at once.</summary>
    QueueOverflow = 24,
    /// <summary>Triggered if a hook handler was registered with mpv_hook_add(), and the hook is invoked. If you receive this, you must handle it, and continue the hook with mpv_hook_continue(). See also mpv_event and mpv_event_hook.</summary>
    Hook = 25,
}

/// <summary>Data format for options and properties. The API functions to get/set properties and options support multiple formats, and this enum describes them.</summary>
public enum MpvFormat
{
    /// <summary>Invalid. Sometimes used for empty values. This is always defined to 0, so a normal 0-init of mpv_format (or e.g. mpv_node) is guaranteed to set this it to MPV_FORMAT_NONE (which makes some things saner as consequence).</summary>
    None = 0,
    /// <summary>The basic type is char*. It returns the raw property string, like using ${=property} in input.conf (see input.rst).</summary>
    String = 1,
    /// <summary>The basic type is char*. It returns the OSD property string, like using ${property} in input.conf (see input.rst). In many cases, this is the same as the raw string, but in other cases it's formatted for display on OSD. It's intended to be human readable. Do not attempt to parse these strings.</summary>
    OsdString = 2,
    /// <summary>The basic type is int. The only allowed values are 0 ("no") and 1 ("yes").</summary>
    Flag = 3,
    /// <summary>The basic type is int64_t.</summary>
    Int64 = 4,
    /// <summary>The basic type is double.</summary>
    Double = 5,
    /// <summary>The type is mpv_node.</summary>
    Node = 6,
    /// <summary>Used with mpv_node only. Can usually not be used directly.</summary>
    NodeArray = 7,
    /// <summary>See MPV_FORMAT_NODE_ARRAY.</summary>
    NodeMap = 8,
    /// <summary>A raw, untyped byte array. Only used only with mpv_node, and only in some very specific situations. (Some commands use it.)</summary>
    ByteArray = 9,
}

/// <summary>Numeric log levels. The lower the number, the more important the message is. MPV_LOG_LEVEL_NONE is never used when receiving messages. The string in the comment after the value is the name of the log level as used for the mpv_request_log_messages() function. Unused numeric values are unused, but reserved for future use.</summary>
public enum MpvLogLevel
{
    None = 0,
    /// <summary>Disable absolutely all messages</summary>
    Fatal = 10,
    /// <summary>Critical/aborting errors</summary>
    Error = 20,
    /// <summary>Simple errors</summary>
    Warning = 30,
    /// <summary>Possible problems</summary>
    Information = 40,
    /// <summary>Informational message</summary>
    Verbose = 50,
    /// <summary>Noisy informational message</summary>
    Debug = 60,
    /// <summary>Very noisy technical information</summary>
    Trade = 70,
}

/// <summary>Flags used in mpv_render_frame_info.flags. Each value represents a bit in it.</summary>
public enum MpvRenderFrameInfoFlag
{
    /// <summary>Set if there is actually a next frame. If unset, there is no next frame yet, and other flags and fields that require a frame to be queued will be unset.</summary>
    Present = 1,
    /// <summary>If set, the frame is not an actual new video frame, but a redraw request. For example if the video is paused, and an option that affects video rendering was changed (or any other reason), an update request can be issued and this flag will be set.</summary>
    Redraw = 2,
    /// <summary>If set, this is supposed to reproduce the previous frame perfectly. This is usually used for certain "video-sync" options ("display-..." modes). Typically the renderer will blit the video from a FBO. Unset otherwise.</summary>
    Repeat = 4,
    /// <summary>If set, the player timing code expects that the user thread blocks on vsync (by either delaying the render call, or by making a call to mpv_render_context_report_swap() at vsync time).</summary>
    BlockVSync = 8,
}

/// <summary>Parameters for mpv_render_param (which is used in a few places such as mpv_render_context_create().</summary>
public enum MpvRenderParamType
{
    /// <summary>Not a valid value, but also used to terminate a params array. Its value is always guaranteed to be 0 (even if the ABI changes in the future).</summary>
    Invalid = 0,
    /// <summary>The render API to use. Valid for mpv_render_context_create().</summary>
    ApiType = 1,
    /// <summary>Required parameters for initializing the OpenGL renderer. Valid for mpv_render_context_create(). Type: mpv_opengl_init_params*</summary>
    OpenGlInitParams = 2,
    /// <summary>Describes a GL render target. Valid for mpv_render_context_render(). Type: mpv_opengl_fbo*</summary>
    OpenGlFbo = 3,
    /// <summary>Control flipped rendering. Valid for mpv_render_context_render(). Type: int* If the value is set to 0, render normally. Otherwise, render it flipped, which is needed e.g. when rendering to an OpenGL default framebuffer (which has a flipped coordinate system).</summary>
    FlipY = 4,
    /// <summary>Control surface depth. Valid for mpv_render_context_render(). Type: int* This implies the depth of the surface passed to the render function in bits per channel. If omitted or set to 0, the renderer will assume 8. Typically used to control dithering.</summary>
    Depth = 5,
    /// <summary>ICC profile blob. Valid for mpv_render_context_set_parameter(). Type: mpv_byte_array* Set an ICC profile for use with the "icc-profile-auto" option. (If the option is not enabled, the ICC data will not be used.)</summary>
    IccProfile = 6,
    /// <summary>Ambient light in lux. Valid for mpv_render_context_set_parameter(). Type: int* This can be used for automatic gamma correction.</summary>
    AmbientLight = 7,
    /// <summary>X11 Display, sometimes used for hwdec. Valid for mpv_render_context_create(). The Display must stay valid for the lifetime of the mpv_render_context. Type: Display*</summary>
    X11Display = 8,
    /// <summary>Wayland display, sometimes used for hwdec. Valid for mpv_render_context_create(). The wl_display must stay valid for the lifetime of the mpv_render_context. Type: struct wl_display*</summary>
    WaylandDisplay = 9,
    /// <summary>Better control about rendering and enabling some advanced features. Valid for mpv_render_context_create().</summary>
    AdvancedControl = 10,
    /// <summary>Return information about the next frame to render. Valid for mpv_render_context_get_info().</summary>
    NextFrameInfo = 11,
    /// <summary>Enable or disable video timing. Valid for mpv_render_context_render().</summary>
    BlockForTargetTime = 12,
    /// <summary>Use to skip rendering in mpv_render_context_render().</summary>
    SkipRendering = 13,
    /// <summary>Deprecated. Not supported. Use MPV_RENDER_PARAM_DRM_DISPLAY_V2 instead. Type : struct mpv_opengl_drm_params*</summary>
    DrmDisplay = 14,
    /// <summary>DRM draw surface size, contains draw surface dimensions. Valid for mpv_render_context_create(). Type : struct mpv_opengl_drm_draw_surface_size*</summary>
    DrmDrawSurfaceSize = 15,
    /// <summary>DRM display, contains drm display handles. Valid for mpv_render_context_create(). Type : struct mpv_opengl_drm_params_v2*</summary>
    DrmDisplayV2 = 16,
    /// <summary>MPV_RENDER_API_TYPE_SW only: rendering target surface size, mandatory. Valid for MPV_RENDER_API_TYPE_SW & mpv_render_context_render(). Type: int[2] (e.g.: int s[2] = {w, h}; param.data = s[0];)</summary>
    SwSize = 17,
    /// <summary>MPV_RENDER_API_TYPE_SW only: rendering target surface pixel format, mandatory. Valid for MPV_RENDER_API_TYPE_SW & mpv_render_context_render(). Type: char* (e.g.: char *f = "rgb0"; param.data = f;)</summary>
    SwFormat = 18,
    /// <summary>MPV_RENDER_API_TYPE_SW only: rendering target surface bytes per line, mandatory. Valid for MPV_RENDER_API_TYPE_SW & mpv_render_context_render(). Type: size_t*</summary>
    SwStride = 19,
    /// <summary>MPV_RENDER_API_TYPE_SW only: rendering target surface bytes per line, mandatory. Valid for MPV_RENDER_API_TYPE_SW & mpv_render_context_render(). Type: size_t*</summary>
    SwPointer = 20,
}

/// <summary>Flags returned by mpv_render_context_update(). Each value represents a bit in the function's return value.</summary>
public enum MpvRenderUpdateFlag
{
    /// <summary>A new video frame must be rendered. mpv_render_context_render() must be called.</summary>
    Frame = 1,
}

