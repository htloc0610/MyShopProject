using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using MyShop.Views.Products;
using MyShop.Views.Categories;
using MyShop.Views.Dashboard;
using MyShop.Views.Auth;
using MyShop.Services.Auth;

namespace MyShop;

/// <summary>
/// Main application window with NavigationView.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; private set; } = null!;
    private ISessionService? _sessionService;
    private IAuthService? _authService;

    public MainWindow()
    {
        try
        {
            Debug.WriteLine("=== MainWindow: Starting initialization ===");
            InitializeComponent();
            Debug.WriteLine("=== MainWindow: InitializeComponent done ===");

            ViewModel = App.Current.Services.GetRequiredService<MainWindowViewModel>();
            _sessionService = App.Current.Services.GetRequiredService<ISessionService>();
            _authService = App.Current.Services.GetRequiredService<IAuthService>();

            // Set window size
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(1400, 900));

            // Show login page
            ShowLoginPage();
            Debug.WriteLine("=== MainWindow: Initialization complete ===");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"=== MainWindow ERROR: {ex.Message} ===");
            Debug.WriteLine($"=== Stack: {ex.StackTrace} ===");
        }
    }

    /// <summary>
    /// Show the login page.
    /// </summary>
    private void ShowLoginPage()
    {
        Debug.WriteLine("=== ShowLoginPage: Starting ===");
        
        // Show login container, hide main nav
        LoginContainer.Visibility = Visibility.Visible;
        NavView.Visibility = Visibility.Collapsed;

        var loginView = new LoginView();
        loginView.LoginSuccessful += (s, e) => ShowMainContent();
        
        // Navigate to login page in the login frame
        LoginFrame.Content = loginView;
        Debug.WriteLine("=== ShowLoginPage: Done ===");
    }

    /// <summary>
    /// Show the main content after successful login.
    /// </summary>
    private void ShowMainContent()
    {
        Debug.WriteLine("=== ShowMainContent: Starting ===");
        
        // Hide login container, show main nav
        LoginContainer.Visibility = Visibility.Collapsed;
        NavView.Visibility = Visibility.Visible;

        // Navigate to Dashboard
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(Dashboard));

        // Load product count
        _ = ViewModel.LoadProductCountAsync();
        Debug.WriteLine("=== ShowMainContent: Done ===");
    }

    /// <summary>
    /// Handle logout.
    /// </summary>
    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        if (_authService != null)
        {
            await _authService.LogoutAsync();
        }
        ShowLoginPage();
    }

    /// <summary>
    /// Handles navigation when user selects a menu item.
    /// </summary>
    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            return;
        }

        var selectedItem = args.SelectedItemContainer as NavigationViewItem;
        if (selectedItem == null) return;

        string tag = selectedItem.Tag?.ToString() ?? string.Empty;

        Type? pageType = tag switch
        {
            "Dashboard" => typeof(Dashboard),
            "ProductList" => typeof(ProductListPage),
            "AddProduct" => typeof(AddProductPage),
            "Categories" => typeof(CategoryListPage),
            _ => null
        };

        if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }
}
