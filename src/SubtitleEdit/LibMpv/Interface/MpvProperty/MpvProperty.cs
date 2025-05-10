using System.Globalization;
using System.Text.Json;
using HanumanInstitute.LibMpv.Core;

namespace HanumanInstitute.LibMpv;

/// <summary>
/// Represents a read-only MPV property.
/// </summary>
/// <typeparam name="TNull">The nullable return type of the property.</typeparam>
/// <typeparam name="TRaw">The raw data type to be parsed from MPV. Usually the same.</typeparam>
public abstract class MpvProperty<TNull, TRaw>
{
    protected MpvContext Mpv { get; private set; }
    protected MpvFormat Format { get; set; }

    public MpvProperty(MpvContext mpv, string name)
    {
        Mpv = mpv;
        PropertyName = name.CheckNotNullOrEmpty(nameof(name));
        var type = typeof(TNull);
        Format = MpvFormatter.GetMpvFormat<TRaw>();
    }

    /// <summary>
    /// Gets the API name of the property.
    /// </summary>
    public string PropertyName { get; private set; }

    /// <summary>
    /// Parse value as specified type.
    /// </summary>
    /// <param name="value">The raw value to parse.</param>
    /// <returns>The typed parsed value.</returns>
    /// <exception cref="FormatException">Value is not in a valid format.</exception>
    /// <exception cref="OverflowException">Value represents a number that is out of the range.</exception>
    protected virtual TNull ParseValue(TRaw value)
    {
        var type = Nullable.GetUnderlyingType(typeof(TNull)) ?? typeof(TNull);
        return (TNull)(1 switch
        {
            _ when type == typeof(TRaw) => (object?)value!,
            _ when type == typeof(string) => value.ToStringInvariant()!,
            _ when value is string v => 1 switch
            {
                _ when string.IsNullOrEmpty(v) => default,
                _ when type == typeof(int) => v.Parse<int>(),
                _ when type == typeof(long) => v.Parse<long>(),
                _ when type == typeof(bool) => v.Parse<bool>(),
                _ when type == typeof(double) => v.Parse<double>(),
                _ when type == typeof(float) => v.Parse<float>(),
                _ => throw new FormatException("Custom parsing must be implemented.")
            },
            _ => throw new FormatException("Custom parsing must be implemented.")
        })!;
    }

    // /// <summary>
    // /// Formats specified value to send into a MPV request.
    // /// </summary>
    // /// <param name="value">The value to format.</param>
    // /// <returns>The formatted value.</returns>
    // protected virtual TRaw? FormatValue(TNull value) => value?.ToStringInvariant();
}
