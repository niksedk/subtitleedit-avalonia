using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.MpvLogic.FFI.Raw;

[StructLayout(LayoutKind.Sequential)]
public struct MpvEvent
{
        /**
         * One of mpv_event. Keep in mind that later ABI compatible releases might
         * add new event types. These should be ignored by the API user.
         */
        public MpvEventId event_id;
        /**
         * This is mainly used for events that are replies to (asynchronous)
         * requests. It contains a status code, which is >= 0 on success, or < 0
         * on error (a mpv_error value). Usually, this will be set if an
         * asynchronous request fails.
         * Used for:
         *  MPV_EVENT_GET_PROPERTY_REPLY
         *  MPV_EVENT_SET_PROPERTY_REPLY
         *  MPV_EVENT_COMMAND_REPLY
         */
        int error;
        /**
         * If the event is in reply to a request (made with this API and this
         * API handle), this is set to the reply_userdata parameter of the request
         * call. Otherwise, this field is 0.
         * Used for:
         *  MPV_EVENT_GET_PROPERTY_REPLY
         *  MPV_EVENT_SET_PROPERTY_REPLY
         *  MPV_EVENT_COMMAND_REPLY
         *  MPV_EVENT_PROPERTY_CHANGE
         *  MPV_EVENT_HOOK
         */
        ulong reply_userdata;
    /**
     * The meaning and contents of the data member depend on the event_id:
     *  MPV_EVENT_GET_PROPERTY_REPLY:     mpv_event_property*
     *  MPV_EVENT_PROPERTY_CHANGE:        mpv_event_property*
     *  MPV_EVENT_LOG_MESSAGE:            mpv_event_log_message*
     *  MPV_EVENT_CLIENT_MESSAGE:         mpv_event_client_message*
     *  MPV_EVENT_START_FILE:             mpv_event_start_file* (since v1.108)
     *  MPV_EVENT_END_FILE:               mpv_event_end_file*
     *  MPV_EVENT_HOOK:                   mpv_event_hook*
     *  MPV_EVENT_COMMAND_REPLY*          mpv_event_command*
     *  other: NULL
     *
     * Note: future enhancements might add new event structs for existing or new
     *       event types.
     */
    nint data;
}