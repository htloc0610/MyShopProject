using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models.Dashboard;
using MyShop.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly DashboardService _dashboardService;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    // ===== SUMMARY =====
    [ObservableProperty]
    private int totalProducts;

    [ObservableProperty]
    private int todayOrders;

    [ObservableProperty]
    private string todayRevenue = string.Empty;

    // ===== COLLECTIONS =====
    [ObservableProperty]
    private ObservableCollection<LowStockProduct> lowStockProducts = new();

    [ObservableProperty]
    private ObservableCollection<TopSellingProduct> topSellingProducts = new();

    [ObservableProperty]
    private ObservableCollection<RecentOrder> recentOrders = new();

    [ObservableProperty]
    private ObservableCollection<RevenueByDay> monthlyRevenue = new();


    public DashboardViewModel(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // GIỐNG LoadProductsAsync
    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            // SUMMARY
            var summary = await _dashboardService.GetSummaryAsync();
            if (summary != null)
            {
                TotalProducts = summary.TotalProducts;
                TodayOrders = summary.TodayOrders;
                TodayRevenue = summary.TodayRevenueFormatted;
            }

            // LOW STOCK
            LowStockProducts.Clear();
            var lowStock = await _dashboardService.GetLowStockProductsAsync();
            foreach (var item in lowStock)
                LowStockProducts.Add(item);

            // TOP SELLING
            TopSellingProducts.Clear();
            var topSelling = await _dashboardService.GetTopSellingProductsAsync();
            foreach (var item in topSelling)
                TopSellingProducts.Add(item);

            // RECENT ORDERS
            RecentOrders.Clear();
            var orders = await _dashboardService.GetRecentOrdersAsync();
            foreach (var order in orders)
                RecentOrders.Add(order);

            // MONTHLY REVENUE
            MonthlyRevenue.Clear();
            var revenueData = await _dashboardService.GetMonthlyRevenueAsync();
            if (revenueData.Count > 0)
            {
                var maxRevenue = revenueData.Max(x => x.Revenue);
                const double maxBarHeight = 160;

                foreach (var item in revenueData)
                {   
                    item.BarHeight = maxRevenue == 0
                        ? 0
                        : (double)(item.Revenue / maxRevenue) * maxBarHeight;

                    MonthlyRevenue.Add(item);
                }
            }

        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
