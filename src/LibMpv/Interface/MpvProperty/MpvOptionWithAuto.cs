namespace HanumanInstitute.LibMpv;

public class MpvOptionWithAuto<T> : MpvOptionWith<T>
    where T : struct
{
    public MpvOptionWithAuto(MpvContext mpv, string name) :
        base(mpv, name)
    {
    }

    /// <summary>
    /// Sets the option to 'auto'.
    /// </summary>
    public void SetAuto() => SetValue("auto");

    /// <summary>
    /// Sets the option to 'auto'.
    /// </summary>
    public Task SetAutoAsync(MpvAsyncOptions? options = null) => SetValueAsync("auto", options);

    /// <summary>
    /// Gets whether the option is 'auto'.
    /// </summary>
    public bool GetAuto() => GetValue("auto");

    /// <summary>
    /// Gets whether the option is 'auto'.
    /// </summary>
    public Task<bool> GetAutoAsync(MpvAsyncOptions? options = null) => GetValueAsync("auto", options);
}
