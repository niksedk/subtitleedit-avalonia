using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.MpvLogic.FFI.Raw;

[StructLayout(LayoutKind.Explicit)]
public struct MpvNodeData
{
    [FieldOffset(0)] public nint str;  /** valid if format==MPV_FORMAT_STRING */
    [FieldOffset(0)] public int flag;  /** valid if format==MPV_FORMAT_FLAG   */
    [FieldOffset(0)] public ulong intData;  /** valid if format==MPV_FORMAT_INT64  */
    [FieldOffset(0)] public double doubleData; /** valid if format==MPV_FORMAT_DOUBLE */
    [FieldOffset(0)] public nint nodeList; // valid if format==MPV_FORMAT_NODE_ARRAY or if format==MPV_FORMAT_NODE_MAP
    [FieldOffset(0)] public nint byteArray; // valid if format==MPV_FORMAT_BYTE_ARRAY
}