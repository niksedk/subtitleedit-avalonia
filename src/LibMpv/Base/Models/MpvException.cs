using HanumanInstitute.LibMpv.Core;

namespace HanumanInstitute.LibMpv;

public class MpvException : Exception
{
    public int Code { get; }

    public MpvException(string message, int code = -1) : base(message)
    {
        Code = code;
    }

    public static MpvException FromCode(int code)
    {
        return new MpvException(MpvApi.ErrorString(code), code);
    }
}
