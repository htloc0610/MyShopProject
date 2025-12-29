using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

/// <summary>
/// ViewModel for managing product list display.
/// Uses MVVM Toolkit for property change notifications and commands.
/// </summary>
public partial class ProductViewModel : ObservableObject
{
    private readonly IProductService _productService;

    /// <summary>
    /// Collection of products to display in the UI.
    /// ObservableCollection automatically notifies UI of changes.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    /// <summary>
    /// Indicates whether data is currently being loaded.
    /// Bound to ProgressRing visibility.
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Error message to display to user if loading fails.
    /// </summary>
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Indicates whether an error occurred.
    /// </summary>
    [ObservableProperty]
    private bool _hasError;

    /// <summary>
    /// Total number of products loaded.
    /// </summary>
    [ObservableProperty]
    private int _productCount;

    public ProductViewModel(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Loads all products from the API.
    /// Command can be invoked from XAML button or automatically on page load.
    /// </summary>
    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            // Call API to get products
            var products = await _productService.GetProductsAsync();

            // Clear existing products
            Products.Clear();

            // Add products to observable collection
            // This approach avoids "CollectionView does not support changes" error
            foreach (var product in products)
            {
                Products.Add(product);
            }

            ProductCount = Products.Count;

            // Show error if no products found
            if (ProductCount == 0)
            {
                HasError = true;
                ErrorMessage = "Không tìm th?y s?n ph?m nào. Vui lòng ki?m tra k?t n?i API.";
            }
        }
        catch (System.Exception ex)
        {
            HasError = true;
            ErrorMessage = $"L?i khi t?i s?n ph?m: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error in LoadProductsAsync: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads products for a specific category.
    /// </summary>
    [RelayCommand]
    private async Task LoadProductsByCategoryAsync(int categoryId)
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var products = await _productService.GetProductsByCategoryAsync(categoryId);

            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }

            ProductCount = Products.Count;

            if (ProductCount == 0)
            {
                HasError = true;
                ErrorMessage = $"Không tìm th?y s?n ph?m nào trong danh m?c này.";
            }
        }
        catch (System.Exception ex)
        {
            HasError = true;
            ErrorMessage = $"L?i khi t?i s?n ph?m: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error in LoadProductsByCategoryAsync: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Refreshes the product list.
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadProductsAsync();
    }
}
