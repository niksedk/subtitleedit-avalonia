using System;
using System.Runtime.InteropServices;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    internal static class NativeMethods
    {
        #region Win32 API

        // Win32 API functions for dynamically loading DLLs
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        internal static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AttachConsole(int dwProcessId);
        internal const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeConsole();

        [DllImport("user32.dll")]
        internal static extern short GetKeyState(int vKey);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        internal static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int width, int height, int wFlags);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        internal const int WM_SETREDRAW = 0x0b;

        [DllImport("dwmapi.dll")]
        internal static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        internal static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        [DllImport("kernel32.dll")]

        internal static extern uint SetThreadExecutionState(uint esFlags);

        internal const uint ES_CONTINUOUS = 0x80000000;

        internal const uint ES_SYSTEM_REQUIRED = 0x00000001;

        #endregion Win32 API
      
        #region Linux System

        internal const int LC_NUMERIC = 1;

        internal const int RTLD_NOW = 0x0001;
        internal const int RTLD_GLOBAL = 0x0100;

        [DllImport("libc.so.6")]
        internal static extern IntPtr setlocale(int category, string locale);

        [DllImport("libdl.so.2")]
        internal static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libdl.so.2")]
        internal static extern IntPtr dlclose(IntPtr handle);

        [DllImport("libdl.so.2")]
        internal static extern IntPtr dlsym(IntPtr handle, string symbol);

        #endregion

        #region Cross platform

        internal static IntPtr CrossLoadLibrary(string fileName)
        {
            if (Configuration.IsRunningOnWindows)
            {
                return LoadLibrary(fileName);
            }

            return dlopen(fileName, RTLD_NOW | RTLD_GLOBAL);
        }

        internal static void CrossFreeLibrary(IntPtr handle)
        {
            if (Configuration.IsRunningOnWindows)
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
            if (Configuration.IsRunningOnWindows)
            {
                return GetProcAddress(handle, name);
            }
            
            return dlsym(handle, name);
        }

        #endregion
    }
}
