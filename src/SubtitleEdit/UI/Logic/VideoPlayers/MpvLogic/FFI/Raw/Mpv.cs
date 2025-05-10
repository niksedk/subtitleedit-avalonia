using System.Runtime.InteropServices;
using MpvHandle = nint;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.MpvLogic.FFI.Raw;

internal static class WinFunctions
{
    // https://docs.microsoft.com/en-us/windows/desktop/api/libloaderapi/nf-libloaderapi-loadlibrarya
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern MpvHandle LoadLibrary(string lpFileName);

    // https://docs.microsoft.com/en-us/windows/desktop/api/libloaderapi/nf-libloaderapi-freelibrary
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern int FreeLibrary(MpvHandle hModule);

    // https://docs.microsoft.com/en-us/windows/desktop/api/libloaderapi/nf-libloaderapi-getprocaddress
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern MpvHandle GetProcAddress(MpvHandle hModule, string lProcName);
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void CommonCallBack(MpvHandle data);

public static class Mpv
{
    //macos libmpv install by homebrew
    // private const string MpvDllPath = "/usr/local/Cellar/mpv/0.35.1/lib/libmpv.dylib";
    private const string MpvDllPath = "libmpv-2.dll";
    static Mpv()
    {
    }

    [DllImport(MpvDllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern MpvHandle mpv_create();

    [DllImport(MpvDllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern int mpv_initialize(MpvHandle ctx);

    [DllImport(MpvDllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern int mpv_set_option_string(MpvHandle ctx, string name, string data);

    [DllImport(MpvDllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void mpv_set_wakeup_callback(MpvHandle ctx, CommonCallBack cb, MpvHandle data);

    [DllImport(MpvDllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern MpvHandle mpv_wait_event(MpvHandle ctx, double timeout);

    [DllImport(MpvDllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern int mpv_render_context_create(out MpvHandle ctx, MpvHandle mpv, MpvHandle paras);


    [DllImport(MpvDllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void mpv_render_context_set_update_callback(MpvHandle rednPtrctx,
        CommonCallBack callback,
        MpvHandle data);

    [DllImport(MpvDllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern int mpv_render_context_render(MpvHandle ctx, MpvHandle paras);

    [DllImport(MpvDllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern int mpv_command_node(MpvHandle mpv, MpvHandle args, out MpvNode res);

}


public delegate void MpvRendeContextSetUpdateCallback(MpvHandle ctx, CommonCallBack cb, MpvHandle data);

public class DynamicMpv
{
    public static MpvRendeContextSetUpdateCallback RendeContextSetUpdateCallback { set; get; }

    static DynamicMpv()
    {
        var handle = WinFunctions.LoadLibrary("C:\\Users\\zhou\\Documents\\Code\\CSharp\\AvaloniaApplication2\\AvaloniaApplication2\\bin\\Debug\\net6.0\\libmpv-2.dll");
        var ptr = WinFunctions.GetProcAddress(handle, "mpv_render_context_set_update_callback");
        RendeContextSetUpdateCallback = (MpvRendeContextSetUpdateCallback)Marshal.GetDelegateForFunctionPointer(ptr, typeof(MpvRendeContextSetUpdateCallback));
    }
}

