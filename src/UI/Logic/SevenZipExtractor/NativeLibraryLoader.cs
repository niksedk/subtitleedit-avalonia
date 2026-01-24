using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SevenZipExtractor
{
    /// <summary>
    /// Cross-platform native library loader for 7-Zip libraries
    /// </summary>
    internal static class NativeLibraryLoader
    {
        /// <summary>
        /// Get the platform-specific library names for 7-Zip
        /// </summary>
        internal static string[] GetLibraryNames()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new[] { "7z.dll", "7zxa.dll" };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new[] { "7z.so", "lib7z.so", "7zr.so" };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new[] { "7z.dylib", "lib7z.dylib" };
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported OS platform for 7-Zip.");
            }
        }

        /// <summary>
        /// Get the platform-specific search paths for 7-Zip libraries
        /// </summary>
        internal static string[] GetLibrarySearchPaths()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new[]
                {
                    Se.SevenZipFolder,
                    Directory.GetCurrentDirectory(),
                    string.Empty, // Let system search PATH
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new[]
                {
                    Directory.GetCurrentDirectory(),
                    "/usr/local/lib",
                    "/usr/lib",
                    "/lib",
                    "/usr/lib/x86_64-linux-gnu",
                    "/lib/x86_64-linux-gnu",
                    "/usr/lib/aarch64-linux-gnu",
                    "/lib/aarch64-linux-gnu",
                    "/usr/lib/p7zip",
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new[]
                {
                    Directory.GetCurrentDirectory(),
                    "/Applications/Subtitle Edit.app/Contents/Frameworks",
                    "/opt/local/lib",
                    "/usr/local/lib",
                    "/opt/homebrew/lib",
                    "/opt/lib",
                };
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported OS platform for 7-Zip.");
            }
        }

        /// <summary>
        /// Try to load a native library from multiple possible paths
        /// </summary>
        internal static bool TryLoadLibrary(string libraryPath, out IntPtr handle)
        {
            handle = IntPtr.Zero;

            if (string.IsNullOrEmpty(libraryPath))
            {
                return false;
            }

            // If absolute path provided, try loading it directly
            if (Path.IsPathRooted(libraryPath) && File.Exists(libraryPath))
            {
                return NativeLibrary.TryLoad(libraryPath, out handle);
            }

            // Try loading with just the name (let system resolve it)
            if (NativeLibrary.TryLoad(libraryPath, out handle))
            {
                return true;
            }

            // Try searching in standard paths
            foreach (var searchPath in GetLibrarySearchPaths())
            {
                if (string.IsNullOrEmpty(searchPath))
                {
                    continue;
                }

                var fullPath = Path.Combine(searchPath, libraryPath);
                if (File.Exists(fullPath) && NativeLibrary.TryLoad(fullPath, out handle))
                {
                    return true;
                }
            }

            // Try with default library names
            foreach (var libName in GetLibraryNames())
            {
                if (NativeLibrary.TryLoad(libName, out handle))
                {
                    return true;
                }

                foreach (var searchPath in GetLibrarySearchPaths())
                {
                    if (string.IsNullOrEmpty(searchPath))
                    {
                        continue;
                    }

                    var fullPath = Path.Combine(searchPath, libName);
                    if (File.Exists(fullPath) && NativeLibrary.TryLoad(fullPath, out handle))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Get a function pointer from the loaded library
        /// </summary>
        internal static IntPtr GetExport(IntPtr handle, string name)
        {
            if (handle == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            return NativeLibrary.TryGetExport(handle, name, out var address) ? address : IntPtr.Zero;
        }

        /// <summary>
        /// Free a loaded native library
        /// </summary>
        internal static void FreeLibrary(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                NativeLibrary.Free(handle);
            }
        }
    }
}