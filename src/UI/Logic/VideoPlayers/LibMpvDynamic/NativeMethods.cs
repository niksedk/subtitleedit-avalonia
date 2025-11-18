using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

internal static class NativeMethods
{
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

    // Linux / macOS (use same names)
    [DllImport("libc")]
    internal static extern IntPtr setlocale(int category, string locale);

    [DllImport("libdl")]
    internal static extern IntPtr dlopen(string filename, int flags);

    [DllImport("libdl")]
    internal static extern IntPtr dlclose(IntPtr handle);

    [DllImport("libdl")]
    internal static extern IntPtr dlsym(IntPtr handle, string symbol);

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

