namespace HanumanInstitute.LibMpv.Core;

public static class FunctionResolverFactory
{
    public static IFunctionResolver Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new MacFunctionResolver();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsFunctionResolver();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var isAndroid = RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID"));
            return isAndroid ? new AndroidFunctionResolver() : new LinuxFunctionResolver();
        }

        throw new PlatformNotSupportedException();
    }
}