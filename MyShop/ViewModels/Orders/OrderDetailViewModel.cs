using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models.Orders;
using MyShop.Services.Orders;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.ViewModels.Orders;

/// <summary>
/// ViewModel for the Order Detail page.
/// </summary>
public partial class OrderDetailViewModel : ObservableObject
{
    private readonly IOrderService _orderService;
    private readonly Frame? _navigationFrame;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    private OrderDetail? _currentOrder;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    // Edit mode properties
    [ObservableProperty]
    private bool _isEditMode = false;

    [ObservableProperty]
    private string _selectedStatus = "New";

    public ObservableCollection<string> AvailableStatuses { get; } = new()
    {
        "New",
        "Processing",
        "Completed",
        "Cancelled"
    };

    public OrderDetailViewModel(IOrderService orderService, Frame? navigationFrame = null)
    {
        _orderService = orderService;
        _navigationFrame = navigationFrame;
    }

    /// <summary>
    /// Load order details by ID.
    /// </summary>
    public async Task LoadOrderAsync(int orderId)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            CurrentOrder = await _orderService.GetOrderByIdAsync(orderId);

            if (CurrentOrder == null)
            {
                ErrorMessage = "Không tìm thấy đơn hàng";
            }
            else
            {
                // Initialize selected status from current order
                SelectedStatus = CurrentOrder.Status;
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
    /// Toggle edit mode on/off.
    /// </summary>
    [RelayCommand]
    private void ToggleEditMode()
    {
        if (CurrentOrder == null) return;

        IsEditMode = !IsEditMode;

        if (IsEditMode)
        {
            // Entering edit mode - copy current status
            SelectedStatus = CurrentOrder.Status;
        }
    }

    /// <summary>
    /// Save changes to the order.
    /// </summary>
    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        if (CurrentOrder == null) return;

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            // Only update status, keep existing customer info
            var updatedOrder = await _orderService.UpdateOrderAsync(
                CurrentOrder.OrderId,
                CurrentOrder.CustomerName,  // Keep existing
                CurrentOrder.CustomerPhone, // Keep existing
                CurrentOrder.CustomerAddress, // Keep existing
                SelectedStatus // Only this changes
            );

            if (updatedOrder != null)
            {
                CurrentOrder = updatedOrder;
                SelectedStatus = updatedOrder.Status; // Ensure status is synced
                IsEditMode = false;
                SuccessMessage = "Cập nhật trạng thái thành công";
            }
            else
            {
                ErrorMessage = "Không thể lưu thay đổi";
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
    /// Cancel editing and revert changes.
    /// </summary>
    [RelayCommand]
    private void CancelEdit()
    {
        if (CurrentOrder == null) return;

        // Revert to original status
        SelectedStatus = CurrentOrder.Status;

        IsEditMode = false;
    }

    /// <summary>
    /// Navigate back to orders list.
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        if (_navigationFrame != null && _navigationFrame.CanGoBack)
        {
            _navigationFrame.GoBack();
        }
    }

    /// <summary>
    /// Delete the current order.
    /// </summary>
    [RelayCommand]
    private async Task DeleteOrderAsync()
    {
        if (CurrentOrder == null) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var success = await _orderService.DeleteOrderAsync(CurrentOrder.OrderId);

            if (success)
            {
                // Navigate back to orders list
                GoBack();
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
    /// Determines if the order can be edited (not Completed or Cancelled).
    /// </summary>
    public bool CanEdit => CurrentOrder != null && 
                           CurrentOrder.Status != "Completed" && 
                           CurrentOrder.Status != "Cancelled";
}
