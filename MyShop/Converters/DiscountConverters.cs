using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Converters;

/// <summary>
/// Converts discount status string to color for UI display.
/// Active = Green, Expired = Red, Other = Gray
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string status)
        {
            return status switch
            {
                "Active" => Windows.UI.Color.FromArgb(255, 34, 197, 94), // Green
                "Expired" => Windows.UI.Color.FromArgb(255, 239, 68, 68), // Red
                "Limit Reached" => Windows.UI.Color.FromArgb(255, 245, 158, 11), // Amber
                _ => Windows.UI.Color.FromArgb(255, 156, 163, 175) // Gray
            };
        }
        return Windows.UI.Color.FromArgb(255, 156, 163, 175);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts DateTime to short date string.
/// </summary>
public class DateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("MMM dd, yyyy");
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts count to visibility (0 = visible for empty state).
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int count)
        {
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}


/// <summary>
/// Converts IsActive bool to icon glyph (checkmark or block).
/// </summary>
public class ActiveIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isActive)
        {
            return isActive ? "\uE8FB" : "\uE711"; // CheckMark : BlockContact
        }
        return "\uE711";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts IsActive bool to tooltip text.
/// </summary>
public class ActiveTooltipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isActive)
        {
            return isActive ? "Deactivate" : "Activate";
        }
        return "Toggle Status";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
