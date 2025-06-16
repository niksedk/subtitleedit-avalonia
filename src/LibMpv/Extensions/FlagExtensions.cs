using System.Text.RegularExpressions;

namespace HanumanInstitute.LibMpv;

internal static class FlagExtensions
{
    /// <summary>
    /// Returns the flag represented by specified MPV-formatted value..
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="value">The value to parse.</param>
    /// <returns>The typed enumeration value.</returns>
    public static T? ParseMpvFlag<T>(string? value)
        where T : struct, Enum
    {
        foreach (T item in Enum.GetValues(typeof(T)))
        {
            if (FormatMpvFlag(item) == value)
            {
                return item;
            }
        }
        return default!;
    }

    /// <summary>
    /// Returns a flag name in MPV format, adding '-' between words. "EachFrame" becomes "each-frame".
    /// </summary>
    /// <param name="flag">The flag value to format.</param>
    /// <returns>The flag name in MPV's syntax.</returns>
    public static string FormatMpvFlag<T>(this T flag)
        where T : Enum
    {
        var value = flag?.ToString();
        value = s_regexFlagName.Replace(value, x => $"{x.Value[0]}-{x.Value[1]}");
        return value.ToLowerInvariant();
    }
    private static readonly Regex s_regexFlagName = new Regex("[a-z][A-Z]");

    /// <summary>
    /// Returns a list of flags in MPV format, combining flags with '+'. "EachFrame" "More" becomes "each-frame+more"
    /// </summary>
    /// <typeparam name="T">The type of flag enumeration.</typeparam>
    /// <param name="flags">The type of the flag array to normalize.</param>
    /// <returns>A string representation of the flags.</returns>
    public static string? FormatMpvFlag<T>(this IEnumerable<T> flags)
        where T : Enum
    {
        if (flags == null || !flags.Any())
        {
            return null;
        }

        var values = flags.Select(x => x.FormatMpvFlag());
        return string.Join("+", values);
    }
}
