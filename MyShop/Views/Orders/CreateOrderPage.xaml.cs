using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels.Orders;
using MyShop.Models.Orders;
using Microsoft.Extensions.DependencyInjection;

namespace MyShop.Views.Orders;

public sealed partial class CreateOrderPage : Page
{
    public CreateOrderViewModel ViewModel { get; }

    public CreateOrderPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<CreateOrderViewModel>();
        _ = ViewModel.InitializeAsync();
    }

    // Customer Search
    private async void CustomerSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = CustomerSearchTextBox.Text;
        if (!string.IsNullOrWhiteSpace(searchText) && searchText.Length >= 2)
        {
            await ViewModel.SearchCustomersCommand.ExecuteAsync(searchText);
        }
        else
        {
            ViewModel.CustomerSuggestions.Clear();
        }
    }

    private void CustomerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is Models.Customers.Customer customer)
        {
            ViewModel.SelectCustomerCommand.Execute(customer);
            CustomerListView.SelectedItem = null;
        }
    }

    // Product Search
    private async void ProductSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = ProductSearchTextBox.Text;
        if (!string.IsNullOrWhiteSpace(searchText) && searchText.Length >= 2)
        {
            await ViewModel.SearchProductsCommand.ExecuteAsync(searchText);
        }
        else
        {
            ViewModel.ProductSuggestions.Clear();
        }
    }

    private void ProductListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is Models.Products.Product product)
        {
            ViewModel.AddToCartCommand.Execute(product);
            ProductListView.SelectedItem = null;
            ProductSearchTextBox.Text = string.Empty;
        }
    }

    // Cart Management
    private void RemoveCartItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is CartItem item)
        {
            ViewModel.RemoveFromCartCommand.Execute(item);
        }
    }

    private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is CartItem item)
        {
            item.Quantity++;
        }
    }

    private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is CartItem item)
        {
            if (item.Quantity > 1)
            {
                item.Quantity--;
            }
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // Navigate back to Orders List
        if (this.Frame.CanGoBack)
        {
            this.Frame.GoBack();
        }
        else
        {
            this.Frame.Navigate(typeof(OrdersListPage));
        }
        ViewModel.ClearCartCommand.Execute(null);
        ViewModel.ClearCustomerCommand.Execute(null);
    }

    // Coupon RadioButton Selection
    private void CouponRadioButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton radioButton && radioButton.Tag is AvailableCoupon coupon)
        {
            ViewModel.SelectCouponCommand.Execute(coupon);
        }
    }
}
