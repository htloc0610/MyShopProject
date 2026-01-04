using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Services.Auth;
using System.ComponentModel.DataAnnotations;

namespace MyShop.ViewModels.Auth
{
    /// <summary>
    /// ViewModel for the login page.
    /// Handles user authentication with email/password validation.
    /// </summary>
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly ISessionService _sessionService;

        public LoginViewModel(IAuthService authService, ISessionService sessionService)
        {
            _authService = authService;
            _sessionService = sessionService;
        }

        // ====== OBSERVABLE PROPERTIES ======

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string _email = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string _password = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string _shopName = string.Empty;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isRegisterMode;

        // ====== EVENTS ======

        /// <summary>
        /// Event raised when login is successful.
        /// </summary>
        public event EventHandler? LoginSuccessful;

        // ====== VALIDATION ======

        public bool IsEmailValid => !string.IsNullOrWhiteSpace(Email) && 
            new EmailAddressAttribute().IsValid(Email);

        public bool IsPasswordValid => !string.IsNullOrWhiteSpace(Password) && 
            Password.Length >= 6;

        public bool IsShopNameValid => !string.IsNullOrWhiteSpace(ShopName);

        private bool CanLogin => IsEmailValid && IsPasswordValid && !IsLoading;

        private bool CanRegister => IsEmailValid && IsPasswordValid && IsShopNameValid && !IsLoading;

        // ====== COMMANDS ======

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            if (!CanLogin) return;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var result = await _authService.LoginAsync(Email, Password);

                if (result.Success)
                {
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Login failed. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanRegister))]
        private async Task RegisterAsync()
        {
            if (!CanRegister) return;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var result = await _authService.RegisterAsync(Email, Password, ShopName);

                if (result.Success)
                {
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Registration failed. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ToggleMode()
        {
            IsRegisterMode = !IsRegisterMode;
            ErrorMessage = null;
        }

        [RelayCommand]
        private void ClearError()
        {
            ErrorMessage = null;
        }

        // ====== METHODS ======

        /// <summary>
        /// Try to restore session from stored credentials.
        /// </summary>
        public async Task<bool> TryRestoreSessionAsync()
        {
            if (await _sessionService.TryRestoreSessionAsync())
            {
                // Verify the session is still valid by fetching user info
                var user = await _authService.GetCurrentUserAsync();
                if (user != null)
                {
                    return true;
                }
                else
                {
                    // Try to refresh the token
                    var result = await _authService.RefreshTokenAsync();
                    return result.Success;
                }
            }

            return false;
        }
    }
}
