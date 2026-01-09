using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.ViewModels.Orders;
using MyShop.Services.Orders;
using MyShop.Models.Orders;

namespace MyShop.Views.Orders;

public sealed partial class OrdersListPage : Page
{
    public OrdersListViewModel ViewModel { get; }

    public OrdersListPage()
    {
        this.InitializeComponent();
        
        // Get service and initialize ViewModel (Frame will be set later)
        var orderService = App.Current.Services.GetService(typeof(IOrderService)) as IOrderService;
        ViewModel = new OrdersListViewModel(orderService!, null);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // Set the navigation frame now that the page is in the visual tree
        ViewModel.SetNavigationFrame(this.Frame);
        
        await ViewModel.InitializeAsync();
    }

    private void ViewDetailButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is OrderListItem order)
        {
            this.Frame.Navigate(typeof(OrderDetailPage), order.OrderId);
        }
    }

    private async void DeleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is OrderListItem order)
        {
            // Show confirmation dialog
            var confirmDialog = new ContentDialog
            {
                Title = "Xác nhận xóa đơn hàng",
                Content = $"Bạn có chắc chắn muốn xóa đơn hàng #{order.OrderId} của khách hàng '{order.CustomerName}' không?\n\nHành động này không thể hoàn tác.",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Execute delete command
                await ViewModel.DeleteOrderCommand.ExecuteAsync(order);
                
                // Determine result message
                string resultTitle;
                string resultContent;
                
                if (string.IsNullOrEmpty(ViewModel.ErrorMessage))
                {
                    resultTitle = "Thành công";
                    resultContent = $"Đã xóa đơn hàng #{order.OrderId} thành công!\n\nKho hàng đã được hoàn lại.";
                }
                else
                {
                    resultTitle = "Không thể xóa";
                    
                    // Check if error contains status info (from API)
                    if (ViewModel.ErrorMessage.Contains("Only 'New' orders can be deleted") || 
                        ViewModel.ErrorMessage.Contains("Cannot delete order with status"))
                    {
                        resultContent = $"Không thể xóa đơn hàng #{order.OrderId}\n\nLý do: Chỉ có thể xóa đơn hàng có trạng thái 'New'.\nĐơn hàng này đang ở trạng thái '{order.Status}'.";
                    }
                    else
                    {
                        resultContent = $"Không thể xóa đơn hàng #{order.OrderId}\n\nLỗi: {ViewModel.ErrorMessage}";
                    }
                }
                
                // Show result dialog
                var resultDialog = new ContentDialog
                {
                    Title = resultTitle,
                    Content = resultContent,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                
                await resultDialog.ShowAsync();
                
                // Clear error message after showing
                ViewModel.ErrorMessage = null;
            }
        }
    }
}
