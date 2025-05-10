namespace HanumanInstitute.LibMpv;

public class MpvOptionWithInf<T> : MpvOptionWith<T>
    where T : struct
{
    public MpvOptionWithInf(MpvContext mpv, string name) :
        base(mpv, name)
    {
    }

    /// <summary>
    /// Sets the option to 'inf'.
    /// </summary>
    public void SetInf() => SetValue("inf");

    /// <summary>
    /// Sets the option to 'inf'.
    /// </summary>
    public Task SetInfAsync(MpvAsyncOptions? options = null) => SetValueAsync("inf", options);

    /// <summary>
    /// Gets whether the option is 'inf'.
    /// </summary>
    public bool GetInf() => GetValue(new[] { "inf", "false" });

    /// <summary>
    /// Gets whether the option is 'inf'.
    /// </summary>
    public Task<bool> GetInfAsync(MpvAsyncOptions? options = null) => GetValueAsync(new[] { "inf", "false" }, options);
}
