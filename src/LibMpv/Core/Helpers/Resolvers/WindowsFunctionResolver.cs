namespace HanumanInstitute.LibMpv.Core;

public class WindowsFunctionResolver : FunctionResolverBase
{
    private const string Kernel32 = "kernel32";

    protected override string GetNativeLibraryName(string libraryName, int version) => $"{libraryName}-{version}.dll";
    protected override string[] GetSearchPaths() => new string[] { MpvApi.RootPath };
    protected override IntPtr LoadNativeLibrary(string libraryName) => LoadLibrary(libraryName);
    protected override IntPtr FindFunctionPointer(IntPtr nativeLibraryHandle, string functionName) => GetProcAddress(nativeLibraryHandle, functionName);

    [DllImport(Kernel32, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern IntPtr LoadLibrary(string dllToLoad);
}
