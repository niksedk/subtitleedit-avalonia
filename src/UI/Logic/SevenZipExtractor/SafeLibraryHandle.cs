using System;
using System.Runtime.InteropServices;

namespace SevenZipExtractor
{
    /// <summary>
    /// Safe handle for native library loaded via NativeLibrary
    /// </summary>
    internal sealed class SafeLibraryHandle : IDisposable
    {
        private IntPtr _handle;
        private bool _disposed;

        public SafeLibraryHandle(IntPtr handle)
        {
            _handle = handle;
        }

        public bool IsInvalid => _handle == IntPtr.Zero;

        public bool IsClosed => _handle == IntPtr.Zero;

        public IntPtr DangerousGetHandle() => _handle;

        /// <summary>Release library handle</summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_handle != IntPtr.Zero)
            {
                NativeLibraryLoader.FreeLibrary(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }

        public void Close()
        {
            Dispose();
        }
    }
}