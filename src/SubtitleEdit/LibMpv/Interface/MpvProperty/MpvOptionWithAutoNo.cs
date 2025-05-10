namespace HanumanInstitute.LibMpv;

public class MpvOptionWithAutoNo<T> : MpvOptionWithAuto<T>
    where T : struct
{
    public MpvOptionWithAutoNo(MpvContext mpv, string name) :
        base(mpv, name)
    {
    }

    /// <summary>
    /// Sets the option to 'no'.
    /// </summary>
    public void SetNo() => SetValue("no");

    /// <summary>
    /// Sets the option to 'no'.
    /// </summary>
    public Task SetNoAsync(MpvAsyncOptions? options = null) => SetValueAsync("no", options);

    /// <summary>
    /// Gets whether the option is 'no'.
    /// </summary>
    public bool GetNo() => GetValue(new[] { "no", "false" });

    /// <summary>
    /// Gets whether the option is 'no'.
    /// </summary>
    public Task<bool> GetNoAsync(MpvAsyncOptions? options = null) => GetValueAsync(new[] { "no", "false" }, options);
}
