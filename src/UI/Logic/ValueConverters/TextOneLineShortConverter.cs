using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class TextOneLineShortConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            if (str.Length < 25)
            {
                return str.Replace('n', ' ').Replace('r', ' ');
            }
            
            return str; // shorten
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}