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

    /// <summary>
    /// Handles DataGrid column header click for sorting.
    /// </summary>
    private async void OrdersDataGrid_Sorting(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridColumnEventArgs e)
    {
        // Get the tag (property name) from the column and map to API field name
        var columnTag = e.Column.Tag?.ToString();
        if (string.IsNullOrEmpty(columnTag))
            return;

        // Map column tag to API sort field
        var sortField = columnTag switch
        {
            "OrderId" => "id",
            "CustomerName" => "customer",
            "OrderDate" => "date",
            "FinalAmount" => "amount",
            _ => "date"
        };

        // Determine sort direction
        string sortDirection;
        if (e.Column.SortDirection == CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Ascending)
        {
            e.Column.SortDirection = CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Descending;
            sortDirection = "desc";
        }
        else
        {
            e.Column.SortDirection = CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Ascending;
            sortDirection = "asc";
        }

        // Clear sort direction on other columns
        foreach (var column in OrdersDataGrid.Columns)
        {
            if (column != e.Column)
                column.SortDirection = null;
        }

        // Update ViewModel and reload using ChangeSortCommand
        await ViewModel.ChangeSortCommand.ExecuteAsync(sortField);
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
                // Execute delete command - Toast handles success/error feedback
                await ViewModel.DeleteOrderCommand.ExecuteAsync(order);
            }
        }
    }
}
