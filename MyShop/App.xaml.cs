using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MyShop.Services.Products;
using MyShop.Services.Categories;
using MyShop.Services.Dashboard;
using MyShop.Services.Shared;
using MyShop.Services.Auth;
using MyShop.ViewModels;
using MyShop.ViewModels.Products;
using MyShop.ViewModels.Categories;
using MyShop.ViewModels.Dashboard;
using MyShop.ViewModels.Auth;
using System.Diagnostics;

namespace MyShop;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// Configures Dependency Injection for MVVM architecture with authentication services.
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
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // ====================================================
        // Base HttpClient (shared)
        // ====================================================
        var baseAddress = new Uri("http://localhost:5002/");

        // ====================================================
        // Authentication Services (register first as dependencies)
        // ====================================================
        services.AddSingleton<ICredentialService, CredentialService>();
        services.AddSingleton<ISessionService, SessionService>();
        
        // Auth service - doesn't need auth handler (it's for login/register)
        services.AddSingleton<IAuthService>(sp =>
        {
            var httpClient = new HttpClient { BaseAddress = baseAddress };
            return new AuthService(
                httpClient,
                sp.GetRequiredService<ISessionService>(),
                sp.GetRequiredService<ICredentialService>());
        });

        // ====================================================
        // Authenticated Services (need bearer token)
        // ====================================================
        services.AddSingleton<IProductService>(sp =>
        {
            var sessionService = sp.GetRequiredService<ISessionService>();
            var httpClient = CreateAuthenticatedHttpClient(baseAddress, sessionService);
            return new ProductService(httpClient);
        });

        services.AddSingleton<ICategoryService>(sp =>
        {
            var sessionService = sp.GetRequiredService<ISessionService>();
            var httpClient = CreateAuthenticatedHttpClient(baseAddress, sessionService);
            return new CategoryService(httpClient);
        });

        services.AddSingleton<DashboardService>(sp =>
        {
            var sessionService = sp.GetRequiredService<ISessionService>();
            var httpClient = CreateAuthenticatedHttpClient(baseAddress, sessionService);
            return new DashboardService(httpClient);
        });

        // ====================================================
        // Other Services
        // ====================================================
        services.AddSingleton<ProductChangeNotifier>();

        // ====================================================
        // ViewModels
        // ====================================================
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProductViewModel>();
        services.AddTransient<CategoryViewModel>();
        services.AddTransient<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Creates an HttpClient that automatically adds Bearer token to requests.
    /// </summary>
    private static HttpClient CreateAuthenticatedHttpClient(Uri baseAddress, ISessionService sessionService)
    {
        var handler = new AuthenticatedHttpMessageHandler(sessionService);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = baseAddress
        };
        return httpClient;
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

/// <summary>
/// HTTP message handler that adds Bearer token to requests.
/// </summary>
internal class AuthenticatedHttpMessageHandler : HttpClientHandler
{
    private readonly ISessionService _sessionService;

    public AuthenticatedHttpMessageHandler(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    protected override async System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        System.Threading.CancellationToken cancellationToken)
    {
        var accessToken = _sessionService.AccessToken;
        Debug.WriteLine($"=== API Request: {request.RequestUri} ===");
        Debug.WriteLine($"=== Token Available: {!string.IsNullOrEmpty(accessToken)} ===");
        
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            Debug.WriteLine("=== Token attached to header ===");
        }
        else
        {
            Debug.WriteLine("=== NO TOKEN attached ===");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

/// <summary>
/// Extension method for getting required services with null check.
/// </summary>
public static class ServiceProviderExtensions
{
    public static T GetRequiredService<T>(this IServiceProvider provider) where T : notnull
    {
        return provider.GetService<T>() ?? throw new InvalidOperationException($"Service {typeof(T)} not registered");
    }
}
