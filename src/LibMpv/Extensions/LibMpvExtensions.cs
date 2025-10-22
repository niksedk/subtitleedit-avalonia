using System.ComponentModel;
using System.Globalization;

namespace HanumanInstitute.LibMpv.Extensions;

internal static class LibMpvExtensions
{
    public static string ToStringInvariant<T>(this T value)
    {
        return FormattableString.Invariant($"{value}");
    }

    public static string CheckNotNullOrEmpty(this string? value, string name)
    {
        value.CheckNotNull(name);
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(name, "Value cannot be null or empty.");
        }

        return value!;
    }

    public static T CheckNotNull<T>(this T value, string name)
    {
        if (value == null)
        {
            throw new ArgumentNullException(name);
        }

        return value;
    }

    public static IEnumerable<T>? CheckNotNullOrEmpty<T>(this IEnumerable<T>? value, string name)
    {
        value.CheckNotNull(name);
        if (!value.Any())
        {
            throw new ArgumentNullException(name, "Value cannot be null or empty.");
        }

        return value;
    }

    public static bool HasValue(this string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    public static T? Parse<T>(this string? input) where T : struct
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        try
        {
            return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(input);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public static string FormatInvariant(this string format, params object?[] args)
    {
        return string.Format(CultureInfo.InvariantCulture, format, args);
    }

    public static IEnumerable<T> GetFlags<T>(this T value) where T : Enum
    {
        ulong valueLong = Convert.ToUInt64(value, CultureInfo.InvariantCulture);
        foreach (T item in value.GetType().GetEnumValues().Cast<T>())
        {
            ulong num = Convert.ToUInt64(item, CultureInfo.InvariantCulture);
            if ((num & (num - 1)) == 0L && (valueLong & num) != 0L)
            {
                yield return item;
            }
        }
    }

    public static T CheckRange<T>(this T value, string name, T? min = null, bool minInclusive = true, T? max = null, bool maxInclusive = true) where T : struct, IComparable<T>
    {
        if (!value.IsInRange(min, minInclusive, max, maxInclusive))
        {
            string? rangeError = value.GetRangeError(name, min, minInclusive, max, maxInclusive);
            if (min.HasValue && minInclusive && max.HasValue && maxInclusive)
            {
                string valueRangeBetween = "The value of '{0}' must be between {1} and {2}, inclusive.";
                throw new ArgumentOutOfRangeException(name, value, valueRangeBetween.FormatInvariant(name, min, max));
            }

            if (rangeError != null)
            {
                throw new ArgumentOutOfRangeException(name, value, rangeError);
            }

            throw new ArgumentOutOfRangeException(name, value, "The value is out of range.");
        }

        return value;
    }

    public static string? GetRangeError<T>(this T value, string name, T? min = null, bool minInclusive = true, T? max = null, bool maxInclusive = true) where T : struct, IComparable<T>
    {
        if (value.IsInRange(min, minInclusive, max, maxInclusive))
        {
            return null;
        }

        string? text = min.HasValue ? GetOpText(greaterThan: true, minInclusive).FormatInvariant(min) : null;
        string? text2 = max.HasValue ? GetOpText(greaterThan: false, maxInclusive).FormatInvariant(max) : null;

        if (text != null && text2 != null)
        {
            return $"{name} must be {text} and {text2}.";
        }
        else
        {
            return $"{name} must be {text ?? text2}.";
        }
    }

    private static string GetOpText(bool greaterThan, bool inclusive)
    {
        if (greaterThan && inclusive)
        {
            return "greater than or equal to {0}";
        }
        else if (greaterThan)
        {
            return "greater than {0}";
        }
        else if (inclusive)
        {
            return "less than or equal to {0}";
        }
        else
        {
            return "less than {0}";
        }
    }

    public static bool IsInRange<T>(this T value, T? min = null, bool minInclusive = true, T? max = null, bool maxInclusive = true) where T : struct, IComparable<T>
    {
        bool num = !min.HasValue || (minInclusive && value.CompareTo(min.Value) >= 0) || (!minInclusive && value.CompareTo(min.Value) > 0);
        bool flag = !max.HasValue || (maxInclusive && value.CompareTo(max.Value) <= 0) || (!maxInclusive && value.CompareTo(max.Value) < 0);
        return num && flag;
    }

    public static T CheckEnumValid<T>(this T value, string name) where T : Enum
    {
        int num = Convert.ToInt32(value, CultureInfo.InvariantCulture);
        bool flag = Enum.IsDefined(typeof(T), num);
        if (!flag && IsEnumTypeFlags<T>())
        {
            flag = CheckEnumValidFlags<T>(num);
        }

        if (!flag)
        {
            throw new ArgumentException($"The value '{value}' is not valid for enum type '{typeof(T).Name}'.", name);
        }

        return value;
    }

    private static bool IsEnumTypeFlags<T>() where T : Enum
    {
        return typeof(T).GetCustomAttributes(typeof(FlagsAttribute), inherit: true).Any();
    }

    private static bool CheckEnumValidFlags<T>(int value) where T : Enum
    {
        int num = 0;
        foreach (object value2 in Enum.GetValues(typeof(T)))
        {
            num |= (int)value2;
        }

        return (num & value) == value;
    }
}
