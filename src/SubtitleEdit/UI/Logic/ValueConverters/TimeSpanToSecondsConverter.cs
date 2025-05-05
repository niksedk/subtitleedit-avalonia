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
            return timeSpan.TotalSeconds.ToString("0.000");
        }

        return "0.000";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return TimeSpan.Parse(stringValue);
        }
        
        if (value is decimal decimalSeconds)
        {
            return TimeSpan.FromSeconds((double)decimalSeconds);
        }

        if (value is double doubleSeconds)
        {
            return TimeSpan.FromSeconds(doubleSeconds);
        }

        if (value is float floatSeconds)
        {
            return TimeSpan.FromSeconds((double)floatSeconds);
        }

        return TimeSpan.Zero;
    }
}
