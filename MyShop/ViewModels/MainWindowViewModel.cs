using CommunityToolkit.Mvvm.ComponentModel;
using MyShop.Services;
using System.Threading.Tasks;

namespace MyShop.ViewModels;

/// <summary>
/// ViewModel for MainWindow to manage navigation and global state.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly DashboardService _dashboardService;
    private readonly ProductChangeNotifier _productChangeNotifier;

    [ObservableProperty]
    private int totalProducts;

    public MainWindowViewModel(DashboardService dashboardService, ProductChangeNotifier productChangeNotifier)
    {
        _dashboardService = dashboardService;
        _productChangeNotifier = productChangeNotifier;

        // Subscribe to product changes
        _productChangeNotifier.ProductsChanged += async (s, e) => await LoadProductCountAsync();
    }

    /// <summary>
    /// Load the total product count from the API.
    /// </summary>
    public async Task LoadProductCountAsync()
    {
        try
        {
            var summary = await _dashboardService.GetSummaryAsync();
            if (summary != null)
            {
                TotalProducts = summary.TotalProducts;
            }
        }
        catch
        {
            // If API call fails, set to 0
            TotalProducts = 0;
        }
    }
}
