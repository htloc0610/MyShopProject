using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using MyShop.Models.Dashboard;
using MyShop.Services.Dashboard;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.ViewModels.Dashboard;

/// <summary>
/// ViewModel for managing dashboard data display.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly DashboardService _dashboardService;
    private static readonly SolidColorPaint AxisTextPaint =
        new(new SKColor(107, 114, 128));
    private static readonly SolidColorPaint AxisSeparatorPaint =
        new(new SKColor(229, 231, 235)) { StrokeThickness = 1 };

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private int totalProducts;

    [ObservableProperty]
    private int todayOrders;

    [ObservableProperty]
    private string todayRevenue = string.Empty;

    [ObservableProperty]
    private ObservableCollection<LowStockProduct> lowStockProducts = new();

    [ObservableProperty]
    private ObservableCollection<TopSellingProduct> topSellingProducts = new();

    [ObservableProperty]
    private ObservableCollection<RecentOrder> recentOrders = new();

    [ObservableProperty]
    private ObservableCollection<RevenueByDay> monthlyRevenue = new();

    [ObservableProperty]
    private ISeries[] monthlyRevenueSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ICartesianAxis[] monthlyRevenueXAxes = Array.Empty<ICartesianAxis>();

    [ObservableProperty]
    private ICartesianAxis[] monthlyRevenueYAxes = Array.Empty<ICartesianAxis>();

    public DashboardViewModel(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            // Load summary
            var summary = await _dashboardService.GetSummaryAsync();
            if (summary != null)
            {
                TotalProducts = summary.TotalProducts;
                TodayOrders = summary.TodayOrders;
                TodayRevenue = summary.TodayRevenueFormatted;
            }

            // Load low stock products
            LowStockProducts.Clear();
            var lowStock = await _dashboardService.GetLowStockProductsAsync();
            foreach (var item in lowStock)
                LowStockProducts.Add(item);

            // Load top selling products
            TopSellingProducts.Clear();
            var topSelling = await _dashboardService.GetTopSellingProductsAsync();
            foreach (var item in topSelling)
                TopSellingProducts.Add(item);

            // Load recent orders
            RecentOrders.Clear();
            var orders = await _dashboardService.GetRecentOrdersAsync();
            foreach (var order in orders)
                RecentOrders.Add(order);

            // Load monthly revenue with bar height calculation
            MonthlyRevenue.Clear();
            var revenueData = await _dashboardService.GetMonthlyRevenueAsync();
            if (revenueData.Count > 0)
            {
                var maxRevenue = revenueData.Max(x => x.Revenue);
                const double maxBarHeight = 160;

                foreach (var item in revenueData)
                {   
                    item.BarHeight = maxRevenue == 0 ? 0 : (double)(item.Revenue / maxRevenue) * maxBarHeight;
                    MonthlyRevenue.Add(item);
                }
            }

            BuildMonthlyRevenueChart();

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

    private void BuildMonthlyRevenueChart()
    {
        if (MonthlyRevenue.Count == 0)
        {
            MonthlyRevenueSeries = Array.Empty<ISeries>();
            MonthlyRevenueXAxes = Array.Empty<ICartesianAxis>();
            MonthlyRevenueYAxes = Array.Empty<ICartesianAxis>();
            return;
        }

        var primary = new SKColor(37, 99, 235);
        var values = MonthlyRevenue
            .Select(x => (double)x.Revenue)
            .ToArray();
        var labels = MonthlyRevenue
            .Select(x => x.Day.ToString())
            .ToArray();

        MonthlyRevenueSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name = "Doanh thu",
                Values = values,
                Fill = new LinearGradientPaint(
                    new[] { primary.WithAlpha(220), primary.WithAlpha(120) },
                    new SKPoint(0, 0),
                    new SKPoint(0, 1)),
                Stroke = new SolidColorPaint(primary.WithAlpha(220), 1)
            }
        };

        MonthlyRevenueXAxes = new ICartesianAxis[]
        {
            new Axis
            {
                Labels = labels,
                TextSize = 12,
                LabelsPaint = AxisTextPaint,
                SeparatorsPaint = AxisSeparatorPaint,
                TicksPaint = AxisSeparatorPaint
            }
        };

        MonthlyRevenueYAxes = new ICartesianAxis[]
        {
            new Axis
            {
                MinLimit = 0,
                TextSize = 12,
                LabelsPaint = AxisTextPaint,
                SeparatorsPaint = AxisSeparatorPaint,
                TicksPaint = AxisSeparatorPaint
            }
        };
    }
}