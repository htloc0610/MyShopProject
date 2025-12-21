using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MyShop.ViewModels;

namespace MyShop;

/// <summary>
/// Main application window.
/// Retrieves its ViewModel from the DI container for proper dependency injection.
/// </summary>
public sealed partial class MainWindow : Window
{
    /// <summary>
    /// Gets the ViewModel for this window.
    /// The ViewModel is resolved from the DI container, ensuring all its
    /// dependencies (like IDataService) are properly injected.
    /// </summary>
    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        InitializeComponent();

        // Resolve MainViewModel from the DI container
        // This ensures IDataService is automatically injected into the ViewModel
        ViewModel = App.Current.Services.GetRequiredService<MainViewModel>();
    }
}
