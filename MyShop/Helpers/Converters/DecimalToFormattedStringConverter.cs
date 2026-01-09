using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Helpers.Converters;

/// <summary>
/// Converts a decimal value to a formatted string with thousand separators.
/// Example: 4290000 -> "4.290.000"
/// </summary>
public class DecimalToFormattedStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal decimalValue)
        {
            // Use Vietnamese number format with dot as thousand separator
            return decimalValue.ToString("N0", new System.Globalization.CultureInfo("vi-VN"));
        }
        
        if (value is int intValue)
        {
            return intValue.ToString("N0", new System.Globalization.CultureInfo("vi-VN"));
        }
        
        if (value is long longValue)
        {
            return longValue.ToString("N0", new System.Globalization.CultureInfo("vi-VN"));
        }
        
        return value?.ToString() ?? "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("DecimalToFormattedStringConverter does not support ConvertBack");
    }
}
