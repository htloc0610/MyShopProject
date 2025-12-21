using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MyShop.Services;
using MyShop.ViewModels;

namespace MyShop;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// This class is responsible for configuring Dependency Injection and managing the app lifecycle.
/// </summary>
public partial class App : Application
{
    private Window? _window;

    /// <summary>
    /// Gets the current App instance for accessing services globally.
    /// This pattern allows any component to access the DI container.
    /// </summary>
    public static new App Current => (App)Application.Current;

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
    /// DI Lifecycle Overview:
    /// - Singleton: One instance for the entire application lifetime.
    ///   Use for: Stateless services, caching, configuration.
    /// - Transient: New instance each time it's requested.
    ///   Use for: Lightweight, stateless services, ViewModels.
    /// - Scoped: One instance per scope (not commonly used in desktop apps).
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register Services
        // DataService is registered as Singleton - same instance shared across the app
        // This is suitable for services that maintain state or are expensive to create
        services.AddSingleton<IDataService, DataService>();

        // Register ViewModels
        // ViewModels are registered as Transient - new instance for each request
        // Each view gets its own ViewModel instance
        services.AddTransient<MainViewModel>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
