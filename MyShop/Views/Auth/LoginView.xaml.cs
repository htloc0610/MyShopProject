using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using MyShop.ViewModels.Auth;
using MyShop;
using System;
using MyShop.Models.Auth;

namespace MyShop.Views.Auth;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginView : Page
{
    public LoginViewModel ViewModel { get; }

    public LoginView()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<LoginViewModel>();
        ViewModel.LoginSuccessful += ViewModel_LoginSuccessful;
    }

    private void ViewModel_LoginSuccessful(object? sender, AccountStatusInfo? accountStatus)
    {
        if (App.MainWindow is MainWindow mainWindow)
        {
            // Check account status and navigate accordingly
            if (accountStatus?.Status == AccountStatus.Expired)
            {
                mainWindow.ShowActivationPage();
            }
            else
            {
                // Active or Trial - show main content
                mainWindow.OnLoginSuccess();
            }
        }
    }
}

// Converters inside the same namespace for simplicity or move to Converters folder
public class ModeToButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isLogin)
        {
            return isLogin ? "Login" : "Register";
        }
        return "Login"; // Default
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class StringIsNotEmptyToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return !string.IsNullOrEmpty(value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (value is bool b && b) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ModeToTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (value is bool b && b) ? "Login to your account" : "Create an account";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ModeToToggleTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (value is bool b && b) ? "Don't have an account?" : "Already have an account?";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ModeToToggleActionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (value is bool b && b) ? "Sign up" : "Login";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool b ? !b : true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is bool b ? !b : false;
    }
}
