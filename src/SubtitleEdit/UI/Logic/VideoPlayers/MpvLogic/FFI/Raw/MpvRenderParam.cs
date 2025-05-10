using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.MpvLogic.FFI.Raw;

[StructLayout(LayoutKind.Sequential)]
public struct MpvRenderParam
{
       public MpvRenderParamType type;
       public nint data;
}