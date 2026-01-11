using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels.Auth;
using System;

namespace MyShop.Views.Auth
{
    /// <summary>
    /// Page for entering license activation code.
    /// </summary>
    public sealed partial class ActivationPage : Page
    {
        public ActivationViewModel ViewModel { get; }

        public ActivationPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<ActivationViewModel>();
            ViewModel.ActivationSuccessful += OnActivationSuccessful;
        }

        private void OnActivationSuccessful(object? sender, EventArgs e)
        {
            // Navigate to main content using MainWindow
            if (App.MainWindow is MainWindow mainWindow)
            {
                mainWindow.ShowMainContent();
            }
        }
    }
}
