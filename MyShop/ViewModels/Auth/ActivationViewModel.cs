using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Services.Auth;
using System;
using System.Threading.Tasks;

namespace MyShop.ViewModels.Auth
{
    /// <summary>
    /// ViewModel for the activation page where users enter their license key.
    /// </summary>
    public partial class ActivationViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _activationCode = string.Empty;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private bool _isActivating;

        /// <summary>
        /// Event raised when activation is successful.
        /// </summary>
        public event EventHandler? ActivationSuccessful;

        public ActivationViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Validate and activate the account with the entered code.
        /// </summary>
        [RelayCommand]
        private async Task ActivateAsync()
        {
            ErrorMessage = null;

            // Validate input
            if (string.IsNullOrWhiteSpace(ActivationCode))
            {
                ErrorMessage = "Please enter an activation code.";
                return;
            }

            if (ActivationCode.Length != 6)
            {
                ErrorMessage = "Activation code must be 6 characters.";
                return;
            }

            IsActivating = true;

            try
            {
                var result = await _authService.ActivateAsync(ActivationCode.ToUpper());

                if (result.Success)
                {
                    // Notify the page to navigate to dashboard
                    ActivationSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Activation failed. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsActivating = false;
            }
        }

        /// <summary>
        /// Handle text input to auto-uppercase and limit to 6 characters.
        /// </summary>
        partial void OnActivationCodeChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var processed = value.ToUpper();
                if (processed.Length > 6)
                {
                    processed = processed.Substring(0, 6);
                }
                
                // Only update if different to avoid infinite loop
                if (processed != value)
                {
                    ActivationCode = processed;
                }
            }
        }
    }
}
