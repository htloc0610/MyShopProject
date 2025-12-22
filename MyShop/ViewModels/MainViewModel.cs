using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

/// <summary>
/// Main ViewModel for the application.
/// Inherits from ObservableObject to provide INotifyPropertyChanged implementation.
/// Uses Source Generators for cleaner property and command definitions.
/// 
/// DI Lifecycle Note:
/// - Registered as TRANSIENT: A new instance is created each time it's requested.
///   This is typical for ViewModels as each View gets its own instance.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    /// <summary>
    /// Message displayed in the UI.
    /// The [ObservableProperty] attribute generates:
    /// - A public property 'Message' with INotifyPropertyChanged support
    /// - The backing field must be named with lowercase first letter
    /// </summary>
    [ObservableProperty]
    private string _message = string.Empty;

    /// <summary>
    /// Indicates whether data is currently being loaded.
    /// Used to show loading state in the UI.
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Collection of products loaded from the API.
    /// ObservableCollection automatically notifies UI of changes.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    /// <summary>
    /// Error message to display when API call fails.
    /// </summary>
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of MainViewModel.
    /// IDataService is injected through the constructor by the DI container.
    /// </summary>
    /// <param name="dataService">The data service instance provided by DI.</param>
    public MainViewModel(IDataService dataService)
    {
        _dataService = dataService;
        
        // Initialize with welcome message from the service
        Message = _dataService.GetWelcomeMessage();
    }

    /// <summary>
    /// Command to load data asynchronously.
    /// The [RelayCommand] attribute generates:
    /// - A public IAsyncRelayCommand 'LoadDataCommand'
    /// - Automatic CanExecute handling based on IsLoading property
    /// </summary>
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        
        try
        {
            Message = await _dataService.LoadDataAsync();
        }
        catch (System.Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
            Message = "Failed to load data";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to load products from the API.
    /// </summary>
    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        Products.Clear();

        try
        {
            var products = await _dataService.GetProductsAsync();
            
            foreach (var product in products)
            {
                Products.Add(product);
            }

            Message = $"Successfully loaded {products.Count} products from API!";
        }
        catch (System.Exception ex)
        {
            ErrorMessage = $"Error loading products: {ex.Message}\n\nMake sure MyShopAPI is running on http://localhost:5002";
            Message = "Failed to load products";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
