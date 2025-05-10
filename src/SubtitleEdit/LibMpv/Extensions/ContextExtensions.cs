using System.Runtime.CompilerServices;
using HanumanInstitute.LibMpv.Core;

namespace HanumanInstitute.LibMpv;

internal static unsafe class ContextExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CheckCode(this int code)
    {
        if (code >= 0)
        {
            return code;
        }
        throw MpvException.FromCode(code);
    }
}
