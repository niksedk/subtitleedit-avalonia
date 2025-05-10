namespace HanumanInstitute.LibMpv;

/// <summary>
/// Represents a comma-delimited option list of key-value pairs. ex: Val1=a,Val2=b
/// </summary>
public class MpvOptionDictionary : MpvOptionRef<IDictionary<string, string>>
{
    private readonly char _separator;

    public MpvOptionDictionary(MpvContext mpv, string name, bool isPath = false) : base(mpv, name)
    {
        _separator = isPath ? Path.PathSeparator : ',';
    }

    private static string FormatKeyValue(string key, string value) => EscapeValue(key) + "=" + EscapeValue(value);

    private string FormatKeyValueList(IDictionary<string, string> values) => string.Join(_separator.ToString(), values.Select(x => FormatKeyValue(x.Key, x.Value)));

    private static string EscapeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }
        else
        {
            var length = Encoding.UTF8.GetByteCount(value);
            return "%{0}%{1}".FormatInvariant(length, value);
        }
    }
    
    /// <summary>
    /// Sets a dictionary of key/value pairs.
    /// </summary>
    public override void Set(IDictionary<string, string> values) => Mpv.ChangeList(PropertyName, ListOptionOperation.Set, FormatKeyValueList(values)).Invoke();

    /// <summary>
    /// Sets a dictionary of key/value pairs.
    /// </summary>
    public override Task SetAsync(IDictionary<string, string> values, MpvAsyncOptions? options = null) => Mpv.ChangeList(PropertyName, ListOptionOperation.Set, FormatKeyValueList(values)).InvokeAsync(options.ToCommandOptions());

    /// <summary>
    /// Gets the value of specified key.
    /// </summary>
    public string? Get(string key)
    {
        var values = Get();
        if (values != null && values.TryGetValue(key, out var result))
        {
            return result;
        }
        return null;
    }
    
    /// <summary>
    /// Gets the value of specified key.
    /// </summary>
    public async Task<string?> GetAsync(string key, MpvAsyncOptions? options = null)
    {
        var values = await GetAsync(options);
        if (values != null && values.TryGetValue(key, out var result))
        {
            return result;
        }
        return null;
    }

    // /// <summary>
    // /// Gets a dictionary containing all values.
    // /// </summary>
    //public override async Task<IDictionary<string, string>?> GetAsync(ApiCommandOptions? options = null)
    //{
    //    var values = await Api.GetPropertyAsync(PropertyName, options);
    //    return ParseValue(values.Data) ?? new Dictionary<string, string>();
    //}

    /// <summary>
    /// Adds a key/value pair to the list.
    /// </summary>
    public void Add(string key, string value, MpvAsyncOptions? options = null) =>
        Mpv.ChangeList(PropertyName, ListOptionOperation.Add, 
            FormatKeyValue(key.CheckNotNullOrEmpty(nameof(key)), value)).Invoke(options.ToCommandOptions());
    
    /// <summary>
    /// Adds a key/value pair to the list.
    /// </summary>
    public Task AddAsync(string key, string value, MpvAsyncOptions? options = null) =>
        Mpv.ChangeList(PropertyName, ListOptionOperation.Add, 
            FormatKeyValue(key.CheckNotNullOrEmpty(nameof(key)), value)).InvokeAsync(options.ToCommandOptions());

    /// <summary>
    /// Adds a dictionary of key/value pairs to the list.
    /// </summary>
    public override Task AddAsync(IDictionary<string, string> values, MpvAsyncOptions? options = null) => Mpv.ChangeList(PropertyName, ListOptionOperation.Add, FormatKeyValueList(values)).InvokeAsync(options.ToCommandOptions());

    /// <summary>
    /// Delete item if present (does not interpret escapes).
    /// </summary>
    public void Remove(string key, MpvAsyncOptions? options = null) =>
        Mpv.ChangeList(PropertyName, ListOptionOperation.Remove, 
            key.CheckNotNullOrEmpty(nameof(key))).Invoke(options.ToCommandOptions());

    /// <summary>
    /// Delete item if present (does not interpret escapes).
    /// </summary>
    public Task RemoveAsync(string key, MpvAsyncOptions? options = null) =>
        Mpv.ChangeList(PropertyName, ListOptionOperation.Remove, 
            key.CheckNotNullOrEmpty(nameof(key))).InvokeAsync(options.ToCommandOptions());
}
