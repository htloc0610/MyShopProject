using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

/// <summary>
/// ViewModel for managing product list with paging, sorting, and filtering.
/// Follows MVVM pattern with CommunityToolkit.Mvvm.
/// </summary>
public partial class ProductViewModel : ObservableObject
{
    #region Fields

    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    #endregion

    #region Observable Properties - Display State

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

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

    #endregion

    #region Constructor

    public ProductViewModel(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
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
    /// Changes sort column and direction.
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

    #region Property Changed Handlers

    partial void OnSelectedPageSizeChanged(int value)
    {
        _ = ChangePageSizeAsync(value);
    }

    #endregion
}
