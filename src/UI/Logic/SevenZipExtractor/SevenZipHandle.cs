using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SevenZipExtractor
{
    internal class SevenZipHandle : IDisposable
    {
        private SafeLibraryHandle sevenZipSafeHandle;

        public SevenZipHandle(string sevenZipLibPath)
        {
            IntPtr libraryHandle;
            
            // Try to load the library
            if (!NativeLibraryLoader.TryLoadLibrary(sevenZipLibPath, out libraryHandle))
            {
                throw new SevenZipException($"Cannot load 7-Zip library from: {sevenZipLibPath}");
            }

            this.sevenZipSafeHandle = new SafeLibraryHandle(libraryHandle);

            if (this.sevenZipSafeHandle.IsInvalid)
            {
                throw new SevenZipException("Failed to load 7-Zip library");
            }

            IntPtr functionPtr = NativeLibraryLoader.GetExport(libraryHandle, "GetHandlerProperty");
            
            // Not valid dll
            if (functionPtr == IntPtr.Zero)
            {
                this.sevenZipSafeHandle.Close();
                throw new SevenZipException("Invalid 7-Zip library - missing GetHandlerProperty export");
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

            this.sevenZipSafeHandle = null;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IInArchive CreateInArchive(Guid classId)
        {
            if (this.sevenZipSafeHandle == null)
            {
                throw new ObjectDisposedException("SevenZipHandle");
            }

            IntPtr procAddress = NativeLibraryLoader.GetExport(this.sevenZipSafeHandle.DangerousGetHandle(), "CreateObject");
            
            if (procAddress == IntPtr.Zero)
            {
                throw new SevenZipException("Cannot find CreateObject function in 7-Zip library");
            }

            CreateObjectDelegate createObject = (CreateObjectDelegate) Marshal.GetDelegateForFunctionPointer(procAddress, typeof (CreateObjectDelegate));

            object result;
            Guid interfaceId = typeof (IInArchive).GUID;
            createObject(ref classId, ref interfaceId, out result);

            return result as IInArchive;
        }
    }
}