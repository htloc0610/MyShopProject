using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models.Orders;
using MyShop.Services.Orders;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.ViewModels.Orders;

/// <summary>
/// ViewModel for the Orders List page.
/// </summary>
public partial class OrdersListViewModel : ObservableObject
{
    private readonly IOrderService _orderService;
    private readonly Frame? _navigationFrame;

    [ObservableProperty]
    private ObservableCollection<OrderListItem> _orders = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoToPreviousPage))]
    [NotifyPropertyChangedFor(nameof(CanGoToNextPage))]
    [NotifyPropertyChangedFor(nameof(PageInfo))]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1; // Reset to first page
        if (!IsLoading)
        {
            _ = LoadOrdersAsync();
        }
    }

    public ObservableCollection<int> AvailablePageSizes { get; } = new() { 5, 10, 20, 50, 100 };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoToPreviousPage))]
    [NotifyPropertyChangedFor(nameof(CanGoToNextPage))]
    [NotifyPropertyChangedFor(nameof(PageInfo))]
    private int _totalPages = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageInfo))]
    private int _totalCount = 0;

    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string? _errorMessage;

    // Filter properties
    [ObservableProperty]
    private string? _selectedStatus;

    [ObservableProperty]
    private DateTimeOffset? _startDate;

    [ObservableProperty]
    private DateTimeOffset? _endDate;

    [ObservableProperty]
    private string? _minAmountText;

    [ObservableProperty]
    private string? _maxAmountText;

    [ObservableProperty]
    private string _sortBy = "date";

    [ObservableProperty]
    private string _sortDirection = "desc";

    [ObservableProperty]
    private bool _isFilterExpanded = false;

    public ObservableCollection<string> AvailableStatuses { get; } = new()
    {
        "Tất cả",
        "New",
        "Processing",
        "Completed",
        "Cancelled"
    };

    public ObservableCollection<string> SortByOptions { get; } = new()
    {
        "Ngày đặt",
        "Giá trị",
        "Khách hàng",
        "Trạng thái"
    };

    [ObservableProperty]
    private string _selectedSortOption = "Ngày đặt";

    partial void OnSelectedSortOptionChanged(string value)
    {
        SortBy = value switch
        {
            "Ngày đặt" => "date",
            "Giá trị" => "amount",
            "Khách hàng" => "customer",
            "Trạng thái" => "status",
            _ => "date"
        };
    }

    public OrdersListViewModel(IOrderService orderService, Frame? navigationFrame = null)
    {
        _orderService = orderService;
        _navigationFrame = navigationFrame;
    }

    /// <summary>
    /// Set the navigation frame after construction.
    /// </summary>
    public void SetNavigationFrame(Frame? frame)
    {
        typeof(OrdersListViewModel).GetField("_navigationFrame", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(this, frame);
    }

    /// <summary>
    /// Initialize and load first page of orders.
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadOrdersAsync();
    }

    /// <summary>
    /// Load orders with current pagination, search, filters, and sorting settings.
    /// </summary>
    [RelayCommand]
    private async Task LoadOrdersAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            // Parse amount filters
            decimal? minAmount = null;
            decimal? maxAmount = null;
            
            if (!string.IsNullOrWhiteSpace(MinAmountText) && decimal.TryParse(MinAmountText, out var min))
            {
                minAmount = min;
            }
            
            if (!string.IsNullOrWhiteSpace(MaxAmountText) && decimal.TryParse(MaxAmountText, out var max))
            {
                maxAmount = max;
            }

            // Get status filter (convert "Tất cả" to null)
            var statusFilter = SelectedStatus == "Tất cả" || string.IsNullOrWhiteSpace(SelectedStatus) 
                ? null 
                : SelectedStatus;

            var result = await _orderService.GetOrdersAsync(
                page: CurrentPage,
                pageSize: PageSize,
                searchKeyword: SearchKeyword,
                status: statusFilter,
                startDate: StartDate?.DateTime,
                endDate: EndDate?.DateTime,
                minAmount: minAmount,
                maxAmount: maxAmount,
                sortBy: SortBy,
                sortDirection: SortDirection
            );

            if (result != null)
            {
                Orders.Clear();
                foreach (var order in result.Items)
                {
                    Orders.Add(order);
                }

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
            }
            else
            {
                ErrorMessage = "Không thể tải danh sách đơn hàng";
            }
        }
        catch (System.Exception ex)
        {
            ErrorMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Search orders by keyword.
    /// </summary>
    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1; // Reset to first page when searching
        await LoadOrdersAsync();
    }

    /// <summary>
    /// Go to next page.
    /// </summary>
    [RelayCommand]
    private async Task GoToNextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadOrdersAsync();
        }
    }

    /// <summary>
    /// Go to previous page.
    /// </summary>
    [RelayCommand]
    private async Task GoToPreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadOrdersAsync();
        }
    }

    /// <summary>
    /// Navigate to create new order page.
    /// </summary>
    [RelayCommand]
    private void CreateNewOrder()
    {
        if (_navigationFrame != null)
        {
            _navigationFrame.Navigate(typeof(Views.Orders.CreateOrderPage));
        }
    }

    /// <summary>
    /// Navigate to order detail page.
    /// </summary>
    [RelayCommand]
    private void ViewOrderDetail(OrderListItem order)
    {
        if (_navigationFrame != null && order != null)
        {
            _navigationFrame.Navigate(typeof(Views.Orders.OrderDetailPage), order.OrderId);
        }
    }

    /// <summary>
    /// Delete an order from the list.
    /// </summary>
    [RelayCommand]
    private async Task DeleteOrderAsync(OrderListItem order)
    {
        if (order == null) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var success = await _orderService.DeleteOrderAsync(order.OrderId);

            if (success)
            {
                // Remove from list
                Orders.Remove(order);
                TotalCount--;
                
                // Refresh to update pagination
                await LoadOrdersAsync();
            }
            else
            {
                ErrorMessage = "Không thể xóa đơn hàng";
            }
        }
        catch (System.Exception ex)
        {
            ErrorMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Apply current filter settings and reload orders.
    /// </summary>
    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        CurrentPage = 1; // Reset to first page when applying filters
        await LoadOrdersAsync();
    }

    /// <summary>
    /// Clear all filter settings and reload orders.
    /// </summary>
    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SelectedStatus = "Tất cả";
        StartDate = null;
        EndDate = null;
        MinAmountText = null;
        MaxAmountText = null;
        SearchKeyword = string.Empty;
        
        CurrentPage = 1;
        await LoadOrdersAsync();
    }

    /// <summary>
    /// Toggle filter panel visibility.
    /// </summary>
    [RelayCommand]
    private void ToggleFilterPanel()
    {
        IsFilterExpanded = !IsFilterExpanded;
    }

    /// <summary>
    /// Toggle sort direction.
    /// </summary>
    [RelayCommand]
    private async Task ToggleSortDirectionAsync()
    {
        SortDirection = SortDirection == "asc" ? "desc" : "asc";
        await LoadOrdersAsync();
    }

    /// <summary>
    /// Change sort field and direction.
    /// </summary>
    [RelayCommand]
    private async Task ChangeSortAsync(string sortField)
    {
        // Toggle direction if clicking same field
        if (SortBy == sortField)
        {
            SortDirection = SortDirection == "asc" ? "desc" : "asc";
        }
        else
        {
            SortBy = sortField;
            SortDirection = "desc"; // Default to descending for new field
        }
        
        await LoadOrdersAsync();
    }

    // Computed properties
    public bool CanGoToPreviousPage => CurrentPage > 1;
    public bool CanGoToNextPage => CurrentPage < TotalPages;
    public string PageInfo => TotalCount > 0 ? $"Trang {CurrentPage}/{TotalPages} (Tổng {TotalCount} đơn)" : "Không có đơn hàng";
}
