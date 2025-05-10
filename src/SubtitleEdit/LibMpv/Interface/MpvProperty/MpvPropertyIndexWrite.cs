namespace HanumanInstitute.LibMpv;

/// <summary>
/// Represents a read/write MPV indexed property.
/// </summary>
/// <typeparam name="TIndex">The indexer data type.</typeparam>
/// <typeparam name="T">The return type of the property.</typeparam>
public class MpvPropertyIndexWrite<TIndex, T> : MpvPropertyIndexWrite<TIndex, T, T>
    where T : struct
{
    public MpvPropertyIndexWrite(MpvContext mpv, string name) : base(mpv, name)
    {
    }
}

/// <summary>
/// Represents a read/write MPV indexed property.
/// </summary>
/// <typeparam name="TIndex">The indexer data type.</typeparam>
/// <typeparam name="T">The return type of the property.</typeparam>
/// <typeparam name="TRaw">The raw data type to be parsed from MPV. Usually the same.</typeparam>
public class MpvPropertyIndexWrite<TIndex, T, TRaw> : MpvPropertyIndexRead<TIndex, T, TRaw>
    where T : struct
{
    public MpvPropertyIndexWrite(MpvContext mpv, string name) : base(mpv, name)
    {
    }

    /// <summary>
    /// Gives access to specified indexed property.
    /// </summary>
    /// <param name="index">The property index to access.</param>
    /// <returns>A property.</returns>
    public new MpvPropertyWrite<T, TRaw> this[TIndex index] => new(Mpv, GetPropertyIndexName(index));
}

/// <summary>
/// Represents a read/write MPV indexed property.
/// </summary>
/// <typeparam name="TIndex">The indexer data type.</typeparam>
/// <typeparam name="T">The return type of the property.</typeparam>
public class MpvPropertyIndexWriteRef<TIndex, T> : MpvPropertyIndexWriteRef<TIndex, T, T>
    where T : class
{
    public MpvPropertyIndexWriteRef(MpvContext mpv, string name) : base(mpv, name)
    {
    }
}

/// <summary>
/// Represents a read/write MPV indexed property.
/// </summary>
/// <typeparam name="TIndex">The indexer data type.</typeparam>
/// <typeparam name="T">The return type of the property.</typeparam>
/// <typeparam name="TRaw">The raw data type to be parsed from MPV. Usually the same.</typeparam>
public class MpvPropertyIndexWriteRef<TIndex, T, TRaw> : MpvPropertyIndexReadRef<TIndex, T, TRaw>
    where T : class
{
    public MpvPropertyIndexWriteRef(MpvContext mpv, string name) : base(mpv, name)
    {
    }

    /// <summary>
    /// Gives access to specified indexed property.
    /// </summary>
    /// <param name="index">The property index to access.</param>
    /// <returns>A property.</returns>
    public new MpvPropertyWriteRef<T, TRaw> this[TIndex index] => new(Mpv, GetPropertyIndexName(index));
}
