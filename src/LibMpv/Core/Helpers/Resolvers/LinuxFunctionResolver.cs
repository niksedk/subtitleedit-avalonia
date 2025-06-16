// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
namespace HanumanInstitute.LibMpv.Core;

public class LinuxFunctionResolver : FunctionResolverBase
{
    private const string Libdl = "libdl.so.2";
    private const int RTLD_NOW = 0x002;

    protected override string GetNativeLibraryName(string libraryName, int version) => $"{libraryName}.so.{version}";
    protected override string[] GetSearchPaths() => new[] { MpvApi.RootPath, "/lib64", "/usr/lib64", "/lib", "/usr/lib" };
    protected override IntPtr LoadNativeLibrary(string libraryName) => dlopen(libraryName, RTLD_NOW);
    protected override IntPtr FindFunctionPointer(IntPtr nativeLibraryHandle, string functionName) => dlsym(nativeLibraryHandle, functionName);

    [DllImport(Libdl)]
    public static extern IntPtr dlsym(IntPtr handle, string symbol);

    [DllImport(Libdl)]
    public static extern IntPtr dlopen(string fileName, int flag);
}
