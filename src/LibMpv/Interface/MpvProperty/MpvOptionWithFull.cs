namespace HanumanInstitute.LibMpv;

public class MpvOptionWithFull<T> : MpvOptionWith<T>
    where T : struct
{
    public MpvOptionWithFull(MpvContext mpv, string name) :
        base(mpv, name)
    {
    }

    /// <summary>
    /// Sets the option to 'full'.
    /// </summary>
    public void SetFull() => SetValue("full");

    /// <summary>
    /// Sets the option to 'full'.
    /// </summary>
    public Task SetFullAsync(MpvAsyncOptions? options = null) => SetValueAsync("full", options);

    /// <summary>
    /// Gets whether the option is 'full'.
    /// </summary>
    public bool GetFull() => GetValue("full");

    /// <summary>
    /// Gets whether the option is 'full'.
    /// </summary>
    public Task<bool> GetFullAsync(MpvAsyncOptions? options = null) => GetValueAsync("full", options);
}
