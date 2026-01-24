# 7-Zip Cross-Platform Support

## Overview

The 7-Zip extractor has been fully refactored for cross-platform operation on Windows, Linux, and macOS:
- ? Uses .NET's `NativeLibrary` class for dynamic library loading
- ? Cross-platform COM interop without Windows dependencies
- ? Manual PROPVARIANT handling instead of ole32.dll
- ? No Windows-specific P/Invoke calls

## Key Improvements

### 1. **No Windows-Specific Dependencies**
   - ? **Removed**: `ole32.dll!PropVariantClear` (Windows-only)
   - ? **Removed**: `Marshal.GetObjectForNativeVariant` (Windows-only)
   - ? **Removed**: `kernel32.dll` P/Invoke (Windows-only)
   - ? **Added**: Manual PROPVARIANT marshaling (cross-platform)
   - ? **Added**: Cross-platform memory management (`Marshal.FreeCoTaskMem`, `Marshal.FreeBSTR`)
   - ? **Added**: `NativeLibrary` API (.NET 5+)

### 2. **Cross-Platform COM Interop**
   - Uses .NET's built-in COM marshaling (available since .NET Core 3.0)
   - `[ComImport]` and `[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]` work on all platforms
   - Vtable calling conventions handled by .NET runtime
   - No Windows COM runtime required on Linux/macOS
   - 7-Zip's native libraries (7z.so, 7z.dylib) expose COM-compatible vtables

## Changes Made

### 1. **Kernel32Dll.cs ? NativeLibraryLoader.cs**
   - **Before**: Windows-only P/Invoke to `kernel32.dll` (`LoadLibrary`, `GetProcAddress`, `FreeLibrary`)
   - **After**: Cross-platform `NativeLibrary` class from .NET
   - **Benefits**:
     - Works on Linux and macOS
     - Uses standard .NET APIs
     - Better error handling and library search paths

### 2. **SafeLibraryHandle.cs**
   - **Before**: Inherited from `SafeHandleZeroOrMinusOneIsInvalid` (Windows-specific)
   - **After**: Implements `IDisposable` with cross-platform `IntPtr` handle
   - **Benefits**:
     - Simpler implementation
     - Works across all platforms
     - Properly disposes native library handles

### 3. **SevenZipInterface.cs - PropVariant**
   - **Before**: 
     - Used `ole32.dll!PropVariantClear` for cleanup
     - Used `Marshal.GetObjectForNativeVariant` for conversion
   - **After**: 
     - Manual memory cleanup with `Marshal.FreeCoTaskMem`/`Marshal.FreeBSTR`
     - Manual type conversion for all PROPVARIANT types
   - **Benefits**:
     - No Windows DLL dependencies
     - Full control over memory management
     - Supports all common PROPVARIANT types:
       - Simple types: INT, UINT, BOOL, etc.
       - Strings: BSTR, LPSTR, LPWSTR
       - DateTime: FILETIME
       - Floating point: R4, R8

### 4. **SevenZipHandle.cs**
   - **Before**: Used `Kernel32Dll.LoadLibrary` and `Kernel32Dll.GetProcAddress`
   - **After**: Uses `NativeLibraryLoader.TryLoadLibrary` and `NativeLibraryLoader.GetExport`
   - **Benefits**:
     - Better error messages
     - Cross-platform compatibility
     - Consistent with other native libraries (LibMpv)

### 5. **ArchiveFile.cs**
   - **Before**: Only searched for Windows DLLs (`7zxa.dll`, `7z.dll`)
   - **After**: Platform-aware library search:
     - **Windows**: `7zxa.dll`, `7z.dll`
     - **Linux**: `7z.so`, `lib7z.so` (bundled or system)
     - **macOS**: `7z.dylib`, `lib7z.dylib` (bundled or system)
   - **Benefits**:
     - Automatically finds system libraries
     - Falls back to bundled libraries
     - Clear error messages

### 6. **SevenZipInitializer.cs**
   - **Before**: Returned early for non-Windows with "only Windows support for now"
   - **After**: Updated comment to clarify Linux/macOS use system libraries
   - **Benefits**:
     - Clear expected behavior per platform
     - Windows bundles libraries; Linux/macOS use system packages

## Library Search Strategy

### Windows
1. `{SevenZipFolder}/7zxa.dll`
2. `{SevenZipFolder}/7z.dll`
3. Current directory
4. System PATH

### Linux
1. `{SevenZipFolder}/7z.so`
2. `{SevenZipFolder}/lib7z.so`
3. System resolution (`7z.so`, `lib7z.so`)
4. Standard paths: `/usr/local/lib`, `/usr/lib`, `/lib`, `/usr/lib/p7zip`

### macOS
1. `{SevenZipFolder}/7z.dylib`
2. `{SevenZipFolder}/lib7z.dylib`
3. System resolution (`7z.dylib`, `lib7z.dylib`)
4. Standard paths: `/opt/local/lib`, `/usr/local/lib`, `/opt/homebrew/lib`

## Installation Requirements

### Windows
- Libraries are bundled with the application (`7zxa.dll`)
- No additional installation required

### Linux
Install `p7zip` or `p7zip-full` package:
```bash
# Debian/Ubuntu
sudo apt install p7zip-full

# Fedora/RHEL
sudo dnf install p7zip p7zip-plugins

# Arch Linux
sudo pacman -S p7zip
```

### macOS
Install via Homebrew or MacPorts:
```bash
# Homebrew
brew install p7zip

# MacPorts
sudo port install p7zip
```

## Interface Version

Updated to 7-Zip interface version **25.1** with the following enhancements:

### AskMode Enum
- Added `kReadExternal` mode for external file reading

### OperationResult Enum
- Added error codes:
  - `kUnavailable` - File unavailable
  - `kUnexpectedEnd` - Unexpected end of data
  - `kDataAfterEnd` - Data after end of archive
  - `kIsNotArc` - Not an archive file
  - `kHeadersError` - Archive headers error
  - `kWrongPassword` - Wrong password provided
- Fixed `kUnSupportedMethod` ? `kUnsupportedMethod` for consistency

## Technical Details

### COM Interop on Non-Windows Platforms

.NET Core 3.0+ and .NET 5+ provide COM interop support on all platforms:

```csharp
[ComImport]
[Guid("23170F69-40C1-278A-0000-000600600000")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IInArchive
{
    // Methods are marshaled via vtable
}
```

**How it works:**
1. .NET generates vtable marshaling stubs
2. Native 7-Zip libraries expose COM-compatible vtables
3. No Windows COM runtime required
4. Standard calling conventions (stdcall) handled by runtime

### PROPVARIANT Handling

Manual marshaling for all variant types:

```csharp
public object GetObject()
{
    switch (this.VarType)
    {
        case VarEnum.VT_BSTR:
            return Marshal.PtrToStringBSTR(this.pointerValue);
        case VarEnum.VT_I4:
            return (int)this.longValue;
        case VarEnum.VT_FILETIME:
            return DateTime.FromFileTime(this.longValue);
        // ... etc
    }
}
```

**Memory Management:**
- `VT_BSTR` ? `Marshal.FreeBSTR()`
- `VT_LPSTR`/`VT_LPWSTR` ? `Marshal.FreeCoTaskMem()`
- Simple types ? Just clear variant type

## Architecture

The implementation follows the same pattern as `LibMpvDynamicPlayer`:

1. **Platform Detection**: `RuntimeInformation.IsOSPlatform()`
2. **Dynamic Loading**: `NativeLibrary.TryLoad()` and `NativeLibrary.GetExport()`
3. **Multiple Search Paths**: Tries bundled, then system libraries
4. **Graceful Fallback**: Clear error messages when libraries not found
5. **No Windows Dependencies**: Pure .NET marshaling APIs

## Testing

To test cross-platform support:

1. **Windows**: Works with bundled `7zxa.dll`
2. **Linux**: Install `p7zip-full` and test archive extraction
3. **macOS**: Install `p7zip` via Homebrew and test extraction

Example test:
```csharp
using SevenZipExtractor;

// Works on all platforms
using (var archive = new ArchiveFile("test.zip"))
{
    foreach (var entry in archive.Entries)
    {
        Console.WriteLine($"{entry.FileName} - {entry.Size} bytes");
    }
    
    archive.Extract("output_folder");
}
```

## Compatibility

- ? .NET 9 (primary target)
- ? .NET 8
- ? .NET 7
- ? .NET 6
- ? .NET 5
- ?? .NET Core 3.0+ (COM interop available)
- ? .NET Framework (Windows-only, but works with old code)
- ? .NET Standard 2.1 (no `NativeLibrary` class)

## Future Improvements

- Bundle native libraries for Linux/macOS in application package
- Add runtime library version detection
- Support for additional archive formats per platform
- Automated tests for cross-platform library loading
- Performance benchmarks across platforms

## References

- [7-Zip Source Code](https://github.com/mcmilk/7-Zip)
- [7-Zip Interface 25.1](https://github.com/mcmilk/7-Zip/blob/master/CPP/7zip/Archive/IArchive.h)
- [.NET NativeLibrary Class](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.nativelibrary)
- [.NET COM Interop](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/cominterop)
- [Cross-Platform COM in .NET Core](https://devblogs.microsoft.com/dotnet/improvements-in-native-code-interop-in-net-5-0/)
