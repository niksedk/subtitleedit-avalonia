namespace HanumanInstitute.LibMpv;

public class MpvOptionWithDefault<T> : MpvOptionWith<T>
    where T : struct
{
    public MpvOptionWithDefault(MpvContext mpv, string name) :
        base(mpv, name)
    {
    }

    /// <summary>
    /// Sets the option to 'default'.
    /// </summary>
    public void SetDefault() => SetValue("default");

    /// <summary>
    /// Sets the option to 'default'.
    /// </summary>
    public Task SetDefaultAsync(MpvAsyncOptions? options = null) => SetValueAsync("default", options);

    /// <summary>
    /// Gets whether the option is 'default'.
    /// </summary>
    public bool GetDefault() => GetValue("default");

    /// <summary>
    /// Gets whether the option is 'default'.
    /// </summary>
    public Task<bool> GetDefaultAsync(MpvAsyncOptions? options = null) => GetValueAsync("default", options);
}
