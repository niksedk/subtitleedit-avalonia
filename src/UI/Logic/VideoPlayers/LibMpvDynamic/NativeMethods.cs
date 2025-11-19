using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

internal static class NativeMethods
{
    private static IntPtr _libdlHandle;
    private static IntPtr _libcHandle;

    // Delegate types for dynamically loaded POSIX functions
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SetlocaleDelegate(int category, string locale);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr DlopenDelegate(string filename, int flags);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int DlcloseDelegate(IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr DlsymDelegate(IntPtr handle, string symbol);

    private static readonly SetlocaleDelegate? _setlocale;
    private static readonly DlopenDelegate? _dlopen;
    private static readonly DlcloseDelegate? _dlclose;
    private static readonly DlsymDelegate? _dlsym;

    static NativeMethods()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Try to load libc with various names
            var libcNames = new[] { "libc.so.6", "libc.so", "libc" };
            foreach (var name in libcNames)
            {
                if (NativeLibrary.TryLoad(name, out _libcHandle))
                {
                    break;
                }
            }

            // Try to load libdl with various names (dl functions might be in libc on modern systems)
            var libdlNames = new[] { "libdl.so.2", "libdl.so", "libdl" };
            foreach (var name in libdlNames)
            {
                if (NativeLibrary.TryLoad(name, out _libdlHandle))
                {
                    break;
                }
            }

            // If libdl didn't load, try using libc handle (many modern systems have dl* functions in libc)
            if (_libdlHandle == IntPtr.Zero)
            {
                _libdlHandle = _libcHandle;
            }

            // Load function pointers
            if (_libcHandle != IntPtr.Zero)
            {
                if (NativeLibrary.TryGetExport(_libcHandle, "setlocale", out var setlocalePtr))
                {
                    _setlocale = Marshal.GetDelegateForFunctionPointer<SetlocaleDelegate>(setlocalePtr);
                }
            }

            if (_libdlHandle != IntPtr.Zero)
            {
                if (NativeLibrary.TryGetExport(_libdlHandle, "dlopen", out var dlopenPtr))
                {
                    _dlopen = Marshal.GetDelegateForFunctionPointer<DlopenDelegate>(dlopenPtr);
                }
                if (NativeLibrary.TryGetExport(_libdlHandle, "dlclose", out var dlclosePtr))
                {
                    _dlclose = Marshal.GetDelegateForFunctionPointer<DlcloseDelegate>(dlclosePtr);
                }
                if (NativeLibrary.TryGetExport(_libdlHandle, "dlsym", out var dlsymPtr))
                {
                    _dlsym = Marshal.GetDelegateForFunctionPointer<DlsymDelegate>(dlsymPtr);
                }
            }
        }
    }

    // Windows
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    internal static extern IntPtr LoadLibrary(string dllToLoad);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool FreeLibrary(IntPtr hModule);

    // POSIX constants
    internal const int LC_NUMERIC = 1;
    internal const int RTLD_NOW = 0x0002;
    internal const int RTLD_GLOBAL = 0x0100;

    // POSIX function wrappers
    internal static IntPtr setlocale(int category, string locale)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("setlocale is not supported on Windows");
        }
        return _setlocale?.Invoke(category, locale) ?? IntPtr.Zero;
    }

    internal static IntPtr dlopen(string filename, int flags)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("dlopen is not supported on Windows");
        }
        return _dlopen?.Invoke(filename, flags) ?? IntPtr.Zero;
    }

    internal static IntPtr dlclose(IntPtr handle)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("dlclose is not supported on Windows");
        }
        return _dlclose?.Invoke(handle) ?? IntPtr.Zero;
    }

    internal static IntPtr dlsym(IntPtr handle, string symbol)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("dlsym is not supported on Windows");
        }
        return _dlsym?.Invoke(handle, symbol) ?? IntPtr.Zero;
    }

    // Cross-platform wrappers
    internal static IntPtr CrossLoadLibrary(string fileName)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return LoadLibrary(fileName);
        }
        else
        {
            return dlopen(fileName, RTLD_NOW | RTLD_GLOBAL);
        }
    }

    internal static void CrossFreeLibrary(IntPtr handle)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            FreeLibrary(handle);
        }
        else
        {
            dlclose(handle);
        }
    }

    internal static IntPtr CrossGetProcAddress(IntPtr handle, string name)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetProcAddress(handle, name);
        }
        else
        {
            return dlsym(handle, name);
        }
    }

    internal static object? GetDllType(IntPtr handle, Type type, string name)
    {
        var address = CrossGetProcAddress(handle, name);
        return address != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer(address, type) : null;
    }
}

