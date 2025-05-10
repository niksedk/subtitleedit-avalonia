namespace HanumanInstitute.LibMpv;

public class MpvOptionWithIndex<T> : MpvOptionWith<T>
    where T : struct
{
    public MpvOptionWithIndex(MpvContext mpv, string name) :
        base(mpv, name)
    {
    }

    /// <summary>
    /// Sets the option to 'index'.
    /// </summary>
    public void SetIndex() => SetValue("index");

    /// <summary>
    /// Sets the option to 'index'.
    /// </summary>
    public Task SetIndexAsync(MpvAsyncOptions? options = null) => SetValueAsync("index", options);

    /// <summary>
    /// Gets whether the option is 'index'.
    /// </summary>
    public bool GetIndex() => GetValue("index");

    /// <summary>
    /// Gets whether the option is 'index'.
    /// </summary>
    public Task<bool> GetIndexAsync(MpvAsyncOptions? options = null) => GetValueAsync("index", options);
}
