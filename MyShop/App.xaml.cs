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
using MyShop.Services.Discounts;
using MyShop.Services.Orders;
using MyShop.ViewModels;
using MyShop.ViewModels.Auth;
using MyShop.ViewModels.Categories;
using MyShop.ViewModels.Customers;
using MyShop.ViewModels.Dashboard;
using MyShop.ViewModels.Products;
using MyShop.ViewModels.Reports;
using MyShop.ViewModels.Discounts;
using MyShop.ViewModels.Orders;
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
        
        // Handle unhandled exceptions with logging
        this.UnhandledException += OnUnhandledException;
        
        LiveCharts.Configure(config =>
            config.AddSkiaSharp());
            
        // Disable all debug features
        #if DEBUG
        // Comment out all debug settings to disable debugging features
        // this.DebugSettings.EnableFrameRateCounter = false;
        // this.DebugSettings.IsBindingTracingEnabled = false;
        // this.DebugSettings.IsOverdrawHeatMapEnabled = false;
        #endif
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Debug.WriteLine($"=== UNHANDLED EXCEPTION ===");
        Debug.WriteLine($"Message: {e.Message}");
        Debug.WriteLine($"Exception: {e.Exception}");
        Debug.WriteLine($"Stack Trace: {e.Exception?.StackTrace}");
        Debug.WriteLine($"===========================");
        
        // Mark as handled so app doesn't crash and debugger doesn't break
        e.Handled = true;
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

        services.AddSingleton<IDiscountService>(sp =>
        {
            var sessionService = sp.GetRequiredService<ISessionService>();
            var httpClient = CreateAuthenticatedHttpClient(baseAddress, sessionService);
            return new DiscountService(httpClient);
        });

        services.AddSingleton<IOrderService>(sp =>
        {
            var sessionService = sp.GetRequiredService<ISessionService>();
            var httpClient = CreateAuthenticatedHttpClient(baseAddress, sessionService);
            return new OrderService(httpClient);
        });

        // ====================================================
        // Other Services
        // ====================================================
        services.AddSingleton<ProductChangeNotifier>();
        services.AddSingleton<IPrintService, PrintService>();

        // ====================================================
        // ViewModels
        // ====================================================
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ActivationViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProductViewModel>();
        services.AddTransient<CategoryViewModel>();
        services.AddTransient<CustomerViewModel>();
        services.AddTransient<ReportViewModel>();
        services.AddTransient<DiscountViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<CreateOrderViewModel>();

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
            var sessionService = Services.GetRequiredService<ISessionService>();

            if (MainWindow is MainWindow mainWindow)
            {
                if (credentialService.HasStoredCredentials())
                {
                    var result = await authService.RefreshTokenAsync();
                    if (result.Success)
                    {
                        // Check account status
                        if (result.AccountStatus?.Status == Models.Auth.AccountStatus.Expired)
                        {
                            mainWindow.ShowActivationPage();
                        }
                        else
                        {
                            mainWindow.ShowMainContent();
                        }
                        return;
                    }
                    else
                    {
                        credentialService.ClearCredentials();
                    }
                }

                mainWindow.ShowLoginPage();
            }
        }
        catch (Exception)
        {
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
        
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
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
