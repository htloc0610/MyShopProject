using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Services.Auth;
using MyShop.Services.Shared;
using MyShop.Models.Auth;
using System.Threading.Tasks;

namespace MyShop.ViewModels.Auth;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly ICredentialService _credentialService;
    private readonly ISessionService _sessionService;
    private readonly IToastService _toastService;

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

    public event EventHandler<AccountStatusInfo?>? LoginSuccessful;

    public LoginViewModel(
        IAuthService authService, 
        ICredentialService credentialService, 
        ISessionService sessionService,
        IToastService toastService)
    {
        _authService = authService;
        _credentialService = credentialService;
        _sessionService = sessionService;
        _toastService = toastService;
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

        // Clear previous error
        ErrorMessage = string.Empty;
        
        // Validation
        if (string.IsNullOrWhiteSpace(Email))
        {
            _toastService.ShowError("Email không được để trống!");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            _toastService.ShowError("Mật khẩu không được để trống!");
            return;
        }

        if (!IsLoginMode && string.IsNullOrWhiteSpace(ShopName))
        {
            _toastService.ShowError("Tên cửa hàng không được để trống!");
            return;
        }

        // Validate password strength for registration
        if (!IsLoginMode && !ValidatePassword(Password))
        {
            return; // Error toast already shown in ValidatePassword
        }

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
                // Show success message
                if (IsLoginMode)
                {
                    _toastService.ShowSuccess($"Chào mừng trở lại, {result.User?.Email}!");
                }
                else
                {
                    _toastService.ShowSuccess("Đăng ký thành công! Chào mừng bạn đến với dịch vụ của chúng tôi!");
                }

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
                        _credentialService.ClearCredentials();
                    }
                }
                
                // Pass account status to the event handler
                LoginSuccessful?.Invoke(this, result.AccountStatus);
            }
            else
            {
                // Show error toast only
                var errorMsg = result.ErrorMessage ?? (IsLoginMode ? "Đăng nhập thất bại" : "Đăng ký thất bại");
                _toastService.ShowError(errorMsg);
            }
        }
        catch (Exception ex)
        {
            // Show exception as error toast only
            _toastService.ShowError($"Lỗi: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Validate password strength according to backend requirements.
    /// </summary>
    private bool ValidatePassword(string password)
    {
        // Minimum length: 6 characters
        if (password.Length < 6)
        {
            _toastService.ShowError("Mật khẩu phải có ít nhất 6 ký tự!");
            return false;
        }

        // Must contain at least one uppercase letter
        if (!password.Any(char.IsUpper))
        {
            _toastService.ShowError("Mật khẩu phải có ít nhất 1 chữ in hoa!");
            return false;
        }

        // Must contain at least one lowercase letter
        if (!password.Any(char.IsLower))
        {
            _toastService.ShowError("Mật khẩu phải có ít nhất 1 chữ thường!");
            return false;
        }

        // Must contain at least one digit
        if (!password.Any(char.IsDigit))
        {
            _toastService.ShowError("Mật khẩu phải có ít nhất 1 chữ số!");
            return false;
        }

        return true;
    }
}
