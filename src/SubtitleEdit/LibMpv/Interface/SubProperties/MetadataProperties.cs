namespace HanumanInstitute.LibMpv;

/// <summary>
/// Exposes the Metadata sub-properties.
/// </summary>
public class MetadataProperties
{
    private readonly MpvContext _mpv;
    private readonly string _prefix;

    public MetadataProperties(MpvContext mpv, string propertyName)
    {
        _mpv = mpv;
        _prefix = propertyName;
    }

    /// <summary>
    /// Metadata key/value pairs.
    /// </summary>
    public MpvPropertyReadRef<IDictionary<string, string>> Metadata => new(_mpv, _prefix);

    /// <summary>
    /// Value of metadata entry 'key'.
    /// </summary>
    public MpvPropertyIndexReadRef<string, string> MetadataByKey => new(_mpv, _prefix + "/by-key/{0}");

    /// <summary>
    /// Number of metadata entries.
    /// </summary>
    public MpvPropertyRead<int> MetadataListCount => new(_mpv, _prefix + "/list/count");

    /// <summary>
    /// Key name of the Nth metadata entry. (The first entry is 0).
    /// </summary>
    public MpvPropertyIndexReadRef<int, string> MetadataListKey => new(_mpv, _prefix + "/list/{0}/key");

    /// <summary>
    /// Value of the Nth metadata entry.
    /// </summary>
    public MpvPropertyIndexReadRef<int, string> MetadataListValue => new(_mpv, _prefix + "/list/{0}/value");
}
