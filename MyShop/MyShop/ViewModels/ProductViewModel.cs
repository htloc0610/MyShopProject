using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

/// <summary>
/// ViewModel for managing product list display with paging and sorting.
/// </summary>
public partial class ProductViewModel : ObservableObject
{
    private readonly IProductService _productService;

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

    #region Paging Properties

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
    private string _sortColumn = "id";

    [ObservableProperty]
    private bool _isDescending = false;

    [ObservableProperty]
    private int _selectedPageSize = 10;

    public ObservableCollection<int> PageSizeOptions { get; } = new() { 10, 20, 50 };

    public string PaginationInfo => $"Trang {CurrentPage} trên {TotalPages}";

    #endregion

    public ProductViewModel(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Loads products with paging and sorting.
    /// </summary>
    [RelayCommand]
    private async Task LoadProductsPagedAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var result = await _productService.GetProductsPagedAsync(
                CurrentPage,
                PageSize,
                SortColumn,
                IsDescending);

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

    [RelayCommand]
    private async Task ChangePageSizeAsync(int newPageSize)
    {
        PageSize = newPageSize;
        CurrentPage = 1;
        await LoadProductsPagedAsync();
    }

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

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadProductsPagedAsync();
    }

    partial void OnSelectedPageSizeChanged(int value)
    {
        _ = ChangePageSizeAsync(value);
    }
}
