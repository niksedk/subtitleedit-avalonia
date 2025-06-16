namespace HanumanInstitute.LibMpv;

/// <summary>
/// Represents a key in the script-opts option dictionary.
/// </summary>
public class MpvScriptOption
{
    private readonly MpvOptionDictionary _options;
    private readonly string _key;

    public MpvScriptOption(MpvContext mpv, string key)
    {
        mpv.CheckNotNull(nameof(mpv));
        _options = new MpvOptionDictionary(mpv, "script-opts");
        _key = key.CheckNotNull(nameof(key));
    }

    /// <summary>
    /// Gets the value of the script option.
    /// </summary>
    public string? Get() => _options.Get(_key);

    /// <summary>
    /// Gets the value of the script option.
    /// </summary>
    public Task<string?> GetAsync(MpvAsyncOptions? options = null) => _options.GetAsync(_key, options);

    /// <summary>
    /// Sets the value of the script option.
    /// </summary>
    public void SetAsync(string value)
    {
        if (value.HasValue())
        {
            _options.Add(_key, value);
        }
        else
        {
            Remove();
        }
    }
    
    /// <summary>
    /// Sets the value of the script option.
    /// </summary>
    public async Task SetAsync(string value, MpvAsyncOptions? options = null)
    {
        if (value.HasValue())
        {
            await _options.AddAsync(_key, value, options);
        }
        else
        {
            await RemoveAsync(options);
        }
    }

    /// <summary>
    /// Removes the value from script options.
    /// </summary>
    public void Remove() => _options.RemoveAsync(_key);

    /// <summary>
    /// Removes the value from script options.
    /// </summary>
    public Task RemoveAsync(MpvAsyncOptions? options = null) => _options.RemoveAsync(_key, options);
}
