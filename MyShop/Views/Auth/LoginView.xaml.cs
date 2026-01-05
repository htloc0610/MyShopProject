using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShop.ViewModels.Auth;

namespace MyShop.Views.Auth
{
    /// <summary>
    /// Login page with email/password authentication.
    /// </summary>
    public sealed partial class LoginView : Page
    {
        public LoginViewModel ViewModel { get; private set; } = null!;
        private bool _isRegisterMode = false;

        /// <summary>
        /// Event raised when login is successful.
        /// </summary>
        public event EventHandler? LoginSuccessful;

        public LoginView()
        {
            try
            {
                Debug.WriteLine("=== LoginView: Constructor start ===");
                
                InitializeComponent();
                Debug.WriteLine("=== LoginView: InitializeComponent done ===");
                
                ViewModel = App.Current.Services.GetRequiredService<LoginViewModel>();
                Debug.WriteLine("=== LoginView: Got LoginViewModel ===");

                // Subscribe to ViewModel's login success event
                ViewModel.LoginSuccessful += (s, e) => LoginSuccessful?.Invoke(this, EventArgs.Empty);
                Debug.WriteLine("=== LoginView: Events subscribed ===");

                // Subscribe to property changes for UI updates
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
                Debug.WriteLine("=== LoginView: PropertyChanged subscribed ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== LoginView ERROR: {ex.GetType().Name}: {ex.Message} ===");
                Debug.WriteLine($"=== Stack: {ex.StackTrace} ===");
                throw;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                switch (e.PropertyName)
                {
                    case nameof(ViewModel.IsLoading):
                        LoadingRing.IsActive = ViewModel.IsLoading;
                        LoadingRing.Visibility = ViewModel.IsLoading ? Visibility.Visible : Visibility.Collapsed;
                        EmailTextBox.IsEnabled = !ViewModel.IsLoading;
                        PasswordBox.IsEnabled = !ViewModel.IsLoading;
                        ShopNameTextBox.IsEnabled = !ViewModel.IsLoading;
                        PrimaryButton.IsEnabled = !ViewModel.IsLoading;
                        break;
                    case nameof(ViewModel.ErrorMessage):
                        ErrorInfoBar.Message = ViewModel.ErrorMessage ?? string.Empty;
                        ErrorInfoBar.IsOpen = !string.IsNullOrEmpty(ViewModel.ErrorMessage);
                        break;
                    case nameof(ViewModel.IsRegisterMode):
                        UpdateUIForMode();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== PropertyChanged ERROR: {ex.Message} ===");
            }
        }

        private void UpdateUIForMode()
        {
            _isRegisterMode = ViewModel.IsRegisterMode;
            ShopNameTextBox.Visibility = _isRegisterMode ? Visibility.Visible : Visibility.Collapsed;
            PrimaryButton.Content = _isRegisterMode ? "Register" : "Sign In";
            ToggleModeButton.Content = _isRegisterMode 
                ? "Already have an account? Sign In" 
                : "Don't have an account? Register";
            SubtitleText.Text = _isRegisterMode 
                ? "Create a new account" 
                : "Sign in to your account";
        }

        private async void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            // Bind UI to ViewModel
            ViewModel.Email = EmailTextBox.Text;
            ViewModel.Password = PasswordBox.Password;
            ViewModel.ShopName = ShopNameTextBox.Text;

            if (_isRegisterMode)
            {
                if (ViewModel.RegisterCommand.CanExecute(null))
                {
                    await ViewModel.RegisterCommand.ExecuteAsync(null);
                }
            }
            else
            {
                if (ViewModel.LoginCommand.CanExecute(null))
                {
                    await ViewModel.LoginCommand.ExecuteAsync(null);
                }
            }
        }

        private void ToggleModeButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ToggleModeCommand.Execute(null);
        }

        private async void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // Bind UI to ViewModel
                ViewModel.Email = EmailTextBox.Text;
                ViewModel.Password = PasswordBox.Password;
                ViewModel.ShopName = ShopNameTextBox.Text;

                if (!_isRegisterMode && ViewModel.LoginCommand.CanExecute(null))
                {
                    await ViewModel.LoginCommand.ExecuteAsync(null);
                }
            }
        }

        private void ErrorInfoBar_Closed(InfoBar sender, InfoBarClosedEventArgs args)
        {
            ViewModel.ClearErrorCommand.Execute(null);
        }
    }
}
