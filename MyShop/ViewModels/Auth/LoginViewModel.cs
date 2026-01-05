using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Services.Auth;
using MyShop.Models.Auth;
using System.Threading.Tasks;

namespace MyShop.ViewModels.Auth;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly ICredentialService _credentialService;
    private readonly ISessionService _sessionService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _shopName = string.Empty;

    [ObservableProperty]
    private bool _isLoginMode = true;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _rememberMe;

    public event EventHandler? LoginSuccessful;

    public LoginViewModel(IAuthService authService, ICredentialService credentialService, ISessionService sessionService)
    {
        _authService = authService;
        _credentialService = credentialService;
        _sessionService = sessionService;
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsLoginMode = !IsLoginMode;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private async Task Submit()
    {
        if (IsLoading) return;

        ErrorMessage = string.Empty;
        IsLoading = true;

        try
        {
            AuthResult result;

            if (IsLoginMode)
            {
                result = await _authService.LoginAsync(Email, Password);
            }
            else
            {
                result = await _authService.RegisterAsync(Email, Password, ShopName);
            }

            if (result.Success)
            {
                if (result.User != null)
                {
                    if (RememberMe)
                    {
                        var accessToken = _sessionService.AccessToken;
                        var refreshToken = _sessionService.RefreshToken;

                        if (!string.IsNullOrEmpty(accessToken)) _credentialService.SaveAccessToken(accessToken);
                        if (!string.IsNullOrEmpty(refreshToken)) _credentialService.SaveRefreshToken(refreshToken);
                    }
                    else
                    {
                        // Explicitly clear credentials if Remember Me is not checked
                        // This prevents auto-login next time if user previously had it on but now logs in without it.
                        _credentialService.ClearCredentials();
                    }
                }
                
                LoginSuccessful?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Authentication failed";
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
