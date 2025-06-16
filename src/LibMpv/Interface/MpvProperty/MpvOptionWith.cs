using HanumanInstitute.LibMpv.Core;

namespace HanumanInstitute.LibMpv;

/// <summary>
/// Provides a base class for option types that allow extra values, such as Integer value also allowing Auto and No.
/// </summary>
/// <typeparam name="T">The type of regular data.</typeparam>
public class MpvOptionWith<T> : MpvOption<T, string>
    where T : struct
{
    public MpvOptionWith(MpvContext mpv, string name) :
        base(mpv, name)
    {
        Format = MpvFormat.String;
    }

    /// <summary>
    /// Sets the option to specified raw value.
    /// </summary>
    protected void SetValue(string value) =>
        Mpv.SetProperty(PropertyName, value);

    /// <summary>
    /// Sets the option to specified raw value.
    /// </summary>
    protected Task SetValueAsync(string value, MpvAsyncOptions? options = null) =>
        Mpv.SetPropertyAsync(PropertyName, value, options);

    /// <summary>
    /// Gets whether the option is specified raw value.
    /// </summary>
    protected bool GetValue(string value) =>
        GetValue(new[] { value });
    
    /// <summary>
    /// Gets whether the option is specified raw value.
    /// </summary>
    protected Task<bool> GetValueAsync(string value, MpvAsyncOptions? options = null) =>
        GetValueAsync(new[] { value }, options);

    /// <summary>
    /// Gets whether the option is in specified raw values.
    /// </summary>
    protected bool GetValue(IEnumerable<string> values)
    {
        var result = Mpv.GetProperty<string?>(PropertyName);
        return result != null && values.Contains(result);
    }
    
    /// <summary>
    /// Gets whether the option is in specified raw values.
    /// </summary>
    protected async Task<bool> GetValueAsync(IEnumerable<string> values, MpvAsyncOptions? options = null)
    {
        var result = await Mpv.GetPropertyAsync<string?>(PropertyName, options);
        return result != null && values.Contains(result);
    }

    // /// <summary>
    // /// Parse value as specified type without throwing any exception on failure.
    // /// </summary>
    // /// <param name="value">The raw value to parse.</param>
    // /// <returns>The typed parsed value.</returns>
    // protected override T? ParseValue(string? value)
    // {
    //     try
    //     {
    //         return base.ParseValue(value);
    //     }
    //     catch (FormatException)
    //     {
    //         return null;
    //     }
    //     catch (OverflowException)
    //     {
    //         return null;
    //     }
    // }
}
