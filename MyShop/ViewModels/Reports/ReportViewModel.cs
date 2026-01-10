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
        private static readonly SolidColorPaint AxisTextPaint =
            new(new SKColor(107, 114, 128));
        private static readonly SolidColorPaint AxisSeparatorPaint =
            new(new SKColor(229, 231, 235)) { StrokeThickness = 1 };

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
        [ObservableProperty] private double productSalesChartWidth = 700;

        // ================= BAR CHART (REVENUE + PROFIT) =================
        public ObservableCollection<ISeries> RevenueProfitSeries { get; } = new();
        public ObservableCollection<ICartesianAxis> RevenueProfitXAxes { get; } = new();
        public ObservableCollection<ICartesianAxis> RevenueProfitYAxes { get; } = new();
        [ObservableProperty] private double revenueProfitChartWidth = 700;

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

            var primary = new SKColor(37, 99, 235);
            var values = ProductSales
                .Select(x => (double)x.TotalQuantity)
                .ToArray();

            var productLabels = ProductSales
                .Select(x => x.ProductName)
                .ToArray();
            ProductSalesChartWidth = CalculateChartWidth(productLabels.Length, 80, 700);

            ProductSalesSeries.Add(
                new LineSeries<double>
                {
                    Values = values,
                    GeometrySize = 10,
                    LineSmoothness = 0.6,
                    Stroke = new SolidColorPaint(primary, 3) { StrokeCap = SKStrokeCap.Round },
                    Fill = new LinearGradientPaint(
                        new[] { primary.WithAlpha(80), primary.WithAlpha(0) },
                        new SKPoint(0, 0),
                        new SKPoint(0, 1)),
                    GeometryStroke = new SolidColorPaint(primary, 3),
                    GeometryFill = new SolidColorPaint(SKColors.White)
                }
            );

            ProductSalesXAxes.Add(
                new Axis
                {
                    Labels = productLabels,
                    LabelsRotation = 0,
                    TextSize = 10,
                    LabelsPaint = AxisTextPaint,
                    SeparatorsPaint = AxisSeparatorPaint,
                    TicksPaint = AxisSeparatorPaint
                }
            );

            ProductSalesYAxes.Add(
                new Axis
                {
                    MinLimit = 0,
                    TextSize = 12,
                    LabelsPaint = AxisTextPaint,
                    SeparatorsPaint = AxisSeparatorPaint,
                    TicksPaint = AxisSeparatorPaint
                }
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
            RevenueProfitChartWidth = CalculateChartWidth(labels.Length, 90, 700);

            double max = Math.Max(
                revenueValues.DefaultIfEmpty(0).Max(),
                profitValues.Select(Math.Abs).DefaultIfEmpty(0).Max()
            );

            var revenueColor = new SKColor(37, 99, 235);
            var profitColor = new SKColor(16, 185, 129);

            // Doanh thu
            RevenueProfitSeries.Add(
                new ColumnSeries<double>
                {
                    Name = "Doanh thu",
                    Values = revenueValues,
                    Fill = new LinearGradientPaint(
                        new[] { revenueColor.WithAlpha(220), revenueColor.WithAlpha(120) },
                        new SKPoint(0, 0),
                        new SKPoint(0, 1)),
                    Stroke = new SolidColorPaint(revenueColor.WithAlpha(220), 1)
                }
            );

            // Lợi nhuận
            RevenueProfitSeries.Add(
                new ColumnSeries<double>
                {
                    Name = "Lợi nhuận",
                    Values = profitValues,
                    Fill = new LinearGradientPaint(
                        new[] { profitColor.WithAlpha(220), profitColor.WithAlpha(120) },
                        new SKPoint(0, 0),
                        new SKPoint(0, 1)),
                    Stroke = new SolidColorPaint(profitColor.WithAlpha(220), 1)
                }
            );

            RevenueProfitXAxes.Add(
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 0,
                    TextSize = 10,
                    LabelsPaint = AxisTextPaint,
                    SeparatorsPaint = AxisSeparatorPaint,
                    TicksPaint = AxisSeparatorPaint
                }
            );

            RevenueProfitYAxes.Add(
                new Axis
                {
                    MinLimit = -max,
                    MaxLimit = max,
                    TextSize = 12,
                    LabelsPaint = AxisTextPaint,
                    SeparatorsPaint = AxisSeparatorPaint,
                    TicksPaint = AxisSeparatorPaint
                }
            );
        }

        private static double CalculateChartWidth(int labelCount, double perLabel, double minWidth)
        {
            return Math.Max(minWidth, labelCount * perLabel);
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
