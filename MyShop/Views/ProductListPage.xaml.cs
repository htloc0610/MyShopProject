using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
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

        // Load products when page loads (using paged endpoint)
        if (ViewModel.Products.Count == 0)
        {
            await ViewModel.LoadProductsPagedCommand.ExecuteAsync(null);
        }
    }
}
