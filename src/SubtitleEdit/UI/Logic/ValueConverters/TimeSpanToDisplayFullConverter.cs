using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public  class TimeSpanToDisplayFullConverter : IValueConverter
{
    public static readonly TimeSpanToDisplayFullConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2},{3:D3}",
                ts.Hours,
                ts.Minutes,
                ts.Seconds,
                ts.Milliseconds);
        }

        return "00:00:00,000";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            // Expect format: hh:mm:ss,fff
            if (TimeSpan.TryParseExact(s.Replace(',', '.'), @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture, out var ts))
            {
                return ts;
            }
        }

        return TimeSpan.Zero;
    }
}