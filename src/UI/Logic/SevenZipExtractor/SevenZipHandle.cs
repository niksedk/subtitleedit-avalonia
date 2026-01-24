using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SevenZipExtractor
{
    internal class SevenZipHandle : IDisposable
    {
        private SafeLibraryHandle? sevenZipSafeHandle;

        public SevenZipHandle(string sevenZipLibPath)
        {
            this.sevenZipSafeHandle = Kernel32Dll.LoadLibrary(sevenZipLibPath);

            if (this.sevenZipSafeHandle.IsInvalid)
            {
                throw new Win32Exception();
            }

            IntPtr functionPtr = Kernel32Dll.GetProcAddress(this.sevenZipSafeHandle, "GetHandlerProperty");
            
            // Not valid dll
            if (functionPtr == IntPtr.Zero)
            {
                this.sevenZipSafeHandle.Close();
                throw new ArgumentException();
            }
        }

        ~SevenZipHandle()
        {
            this.Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if ((this.sevenZipSafeHandle != null) && !this.sevenZipSafeHandle.IsClosed)
            {
                this.sevenZipSafeHandle.Close();
            }

            sevenZipSafeHandle = null;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IInArchive? CreateInArchive(Guid classId)
        {
            if (sevenZipSafeHandle == null)
            {
                throw new ObjectDisposedException("SevenZipHandle");
            }

            IntPtr procAddress = Kernel32Dll.GetProcAddress(sevenZipSafeHandle, "CreateObject");
            CreateObjectDelegate? createObject = Marshal.GetDelegateForFunctionPointer(procAddress, typeof(CreateObjectDelegate)) as CreateObjectDelegate;

            object? result = null;
            Guid interfaceId = typeof(IInArchive).GUID;
            createObject?.Invoke(ref classId, ref interfaceId, out result);

            return result as IInArchive;
        }
    }
}