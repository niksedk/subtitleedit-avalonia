using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.UI.Logic.ValueConverters;

public class TimeSpanToSecondsConverter : IValueConverter
{
    public static readonly TimeSpanToSecondsConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            return timeSpan.TotalSeconds;
        }

        return 0.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        if (value is decimal decimalSeconds)
        {
            return TimeSpan.FromSeconds((double)decimalSeconds);
        }

        if (value is float floatSeconds)
        {
            return TimeSpan.FromSeconds((double)floatSeconds);
        }

        return TimeSpan.Zero;
    }
}
