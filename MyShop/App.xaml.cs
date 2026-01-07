using LiveChartsCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MyShop.Services.Auth;
using MyShop.Services.Categories;
using MyShop.Services.Customers;
using MyShop.Services.Dashboard;
using MyShop.Services.Products;
using MyShop.Services.Reports;
using MyShop.Services.Shared;
using MyShop.ViewModels;
using MyShop.ViewModels.Auth;
using MyShop.ViewModels.Categories;
using MyShop.ViewModels.Customers;
using MyShop.ViewModels.Dashboard;
using MyShop.ViewModels.Products;
using MyShop.ViewModels.Reports;
using System;
using System.Diagnostics;
using System.Net.Http;
using LiveChartsCore.SkiaSharpView;

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
        LiveCharts.Configure(config =>
            config.AddSkiaSharp());
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

        services.AddSingleton<ReportService>(sp =>
        {
            var sessionService = sp.GetRequiredService<ISessionService>();
            var httpClient = CreateAuthenticatedHttpClient(baseAddress, sessionService);
            return new ReportService(httpClient);
        });

        services.AddSingleton<ICustomerService>(sp =>
        {
            var sessionService = sp.GetRequiredService<ISessionService>();
            var httpClient = CreateAuthenticatedHttpClient(baseAddress, sessionService);
            return new CustomerService(httpClient);
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
        services.AddTransient<CustomerViewModel>();
        services.AddTransient<ReportViewModel>();
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
    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        MainWindow = _window;
        _window.Activate();

        await CheckAutoLoginAsync();
    }

    private async System.Threading.Tasks.Task CheckAutoLoginAsync()
    {
        try
        {
            var credentialService = Services.GetRequiredService<ICredentialService>();
            var authService = Services.GetRequiredService<IAuthService>();
            var sessionService = Services.GetRequiredService<ISessionService>(); // Ensure session is ready

            if (MainWindow is MainWindow mainWindow)
            {
                if (credentialService.HasStoredCredentials())
                {
                    Debug.WriteLine("=== App: Found stored credentials, attempting silent login... ===");
                    // We need to preload the session with the token so AuthService can use it if needed,
                    // or just rely on RefreshTokenAsync which uses CredentialService directly.
                    // AuthService.RefreshTokenAsync uses CredentialService.GetRefreshToken().
                    
                    var result = await authService.RefreshTokenAsync();
                    if (result.Success)
                    {
                        Debug.WriteLine("=== App: Silent login success ===");
                        mainWindow.ShowMainContent();
                        return;
                    }
                    else
                    {
                        Debug.WriteLine($"=== App: Silent login failed: {result.ErrorMessage} ===");
                        // Token invalid/expired and refresh failed -> Clear and show login
                        credentialService.ClearCredentials();
                    }
                }
                else
                {
                    Debug.WriteLine("=== App: No stored credentials ===");
                }

                // Fallback to login page
                mainWindow.ShowLoginPage();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"=== App: Auto-login error: {ex.Message} ===");
            if (MainWindow is MainWindow mw)
            {
                mw.ShowLoginPage();
            }
        }
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
