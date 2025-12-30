using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MyShop.Services.Products;
using MyShop.Services.Categories;
using MyShop.Services.Dashboard;
using MyShop.Services.Shared;
using MyShop.ViewModels;
using MyShop.ViewModels.Products;
using MyShop.ViewModels.Categories;
using MyShop.ViewModels.Dashboard;

namespace MyShop;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// Configures Dependency Injection for MVVM architecture with ProductService and DashboardService.
/// </summary>
public partial class App : Application
{
    private Window? _window;

    /// <summary>
    /// Gets the current App instance for accessing services globally.
    /// </summary>
    public static new App Current => (App)Application.Current;

    /// <summary>
    /// Gets the main window for accessing its HWND.
    /// </summary>
    public static Window? MainWindow { get; private set; }

    /// <summary>
    /// Gets the service provider for resolving dependencies.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Initializes the singleton application object.
    /// Sets up the Dependency Injection container with all required services.
    /// </summary>
    public App()
    {
        Services = ConfigureServices();
        InitializeComponent();
    }

    /// <summary>
    /// Configures and builds the service provider with all application services.
    /// 
    /// Architecture:
    /// - Services: Singleton HttpClient-based services for API communication
    /// - ViewModels: Transient instances, one per view
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register Services with HttpClient
        services.AddHttpClient<IProductService, ProductService>();
        services.AddHttpClient<ICategoryService, CategoryService>();
        services.AddHttpClient<DashboardService>();

        // Register ProductChangeNotifier as Singleton
        services.AddSingleton<ProductChangeNotifier>();

        // Register ViewModels as Transient
        // Each view gets its own ViewModel instance
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProductViewModel>();
        services.AddTransient<CategoryViewModel>();
        services.AddTransient<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        MainWindow = _window;
        _window.Activate();
    }
}
