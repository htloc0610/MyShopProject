using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Converters
{
    /// <summary>
    /// Converts boolean to Visibility (true = Visible, false = Collapsed).
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool boolValue && boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }

    /// <summary>
    /// Converts boolean to Visibility (true = Collapsed, false = Visible).
    /// Inverse of BoolToVisibilityConverter.
    /// </summary>
    public class BoolToVisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool boolValue && boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility visibility && visibility == Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Alias for BoolToVisibilityInverseConverter (for backward compatibility).
    /// </summary>
    public class BoolToInverseVisibilityConverter : BoolToVisibilityInverseConverter
    {
    }

    /// <summary>
    /// Inverts a boolean value (true -> false, false -> true).
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool boolValue && !boolValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is bool boolValue && !boolValue;
        }
    }

    /// <summary>
    /// Converts string to boolean (empty/null = false, otherwise = true).
    /// </summary>
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !string.IsNullOrWhiteSpace(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts string to Visibility (empty/null = Collapsed, otherwise = Visible).
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return string.IsNullOrWhiteSpace(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts price to formatted currency string.
    /// </summary>
    public class PriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal price)
            {
                return $"{price:N0} VN?";
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts sort direction boolean to glyph (arrow icon).
    /// false (ascending) = UpArrow, true (descending) = DownArrow
    /// </summary>
    public class SortDirectionToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isDescending)
            {
                return isDescending ? "\uE74B" : "\uE74A"; // Down arrow : Up arrow
            }
            if (value is string direction)
            {
                return direction.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "\uE74B" : "\uE74A";
            }
            return "\uE74A"; // Default: Up arrow
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts integer index to display number (adds 1 for human-readable numbering).
    /// Used for image gallery pagination: 0 -> 1, 1 -> 2, etc.
    /// </summary>
    public class IndexToDisplayNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int index)
            {
                return (index + 1).ToString();
            }
            return "1";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts decimal to double for NumberBox binding.
    /// </summary>
    public class DecimalToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal decimalValue)
            {
                return (double)decimalValue;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double doubleValue)
            {
                return (decimal)doubleValue;
            }
            return 0m;
        }
    }

    /// <summary>
    /// Converts int to double for NumberBox binding.
    /// </summary>
    public class IntToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int intValue)
            {
                return (double)intValue;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double doubleValue)
            {
                return (int)doubleValue;
            }
            return 0;
        }
    }

    /// <summary>
    /// Converts null to Visibility (null = Collapsed, not null = Visible).
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts null to Visibility inverse (null = Visible, not null = Collapsed).
    /// </summary>
    public class NullToVisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Converts relative URL to absolute URL (prepending API base address).
    /// </summary>
    public class RelativeToAbsoluteUrlConverter : IValueConverter
    {
        private const string BaseUrl = "http://localhost:5002";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    return url;
                }
                
                // Handle leading slash
                var cleanUrl = url.StartsWith("/") ? url : "/" + url;
                return BaseUrl + cleanUrl;
            }
            // Return placeholder or empty
            return "/Assets/StoreLogo.png"; // Or null
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts boolean to string based on parameter.
    /// Parameter format: "TrueString|FalseString"
    /// </summary>
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue && parameter is string paramStr)
            {
                var parts = paramStr.Split('|');
                if (parts.Length == 2)
                {
                    return boolValue ? parts[0] : parts[1];
                }
            }
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts boolean to Glyph string based on parameter.
    /// Parameter format: "TrueGlyph|FalseGlyph"
    /// </summary>
    public class BoolToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue && parameter is string paramStr)
            {
                // Unescape strings if needed, but XAML usually handles passing unicode chars
                var parts = paramStr.Split('|');
                if (parts.Length == 2)
                {
                    return boolValue ? parts[0] : parts[1];
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts by comparing a value to a parameter (returns true if equal).
    /// Used for highlighting the active sort button.
    /// </summary>
    public class StringEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || parameter == null)
                return false;

            return value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
