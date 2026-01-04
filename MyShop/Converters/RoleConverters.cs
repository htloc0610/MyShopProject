using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Converters
{
    /// <summary>
    /// Converts role to visibility based on role requirements.
    /// Use ConverterParameter to specify the required role(s).
    /// Example: ConverterParameter="Owner" will show only for Owner role.
    /// </summary>
    public class RoleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not string currentRole || parameter is not string requiredRoles)
            {
                return Visibility.Collapsed;
            }

            // Check if current role matches any of the required roles
            var roles = requiredRoles.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var role in roles)
            {
                if (string.Equals(currentRole.Trim(), role.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts IsOwner boolean to visibility.
    /// Shows content only for Owner role.
    /// </summary>
    public class OwnerOnlyVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool isOwner && isOwner ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts IsOwner boolean to enabled state.
    /// Enables content only for Owner role.
    /// </summary>
    public class OwnerOnlyEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool isOwner && isOwner;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
