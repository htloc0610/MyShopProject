using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models.Products;
using MyShop.Models.Categories;
using MyShop.Models.Shared;
using MyShop.Services.Products;
using MyShop.Services.Categories;
using MyShop.Services.Shared;
using MyShop.Contracts.Helpers;

namespace MyShop.ViewModels.Products;

/// <summary>
/// ViewModel for managing product list with paging, sorting, and filtering.
/// Follows MVVM pattern with CommunityToolkit.Mvvm.
/// </summary>
public partial class ProductViewModel : ObservableObject
{
    #region Fields

    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ProductChangeNotifier _productChangeNotifier;

    #endregion

    #region Observable Properties - Display State

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private int _productCount;

    // Cache for fuzzy search results (client-side paging)
    private List<Product> _fuzzySearchResults = new();

    #endregion

    #region Observable Properties - Paging

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
    private int _totalPages;

    [ObservableProperty]
    private int _selectedPageSize = 10;

    public ObservableCollection<int> PageSizeOptions { get; } = new() { 10, 20, 50 };

    public string PaginationInfo => $"Trang {CurrentPage} trên {TotalPages}";

    #endregion

    #region Observable Properties - Sorting

    [ObservableProperty]
    private string _sortColumn = "id";

    [ObservableProperty]
    private bool _isDescending = false;

    #endregion

    #region Observable Properties - Filtering

    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private int? _selectedCategoryId;

    [ObservableProperty]
    private double _minPrice = 0;

    [ObservableProperty]
    private double _maxPrice = 0;

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private bool _useFuzzySearch = true;

    [ObservableProperty]
    private int _fuzzyThreshold = 70;

    [ObservableProperty]
    private bool _isFilterVisible = true;

    [ObservableProperty]
    private bool _isGridView = false;



    [ObservableProperty]
    private ObservableCollection<string> _selectedImageUrls = new();

    [ObservableProperty]
    private ObservableCollection<System.IO.FileInfo> _selectedImageFiles = new();

    #endregion

    #region Constructor

    public ProductViewModel(IProductService productService, ICategoryService categoryService, ProductChangeNotifier productChangeNotifier)
    {
        _productService = productService;
        _categoryService = categoryService;
        _productChangeNotifier = productChangeNotifier;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initialize and load initial data.
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadProductsPagedAsync();
    }

    #endregion

    #region Commands - Data Loading

    /// <summary>
    /// Loads all categories for the filter dropdown.
    /// </summary>
    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _categoryService.GetCategoriesAsync();
            
            Categories.Clear();
            Categories.Add(new Category { CategoryId = 0, Name = "-- Tất cả --" });
            
            foreach (var category in categories)
            {
                Categories.Add(category);
            }

            if (Categories.Count > 0)
            {
                SelectedCategory = Categories[0];
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex}");
        }
    }

    /// <summary>
    /// Loads products with current paging, sorting, and filtering settings.
    /// </summary>
    [RelayCommand]
    private async Task LoadProductsPagedAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            // Convert 0 to null for optional price filters
            double? minPriceFilter = MinPrice > 0 ? MinPrice : null;
            double? maxPriceFilter = MaxPrice > 0 ? MaxPrice : null;

            // Check if fuzzy search is enabled and has keyword
            if (UseFuzzySearch && !string.IsNullOrWhiteSpace(SearchKeyword))
            {
                // Get ALL products with filters from backend
                var allProducts = await _productService.GetAllProductsAsync(
                    SelectedCategoryId,
                    minPriceFilter,
                    maxPriceFilter);

                // Apply fuzzy search IN PLUGIN using FuzzySearchHelper
                _fuzzySearchResults = allProducts
                    .Where(p => FuzzySearchHelper.IsMatch(SearchKeyword, p.Name, FuzzyThreshold))
                    .ToList();

                // Apply client-side paging
                var pagedResults = _fuzzySearchResults
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                Products.Clear();
                foreach (var product in pagedResults)
                {
                    Products.Add(product);
                }

                ProductCount = _fuzzySearchResults.Count;
                TotalPages = (int)Math.Ceiling((double)ProductCount / PageSize);
                OnPropertyChanged(nameof(PaginationInfo));

                System.Diagnostics.Debug.WriteLine($"🎯 Plugin fuzzy search: '{SearchKeyword}' found {ProductCount} products (threshold: {FuzzyThreshold})");
            }
            else
            {
                // Use normal server-side paging API
                var result = await _productService.GetProductsPagedAsync(
                    CurrentPage,
                    PageSize,
                    SortColumn,
                    IsDescending,
                    SearchKeyword,
                    SelectedCategoryId,
                    minPriceFilter,
                    maxPriceFilter);

                Products.Clear();
                foreach (var product in result.Items)
                {
                    Products.Add(product);
                }

                ProductCount = result.TotalCount;
                TotalPages = result.TotalPages;
                OnPropertyChanged(nameof(PaginationInfo));
            }

            if (ProductCount == 0)
            {
                HasError = true;
                ErrorMessage = "Không tìm thấy sản phẩm nào.";
            }
        }
        catch (System.Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Lỗi khi tải sản phẩm: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error in LoadProductsPagedAsync: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Refreshes the product list with current settings.
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadProductsPagedAsync();
    }

    #endregion

    #region Commands - Filtering

    /// <summary>
    /// Applies current filters and reloads products from page 1.
    /// </summary>
    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        SelectedCategoryId = SelectedCategory?.CategoryId > 0 
            ? SelectedCategory.CategoryId 
            : null;

        CurrentPage = 1;
        await LoadProductsPagedAsync();
    }

    /// <summary>
    /// Clears all filters and reloads products.
    /// </summary>
    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchKeyword = string.Empty;
        SelectedCategory = Categories.FirstOrDefault();
        SelectedCategoryId = null;
        MinPrice = 0;
        MaxPrice = 0;
        CurrentPage = 1;
        await LoadProductsPagedAsync();
    }

    #endregion

    #region Commands - Sorting

    /// <summary>
    /// Selects a sort column without changing direction.
    /// </summary>
    [RelayCommand]
    private async Task SelectSortColumnAsync(string columnName)
    {
        if (!SortColumn.Equals(columnName, System.StringComparison.OrdinalIgnoreCase))
        {
            SortColumn = columnName;
            // Keep existing direction or default to ascending
            CurrentPage = 1;
            await LoadProductsPagedAsync();
        }
    }

    /// <summary>
    /// Toggles sort direction (ASC/DESC) for the current column.
    /// </summary>
    [RelayCommand]
    private async Task ToggleSortDirectionAsync()
    {
        IsDescending = !IsDescending;
        CurrentPage = 1;
        await LoadProductsPagedAsync();
    }

    /// <summary>
    /// Changes sort column and direction (legacy method for compatibility).
    /// </summary>
    [RelayCommand]
    private async Task ChangeSortAsync(string columnName)
    {
        if (SortColumn.Equals(columnName, System.StringComparison.OrdinalIgnoreCase))
        {
            IsDescending = !IsDescending;
        }
        else
        {
            SortColumn = columnName;
            IsDescending = false;
        }

        CurrentPage = 1;
        await LoadProductsPagedAsync();
    }

    #endregion

    #region Commands - Paging

    /// <summary>
    /// Changes page size and resets to page 1.
    /// </summary>
    [RelayCommand]
    private async Task ChangePageSizeAsync(int newPageSize)
    {
        PageSize = newPageSize;
        CurrentPage = 1;
        await LoadProductsPagedAsync();
    }

    /// <summary>
    /// Goes to the next page.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private async Task GoToNextPageAsync()
    {
        CurrentPage++;
        await LoadProductsPagedAsync();
    }

    private bool CanGoToNextPage()
    {
        return TotalPages > 0 && CurrentPage < TotalPages && !IsLoading;
    }

    /// <summary>
    /// Goes to the previous page.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private async Task GoToPreviousPageAsync()
    {
        CurrentPage--;
        await LoadProductsPagedAsync();
    }

    private bool CanGoToPreviousPage()
    {
        return TotalPages > 0 && CurrentPage > 1 && !IsLoading;
    }

    #endregion

    #region Commands - Product Actions

    /// <summary>
    /// Creates a new product.
    /// </summary>
    [RelayCommand]
    private async Task CreateProductAsync(Product product)
    {
        if (product == null) return;

        // Assign images from selected list
        product.Images = SelectedImageUrls.ToList();

        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var createdProduct = await _productService.CreateProductAsync(product);

            if (createdProduct != null)
            {
                // Notify that products have changed
                _productChangeNotifier.NotifyProductsChanged();
                
                // Reload to show the new product
                CurrentPage = 1; // Go to first page to see the new product
                await LoadProductsPagedAsync();
            }
            else
            {
                HasError = true;
                ErrorMessage = "Không thể tạo sản phẩm. Vui lòng thử lại.";
            }
        }
        catch (System.Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Lỗi khi tạo sản phẩm: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error in CreateProductAsync: {ex}");
            throw; // Re-throw to let the UI handle it
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    [RelayCommand]
    private async Task UpdateProductAsync(Product product)
    {
        if (product == null) return;

        try
        {
            IsLoading = true;

            // Create update DTO
            var updateDto = new ProductUpdateDto
            {
                ProductId = product.Id,
                Sku = product.Sku,
                Name = product.Name,
                ImportPrice = product.Price,
                Count = product.Stock,
                Description = product.Description,
                CategoryId = product.CategoryId,
                Images = SelectedImageUrls.ToList()
            };

            var updatedProduct = await _productService.UpdateProductAsync(product.Id, updateDto);

            if (updatedProduct != null)
            {
                // Update the product in the collection
                var index = Products.IndexOf(product);
                if (index >= 0)
                {
                    Products[index] = updatedProduct;
                }
                
                // Reload to get fresh data with correct category name
                await LoadProductsPagedAsync();
                
                // Notify that products have changed (in case count changed)
                _productChangeNotifier.NotifyProductsChanged();
            }
            else
            {
                HasError = true;
                ErrorMessage = "Không thể cập nhật sản phẩm. Vui lòng thử lại.";
            }
        }
        catch (System.Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Lỗi khi cập nhật sản phẩm: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error in UpdateProductAsync: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Deletes the selected product after confirmation.
    /// </summary>
    [RelayCommand]
    private async Task DeleteProductAsync(Product product)
    {
        if (product == null) return;

        try
        {
            IsLoading = true;
            
            var success = await _productService.DeleteProductAsync(product.Id);
            
            if (success)
            {
                // Remove from collection to update UI immediately
                Products.Remove(product);
                ProductCount--;
                OnPropertyChanged(nameof(PaginationInfo));
                
                // Notify that products have changed
                _productChangeNotifier.NotifyProductsChanged();
                
                // If current page is empty and not the first page, go back one page
                if (Products.Count == 0 && CurrentPage > 1)
                {
                    CurrentPage--;
                    await LoadProductsPagedAsync();
                }
            }
            else
            {
                HasError = true;
                ErrorMessage = "Không thể xóa sản phẩm. Vui lòng thử lại.";
            }
        }
        catch (System.Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Lỗi khi xóa sản phẩm: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error in DeleteProductAsync: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    #region Commands - Image Management

    [RelayCommand]
    private async Task PickImagesAsync()
    {
        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            
            // Get the current window handle (HWND)
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, windowHandle);

            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var files = await picker.PickMultipleFilesAsync();
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    // Limit to 3 images total
                    if (SelectedImageUrls.Count >= 3)
                    {
                        ErrorMessage = "Chỉ được phép tải lên tối đa 3 ảnh.";
                        HasError = true;
                        break;
                    }

                    // Upload immediately to get URL (simplest approach for now)
                    using var stream = await file.OpenStreamForReadAsync();
                    var imageUrl = await _productService.UploadImageAsync(stream, file.Name);
                    
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        SelectedImageUrls.Add(imageUrl);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi chọn ảnh: {ex.Message}";
            HasError = true;
        }
    }

    [RelayCommand]
    private void RemoveImage(string imageUrl)
    {
        if (SelectedImageUrls.Contains(imageUrl))
        {
            SelectedImageUrls.Remove(imageUrl);
        }
    }

    #endregion

    #endregion

    #region Property Changed Handlers

    partial void OnSelectedPageSizeChanged(int value)
    {
        _ = ChangePageSizeAsync(value);
    }

    #endregion
}
