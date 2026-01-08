using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MyShop.Models.Reports;
using MyShop.Services.Reports;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.ViewModels.Reports
{
    public partial class ReportViewModel : ObservableObject
    {
        private readonly ReportService _reportService;

        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string errorMessage = string.Empty;

        [ObservableProperty] private DateOnly? fromDate = new(2000, 1, 1);
        [ObservableProperty] private DateOnly toDate = DateOnly.FromDateTime(DateTime.Today);

        public ObservableCollection<ProductSalesSummaryItem> ProductSales { get; } = new();
        public ObservableCollection<ProductRevenueProfitSummaryItem> ProductRevenueProfit { get; } = new();

        // ================= LINE CHART =================
        public ObservableCollection<ISeries> ProductSalesSeries { get; } = new();
        public ObservableCollection<ICartesianAxis> ProductSalesXAxes { get; } = new();
        public ObservableCollection<ICartesianAxis> ProductSalesYAxes { get; } = new();

        // ================= BAR CHART (REVENUE + PROFIT) =================
        public ObservableCollection<ISeries> RevenueProfitSeries { get; } = new();
        public ObservableCollection<ICartesianAxis> RevenueProfitXAxes { get; } = new();
        public ObservableCollection<ICartesianAxis> RevenueProfitYAxes { get; } = new();

        public ReportViewModel(ReportService reportService)
        {
            _reportService = reportService;
        }

        // ---------- LINE: SỐ LƯỢNG BÁN ----------
        private void BuildProductSalesLine()
        {
            ProductSalesSeries.Clear();
            ProductSalesXAxes.Clear();
            ProductSalesYAxes.Clear();

            if (ProductSales.Count == 0)
                return;

            ProductSalesSeries.Add(
                new LineSeries<double>
                {
                    Values = ProductSales
                        .Select(x => (double)x.TotalQuantity)
                        .ToArray(),
                    GeometrySize = 8,
                    Stroke = new SolidColorPaint(SKColors.DodgerBlue, 2),
                    Fill = null
                }
            );

            ProductSalesXAxes.Add(
                new Axis
                {
                    Labels = ProductSales
                        .Select(x => x.ProductName)
                        .ToArray(),
                    LabelsRotation = 15
                }
            );

            ProductSalesYAxes.Add(
                new Axis { MinLimit = 0 }
            );
        }

        // ---------- BAR: DOANH THU + LỢI NHUẬN ----------
        private void BuildRevenueProfitBars()
        {
            RevenueProfitSeries.Clear();
            RevenueProfitXAxes.Clear();
            RevenueProfitYAxes.Clear();

            if (ProductRevenueProfit.Count == 0)
                return;

            var revenueValues = ProductRevenueProfit
                .Select(x => (double)x.Revenue)
                .ToArray();

            var profitValues = ProductRevenueProfit
                .Select(x => (double)x.Profit)
                .ToArray();

            var labels = ProductRevenueProfit
                .Select(x => x.ProductName)
                .ToArray();

            double max = Math.Max(
                revenueValues.DefaultIfEmpty(0).Max(),
                profitValues.Select(Math.Abs).DefaultIfEmpty(0).Max()
            );

            // Doanh thu
            RevenueProfitSeries.Add(
                new ColumnSeries<double>
                {
                    Name = "Doanh thu",
                    Values = revenueValues,
                    Fill = new SolidColorPaint(SKColors.DodgerBlue),
                    Stroke = null
                }
            );

            // Lợi nhuận
            RevenueProfitSeries.Add(
                new ColumnSeries<double>
                {
                    Name = "Lợi nhuận",
                    Values = profitValues,
                    Fill = new SolidColorPaint(SKColors.MediumSeaGreen),
                    Stroke = null
                }
            );

            RevenueProfitXAxes.Add(
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 15
                }
            );

            RevenueProfitYAxes.Add(
                new Axis
                {
                    MinLimit = -max,
                    MaxLimit = max
                }
            );
        }

        // ---------- LOAD ----------
        [RelayCommand]
        private async Task LoadReportAsync()
        {
            IsLoading = true;
            ErrorMessage = "";

            try
            {
                ProductSales.Clear();
                ProductRevenueProfit.Clear();

                var sales =
                    await _reportService.GetProductSalesSummaryAsync(fromDate, toDate);
                if (sales != null)
                    foreach (var i in sales.Items)
                        ProductSales.Add(i);

                var profits =
                    await _reportService.GetProductRevenueProfitSummaryAsync(fromDate, toDate);
                if (profits != null)
                    foreach (var i in profits.Items)
                        ProductRevenueProfit.Add(i);

                BuildProductSalesLine();
                BuildRevenueProfitBars();
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
}
