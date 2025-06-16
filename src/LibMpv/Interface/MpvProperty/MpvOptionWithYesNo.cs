namespace HanumanInstitute.LibMpv;

public class MpvOptionWithYesNo<T> : MpvOptionWithNo<T>
    where T : struct
{
    public MpvOptionWithYesNo(MpvContext mpv, string name) :
        base(mpv, name)
    {
    }

    /// <summary>
    /// Sets the option to 'yes'.
    /// </summary>
    public void SetYes() => SetValue("yes");

    /// <summary>
    /// Sets the option to 'yes'.
    /// </summary>
    public Task SetYesAsync(MpvAsyncOptions? options = null) => SetValueAsync("yes", options);

    /// <summary>
    /// Gets whether the option is 'yes'.
    /// </summary>
    public bool GetYes() => GetValue(new[] { "yes", "true" });

    /// <summary>
    /// Gets whether the option is 'yes'.
    /// </summary>
    public Task<bool> GetYesAsync(MpvAsyncOptions? options = null) => GetValueAsync(new[] { "yes", "true" }, options);
}
