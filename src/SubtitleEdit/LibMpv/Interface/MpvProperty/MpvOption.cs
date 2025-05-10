namespace HanumanInstitute.LibMpv;

public class MpvOption<T> : MpvPropertyWrite<T, T>
    where T : struct
{
    public MpvOption(MpvContext mpv, string name) : base(mpv, name)
    {
    }
}

public class MpvOption<T, TRaw> : MpvPropertyWrite<T, TRaw>
    where T : struct
{
    public MpvOption(MpvContext mpv, string name) : base(mpv, name)
    {
    }
}

public class MpvOptionRef<T> : MpvPropertyWriteRef<T, T>
    where T : class
{
    public MpvOptionRef(MpvContext mpv, string name) : base(mpv, name)
    {
    }
}

public class MpvOptionRef<T, TRaw> : MpvPropertyWriteRef<T, TRaw>
    where T : class
{
    public MpvOptionRef(MpvContext mpv, string name) : base(mpv, name)
    {
    }
}

public class MpvOptionString : MpvOptionRef<string, string>
{
    public MpvOptionString(MpvContext mpv, string name) : base(mpv, name)
    {
    }
}
