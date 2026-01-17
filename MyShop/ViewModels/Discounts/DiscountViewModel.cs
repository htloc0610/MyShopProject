using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models.Discounts;
using MyShop.Services.Discounts;
using MyShop.Services.Shared;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.ViewModels.Discounts;

/// <summary>
/// ViewModel for Discount Management page.
/// Handles CRUD operations for discount codes.
/// </summary>
public partial class DiscountViewModel : ObservableObject
{
    private readonly IDiscountService _discountService;
    private readonly IToastService _toastService;

    public DiscountViewModel(IDiscountService discountService, IToastService toastService)
    {
        _discountService = discountService;
        _toastService = toastService;
    }

    [ObservableProperty]
    private ObservableCollection<Discount> _discounts = new();

    [ObservableProperty]
    private Discount? _selectedDiscount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoToPreviousPage))]
    [NotifyPropertyChangedFor(nameof(CanGoToNextPage))]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // Form fields for Add/Edit
    [ObservableProperty]
    private string _editCode = string.Empty;

    [ObservableProperty]
    private string _editDescription = string.Empty;

    [ObservableProperty]
    private double _editAmount;

    [ObservableProperty]
    private DateTimeOffset _editStartDate = DateTimeOffset.Now;

    [ObservableProperty]
    private DateTimeOffset _editEndDate = DateTimeOffset.Now.AddMonths(1);

    [ObservableProperty]
    private double _editUsageLimit = double.NaN;

    [ObservableProperty]
    private bool _editIsActive = true;

    [ObservableProperty]
    private bool _isEditMode;

    // Pagination properties
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PaginationInfo))]
    [NotifyPropertyChangedFor(nameof(CanGoToPreviousPage))]
    [NotifyPropertyChangedFor(nameof(CanGoToNextPage))]
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PaginationInfo))]
    private int _pageSize = 10;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PaginationInfo))]
    [NotifyPropertyChangedFor(nameof(CanGoToPreviousPage))]
    [NotifyPropertyChangedFor(nameof(CanGoToNextPage))]
    private int _totalPages;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PaginationInfo))]
    private int _totalCount;

    public string PaginationInfo => $"Trang {CurrentPage}/{TotalPages} (Tổng {TotalCount} mã giảm giá)";

    public ObservableCollection<int> PageSizeOptions { get; } = new() { 5, 10, 20, 50, 100 };

    public bool CanGoToPreviousPage => TotalPages > 0 && CurrentPage > 1 && !IsLoading;

    public bool CanGoToNextPage => TotalPages > 0 && CurrentPage < TotalPages && !IsLoading;

    // Filter properties
    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private string _selectedStatus = "all";

    [ObservableProperty]
    private double _minAmount = 0;

    [ObservableProperty]
    private double _maxAmount = 0;

    [ObservableProperty]
    private DateTimeOffset? _filterStartDate;

    [ObservableProperty]
    private DateTimeOffset? _filterEndDate;

    [ObservableProperty]
    private bool _isFilterVisible = false;

    public ObservableCollection<StatusFilterOption> StatusOptions { get; } = new()
    {
        new StatusFilterOption { Value = "all", Display = "-- Tất cả --" },
        new StatusFilterOption { Value = "active", Display = "Đang hoạt động" },
        new StatusFilterOption { Value = "inactive", Display = "Đã tắt" }
    };

    /// <summary>
    /// Load discounts with pagination.
    /// </summary>
    [RelayCommand]
    private async Task LoadDiscountsAsync()
    {
        try
        {
            IsLoading = true;

            // Convert filter values
            double? minAmountFilter = MinAmount > 0 ? MinAmount : null;
            double? maxAmountFilter = MaxAmount > 0 ? MaxAmount : null;
            DateTime? startDateFilter = FilterStartDate?.UtcDateTime;
            DateTime? endDateFilter = FilterEndDate?.UtcDateTime;

            var result = await _discountService.GetDiscountsPagedAsync(
                CurrentPage, 
                PageSize,
                SearchKeyword,
                SelectedStatus,
                minAmountFilter,
                maxAmountFilter,
                startDateFilter,
                endDateFilter);
            
            Discounts.Clear();
            foreach (var discount in result.Items)
            {
                Discounts.Add(discount);
            }

            TotalCount = result.TotalCount;
            TotalPages = result.TotalPages;
            OnPropertyChanged(nameof(PaginationInfo));

            // Debug output
            System.Diagnostics.Debug.WriteLine($"Loaded page {CurrentPage}/{TotalPages}, {result.Items.Count} items, Total: {TotalCount}");
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"Lỗi khi tải mã giảm giá: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Error in LoadDiscountsAsync: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Go to the next page.
    /// </summary>
    [RelayCommand]
    private async Task GoToNextPageAsync()
    {
        CurrentPage++;
        await LoadDiscountsAsync();
    }

    /// <summary>
    /// Go to the previous page.
    /// </summary>
    [RelayCommand]
    private async Task GoToPreviousPageAsync()
    {
        CurrentPage--;
        await LoadDiscountsAsync();
    }

    /// <summary>
    /// Toggle filter visibility.
    /// </summary>
    [RelayCommand]
    private void ToggleFilter()
    {
        IsFilterVisible = !IsFilterVisible;
    }

    /// <summary>
    /// Apply filters and reload discounts.
    /// </summary>
    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        CurrentPage = 1;
        await LoadDiscountsAsync();
    }

    /// <summary>
    /// Clear all filters and reload discounts.
    /// </summary>
    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchKeyword = string.Empty;
        SelectedStatus = "all";
        MinAmount = 0;
        MaxAmount = 0;
        FilterStartDate = null;
        FilterEndDate = null;
        CurrentPage = 1;
        await LoadDiscountsAsync();
    }

    /// <summary>
    /// Prepare to add a new discount.
    /// </summary>
    [RelayCommand]
    private void PrepareAdd()
    {
        IsEditMode = false;
        SelectedDiscount = null;
        ClearForm();
    }

    /// <summary>
    /// Prepare to edit an existing discount.
    /// </summary>
    [RelayCommand]
    private void PrepareEdit(Discount? discount)
    {
        if (discount == null) return;

        IsEditMode = true;
        SelectedDiscount = discount;
        
        EditCode = discount.Code;
        EditDescription = discount.Description ?? string.Empty;
        EditAmount = discount.Amount;
        EditStartDate = new DateTimeOffset(discount.StartDate);
        EditEndDate = new DateTimeOffset(discount.EndDate);
        EditUsageLimit = discount.UsageLimit.HasValue ? discount.UsageLimit.Value : double.NaN;
        EditIsActive = discount.IsActive;
    }

    /// <summary>
    /// Save the discount (create or update).
    /// </summary>
    [RelayCommand]
    private async Task SaveDiscountAsync()
    {
        try
        {
            IsLoading = true;

            // Validation
            if (string.IsNullOrWhiteSpace(EditCode))
            {
                _toastService.ShowError("Mã giảm giá không được để trống");
                return;
            }

            if (EditAmount <= 0)
            {
                _toastService.ShowError("Số tiền giảm phải lớn hơn 0");
                return;
            }

            if (EditEndDate <= EditStartDate)
            {
                _toastService.ShowError("Ngày kết thúc phải sau ngày bắt đầu");
                return;
            }

            var discount = new Discount
            {
                Code = EditCode.Trim(),
                Description = string.IsNullOrWhiteSpace(EditDescription) ? null : EditDescription.Trim(),
                Amount = (int)EditAmount,
                StartDate = EditStartDate.UtcDateTime,
                EndDate = EditEndDate.UtcDateTime,
                UsageLimit = double.IsNaN(EditUsageLimit) ? null : (int)EditUsageLimit,
                IsActive = EditIsActive
            };

            if (IsEditMode && SelectedDiscount != null)
            {
                // Update existing
                discount.DiscountId = SelectedDiscount.DiscountId;
                discount.UsedCount = SelectedDiscount.UsedCount;
                discount.UserId = SelectedDiscount.UserId;

                var success = await _discountService.UpdateDiscountAsync(discount.DiscountId, discount);
                if (success)
                {
                    _toastService.ShowSuccess("Cập nhật mã giảm giá thành công");
                    await LoadDiscountsAsync();
                }
                else
                {
                    _toastService.ShowError("Không thể cập nhật mã giảm giá");
                }
            }
            else
            {
                // Create new
                var created = await _discountService.CreateDiscountAsync(discount);
                if (created != null)
                {
                    _toastService.ShowSuccess("Tạo mã giảm giá thành công");
                    await LoadDiscountsAsync();
                    ClearForm();
                }
                else
                {
                    _toastService.ShowError("Không thể tạo mã giảm giá");
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Delete a discount.
    /// </summary>
    [RelayCommand]
    private async Task DeleteDiscountAsync(Discount? discount)
    {
        if (discount == null) return;

        try
        {
            IsLoading = true;

            var success = await _discountService.DeleteDiscountAsync(discount.DiscountId);
            if (success)
            {
                Discounts.Remove(discount);
                _toastService.ShowSuccess("Xóa mã giảm giá thành công");
            }
            else
            {
                _toastService.ShowError("Không thể xóa mã giảm giá");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Toggle active/inactive status of a discount.
    /// </summary>
    [RelayCommand]
    private async Task ToggleActiveAsync(Discount? discount)
    {
        if (discount == null) return;

        try
        {
            IsLoading = true;
            
            // Toggle the status
            discount.IsActive = !discount.IsActive;
            
            var success = await _discountService.UpdateDiscountAsync(discount.DiscountId, discount);
            if (success)
            {
                _toastService.ShowSuccess(discount.IsActive 
                    ? $"Đã kích hoạt mã '{discount.Code}'" 
                    : $"Đã vô hiệu hóa mã '{discount.Code}'");
                
                // Refresh the list to update UI
                await LoadDiscountsAsync();
            }
            else
            {
                // Revert on failure
                discount.IsActive = !discount.IsActive;
                _toastService.ShowError("Không thể cập nhật trạng thái");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Clear the form fields.
    /// </summary>
    private void ClearForm()
    {
        EditCode = string.Empty;
        EditDescription = string.Empty;
        EditAmount = 0;
        EditStartDate = DateTimeOffset.Now;
        EditEndDate = DateTimeOffset.Now.AddMonths(1);
        EditUsageLimit = double.NaN;
        EditIsActive = true;
    }

    /// <summary>
    /// Handle page size change.
    /// </summary>
    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1; // Reset to first page
        _ = LoadDiscountsAsync();
    }
}
