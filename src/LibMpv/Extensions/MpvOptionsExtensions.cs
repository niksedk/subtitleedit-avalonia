namespace HanumanInstitute.LibMpv;

public static class MpvOptionsExtensions
{
    public static MpvCommandOptions? ToCommandOptions(this MpvAsyncOptions? options) => options == null ?
        null :
        new MpvCommandOptions()
        {
            ResponseTimeout = options.ResponseTimeout,
            WaitForResponse = options.WaitForResponse,
            ThrowOnError = options.ThrowOnError
        };
}
