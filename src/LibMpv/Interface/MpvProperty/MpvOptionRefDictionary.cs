namespace HanumanInstitute.LibMpv;

/// <summary>
/// This variant of MpvOptionDictionary doesn't use ChangeList and gets and sets values as a direct property. Values are not escaped.
/// </summary>
public class MpvOptionRefDictionary : MpvOptionRef<IDictionary<string, string>>
{
    private readonly char _separator;

    public MpvOptionRefDictionary(MpvContext mpv, string name, bool isPath = false) : base(mpv, name)
    {
        _separator = isPath ? Path.PathSeparator : ',';
    }

    private static string FormatKeyValue(string key, string value) => key + "=" + value;

    private string FormatKeyValueList(IDictionary<string, string> values) => string.Join(_separator.ToString(), values.Select(x => FormatKeyValue(x.Key, x.Value)));

    /// <summary>
    /// Sets a dictionary of key/value pairs.
    /// </summary>
    public override void Set(IDictionary<string, string> values) =>
        Mpv.SetProperty(PropertyName, FormatKeyValueList(values));

    /// <summary>
    /// Sets a dictionary of key/value pairs.
    /// </summary>
    public override Task SetAsync(IDictionary<string, string> values, MpvAsyncOptions? options = null) =>
        Mpv.SetPropertyAsync(PropertyName, FormatKeyValueList(values), options);
}
