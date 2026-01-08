using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models.Discounts;
using MyShop.Services.Discounts;
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

    public DiscountViewModel(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    [ObservableProperty]
    private ObservableCollection<Discount> _discounts = new();

    [ObservableProperty]
    private Discount? _selectedDiscount;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
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
    private int _totalCount;

    public string PaginationInfo => $"Trang {CurrentPage} trên {TotalPages} ({TotalCount} mã giảm giá)";

    public ObservableCollection<int> PageSizeOptions { get; } = new() { 10, 20, 50 };

    /// <summary>
    /// Load discounts with pagination.
    /// </summary>
    [RelayCommand]
    private async Task LoadDiscountsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading discounts...";

            var result = await _discountService.GetDiscountsPagedAsync(CurrentPage, PageSize);
            
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

            StatusMessage = $"Loaded {Discounts.Count} discount(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading discounts: {ex.Message}";
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
    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private async Task GoToNextPageAsync()
    {
        CurrentPage++;
        await LoadDiscountsAsync();
    }

    private bool CanGoToNextPage()
    {
        return !IsLoading && TotalPages > 0 && CurrentPage < TotalPages;
    }

    /// <summary>
    /// Go to the previous page.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private async Task GoToPreviousPageAsync()
    {
        CurrentPage--;
        await LoadDiscountsAsync();
    }

    private bool CanGoToPreviousPage()
    {
        return !IsLoading && CurrentPage > 1;
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
                StatusMessage = "Code is required";
                return;
            }

            if (EditAmount <= 0)
            {
                StatusMessage = "Amount must be greater than 0";
                return;
            }

            if (EditEndDate <= EditStartDate)
            {
                StatusMessage = "End date must be after start date";
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
                    StatusMessage = "Discount updated successfully";
                    await LoadDiscountsAsync();
                }
                else
                {
                    StatusMessage = "Failed to update discount";
                }
            }
            else
            {
                // Create new
                var created = await _discountService.CreateDiscountAsync(discount);
                if (created != null)
                {
                    StatusMessage = "Discount created successfully";
                    await LoadDiscountsAsync();
                    ClearForm();
                }
                else
                {
                    StatusMessage = "Failed to create discount";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
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
            StatusMessage = "Deleting discount...";

            var success = await _discountService.DeleteDiscountAsync(discount.DiscountId);
            if (success)
            {
                Discounts.Remove(discount);
                StatusMessage = "Discount deleted successfully";
            }
            else
            {
                StatusMessage = "Failed to delete discount";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
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
                StatusMessage = discount.IsActive 
                    ? $"Discount '{discount.Code}' activated" 
                    : $"Discount '{discount.Code}' deactivated";
                
                // Refresh the list to update UI
                await LoadDiscountsAsync();
            }
            else
            {
                // Revert on failure
                discount.IsActive = !discount.IsActive;
                StatusMessage = "Failed to update discount status";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
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
