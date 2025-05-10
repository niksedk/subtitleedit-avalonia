using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.MpvLogic.FFI.Raw;


public delegate nint procAddress(nint ctx, string name);

[StructLayout(LayoutKind.Sequential)]
public struct MpvOpenglInitParams
{
    /**
         * This retrieves OpenGL function pointers, and will use them in subsequent
         * operation.
         * Usually, you can simply call the GL context APIs from this callback (e.g.
         * glXGetProcAddressARB or wglGetProcAddress), but some APIs do not always
         * return pointers for all standard functions (even if present); in this
         * case you have to compensate by looking up these functions yourself when
         * libmpv wants to resolve them through this callback.
         * libmpv will not normally attempt to resolve GL functions on its own, nor
         * does it link to GL libraries directly.
         */
    public procAddress get_pro_address;
    /**
     * Value passed as ctx parameter to get_proc_address().
     */
    nint get_proc_address_ctx;
}