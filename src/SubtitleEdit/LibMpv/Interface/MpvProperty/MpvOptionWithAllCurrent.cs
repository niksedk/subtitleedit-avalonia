namespace HanumanInstitute.LibMpv;

public class MpvOptionWithAllCurrent<T> : MpvOptionWith<T>
    where T : struct
{
    public MpvOptionWithAllCurrent(MpvContext mpv, string name) :
        base(mpv, name)
    {
    }

    /// <summary>
    /// Sets the option to 'all'.
    /// </summary>
    public void SetAll() => SetValue("all");

    /// <summary>
    /// Sets the option to 'all'.
    /// </summary>
    public Task SetAllAsync(MpvAsyncOptions? options = null) => SetValueAsync("all", options);

    /// <summary>
    /// Gets whether the option is 'all'.
    /// </summary>
    public bool GetAll() => GetValue("all");

    /// <summary>
    /// Gets whether the option is 'all'.
    /// </summary>
    public Task<bool> GetAllAsync(MpvAsyncOptions? options = null) => GetValueAsync("all", options);

    /// <summary>
    /// Sets the option to 'current'.
    /// </summary>
    public void SetCurrent() => SetValue("current");

    /// <summary>
    /// Sets the option to 'current'.
    /// </summary>
    public Task SetCurrentAsync(MpvAsyncOptions? options = null) => SetValueAsync("current", options);

    /// <summary>
    /// Gets whether the option is 'current'.
    /// </summary>
    public bool GetCurrent() => GetValue("current");

    /// <summary>
    /// Gets whether the option is 'current'.
    /// </summary>
    public Task<bool> GetCurrentAsync(MpvAsyncOptions? options = null) => GetValueAsync("current", options);
}
