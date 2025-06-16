namespace HanumanInstitute.LibMpv.Core;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate void* MpvOpenglInitParamsGetProcAddress (void* ctx,     
    #if NETSTANDARD2_1_OR_GREATER
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    #else
    [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]
    #endif
    string name);
public struct MpvOpenglInitParamsGetProcAddressFunc
{
    public IntPtr Pointer;
    public static implicit operator MpvOpenglInitParamsGetProcAddressFunc(MpvOpenglInitParamsGetProcAddress? func) => new MpvOpenglInitParamsGetProcAddressFunc { Pointer = func == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(func) };
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate void MpvRenderContextSetUpdateCallbackCallback (void* cbCtx);
public struct MpvRenderContextSetUpdateCallbackCallbackFunc
{
    public IntPtr Pointer;
    public static implicit operator MpvRenderContextSetUpdateCallbackCallbackFunc(MpvRenderContextSetUpdateCallbackCallback? func) => new MpvRenderContextSetUpdateCallbackCallbackFunc { Pointer = func == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(func) };
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate void MpvSetWakeupCallbackCb (void* d);
public struct MpvSetWakeupCallbackCbFunc
{
    public IntPtr Pointer;
    public static implicit operator MpvSetWakeupCallbackCbFunc(MpvSetWakeupCallbackCb? func) => new MpvSetWakeupCallbackCbFunc { Pointer = func == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(func) };
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate int MpvStreamCbAddRoOpenFn (void* userData, byte* uri, MpvStreamCbInfo* info);
public struct MpvStreamCbAddRoOpenFnFunc
{
    public IntPtr Pointer;
    public static implicit operator MpvStreamCbAddRoOpenFnFunc(MpvStreamCbAddRoOpenFn? func) => new MpvStreamCbAddRoOpenFnFunc { Pointer = func == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(func) };
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate void MpvStreamCbInfoCancelFn (void* cookie);
public struct MpvStreamCbInfoCancelFnFunc
{
    public IntPtr Pointer;
    public static implicit operator MpvStreamCbInfoCancelFnFunc(MpvStreamCbInfoCancelFn? func) => new MpvStreamCbInfoCancelFnFunc { Pointer = func == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(func) };
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate void MpvStreamCbInfoCloseFn (void* cookie);
public struct MpvStreamCbInfoCloseFnFunc
{
    public IntPtr Pointer;
    public static implicit operator MpvStreamCbInfoCloseFnFunc(MpvStreamCbInfoCloseFn? func) => new MpvStreamCbInfoCloseFnFunc { Pointer = func == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(func) };
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate long MpvStreamCbInfoReadFn (void* cookie, byte* buf, ulong nBytes);
public struct MpvStreamCbInfoReadFnFunc
{
    public IntPtr Pointer;
    public static implicit operator MpvStreamCbInfoReadFnFunc(MpvStreamCbInfoReadFn? func) => new MpvStreamCbInfoReadFnFunc { Pointer = func == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(func) };
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate long MpvStreamCbInfoSeekFn (void* cookie, long offset);
public struct MpvStreamCbInfoSeekFnFunc
{
    public IntPtr Pointer;
    public static implicit operator MpvStreamCbInfoSeekFnFunc(MpvStreamCbInfoSeekFn? func) => new MpvStreamCbInfoSeekFnFunc { Pointer = func == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(func) };
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate long MpvStreamCbInfoSizeFn (void* cookie);
public struct MpvStreamCbInfoSizeFnFunc
{
    public IntPtr Pointer;
    public static implicit operator MpvStreamCbInfoSizeFnFunc(MpvStreamCbInfoSizeFn? func) => new MpvStreamCbInfoSizeFnFunc { Pointer = func == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(func) };
}

