using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Models;
using MyShop.ViewModels;

namespace MyShop.Views;

/// <summary>
/// Page for displaying product list in DataGrid with paging and sorting.
/// Uses ProductViewModel for data management.
/// </summary>
public sealed partial class ProductListPage : Page
{
    /// <summary>
    /// Gets the ViewModel for this page.
    /// Injected via DI container.
    /// </summary>
    public ProductViewModel ViewModel { get; }

    public ProductListPage()
    {
        // Get ViewModel from DI container
        ViewModel = App.Current.Services.GetRequiredService<ProductViewModel>();
        
        this.InitializeComponent();
    }

    /// <summary>
    /// Called when page is navigated to.
    /// Automatically loads products with paging.
    /// </summary>
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Initialize ViewModel (load categories and products)
        // Reload products when coming back from detail page
        await ViewModel.InitializeAsync();
    }

    /// <summary>
    /// Handles double-click on DataGrid to show product details.
    /// </summary>
    private void ProductDataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (ViewModel.SelectedProduct != null)
        {
            NavigateToProductDetail(ViewModel.SelectedProduct);
        }
    }

    /// <summary>
    /// Handles View Detail button click.
    /// </summary>
    private void ViewDetailButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Product product)
        {
            NavigateToProductDetail(product);
        }
    }

    /// <summary>
    /// Navigates to product detail page.
    /// </summary>
    private void NavigateToProductDetail(Product product)
    {
        Frame.Navigate(typeof(ProductDetailPage), product);
    }

    /// <summary>
    /// Handles Delete button click with confirmation.
    /// </summary>
    private async void DeleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Product product)
        {
            // Show confirmation dialog
            var dialog = new ContentDialog
            {
                Title = "Xác nhận xóa sản phẩm",
                Content = $"Bạn có chắc chắn muốn xóa sản phẩm '{product.Name}' không?\n\nHành động này không thể hoàn tác.",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.DeleteProductCommand.ExecuteAsync(product);
            }
        }
    }
}
