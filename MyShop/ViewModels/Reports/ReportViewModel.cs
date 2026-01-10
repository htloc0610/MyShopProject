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
        private bool _isReady;

        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string errorMessage = string.Empty;

        [ObservableProperty] private DateOnly? fromDate = new(2000, 1, 1);
        [ObservableProperty] private DateOnly toDate = DateOnly.FromDateTime(DateTime.Today);

        public ObservableCollection<SalesTimeSeriesItem> SalesTimeSeries { get; } = new();
        public ObservableCollection<RevenueProfitTimeSeriesItem> RevenueProfitTimeSeries { get; } = new();

        public ObservableCollection<ReportGroupByOption> GroupByOptions { get; } = new()
        {
            new ReportGroupByOption("day", "Theo ngày"),
            new ReportGroupByOption("week", "Theo tuần"),
            new ReportGroupByOption("month", "Theo tháng"),
            new ReportGroupByOption("year", "Theo năm")
        };

        [ObservableProperty] private ReportGroupByOption selectedGroupBy;

        // ================= LINE CHART =================
        [ObservableProperty] private ObservableCollection<ISeries> productSalesSeries = new();
        [ObservableProperty] private ObservableCollection<ICartesianAxis> productSalesXAxes = new();
        [ObservableProperty] private ObservableCollection<ICartesianAxis> productSalesYAxes = new();
        [ObservableProperty] private double productSalesChartWidth = 700;
        [ObservableProperty] private ObservableCollection<ReportViewModel> productSalesChartHosts = new();

        // ================= BAR CHART (REVENUE + PROFIT) =================
        [ObservableProperty] private ObservableCollection<ISeries> revenueProfitSeries = new();
        [ObservableProperty] private ObservableCollection<ICartesianAxis> revenueProfitXAxes = new();
        [ObservableProperty] private ObservableCollection<ICartesianAxis> revenueProfitYAxes = new();
        [ObservableProperty] private double revenueProfitChartWidth = 700;
        [ObservableProperty] private ObservableCollection<ReportViewModel> revenueProfitChartHosts = new();

        public ReportViewModel(ReportService reportService)
        {
            _reportService = reportService;
            SelectedGroupBy = GroupByOptions.First();
            _isReady = true;
        }

        partial void OnSelectedGroupByChanged(ReportGroupByOption value)
        {
            if (!_isReady)
                return;

            LoadReportCommand.Execute(null);
        }

        // ---------- LINE: SỐ LƯỢNG BÁN ----------
        private void BuildProductSalesLine()
        {
            var primary = new SKColor(37, 99, 235);
            var values = SalesTimeSeries
                .Select(x => (double)x.TotalQuantity)
                .ToArray();

            var labels = SalesTimeSeries
                .Select(x => x.Label)
                .ToArray();
            ProductSalesChartWidth = 700;

            ProductSalesSeries = new ObservableCollection<ISeries>
            {
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
            };

            ProductSalesXAxes = new ObservableCollection<ICartesianAxis>
            {
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 0,
                    TextSize = 10,
                    LabelsPaint = AxisTextPaint,
                    SeparatorsPaint = AxisSeparatorPaint,
                    TicksPaint = AxisSeparatorPaint
                }
            };

            ProductSalesYAxes = new ObservableCollection<ICartesianAxis>
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = values.Length == 0 ? 1 : values.Max(),
                    TextSize = 12,
                    LabelsPaint = AxisTextPaint,
                    SeparatorsPaint = AxisSeparatorPaint,
                    TicksPaint = AxisSeparatorPaint
                }
            };
        }

        // ---------- BAR: DOANH THU + LỢI NHUẬN ----------
        private void BuildRevenueProfitBars()
        {
            var revenueValues = RevenueProfitTimeSeries
                .Select(x => (double)x.Revenue)
                .ToArray();

            var profitValues = RevenueProfitTimeSeries
                .Select(x => (double)x.Profit)
                .ToArray();

            var labels = RevenueProfitTimeSeries
                .Select(x => x.Label)
                .ToArray();
            RevenueProfitChartWidth = 700;

            double max = Math.Max(
                revenueValues.DefaultIfEmpty(0).Max(),
                profitValues.Select(Math.Abs).DefaultIfEmpty(0).Max()
            );
            if (max <= 0)
                max = 1;

            var revenueColor = new SKColor(37, 99, 235);
            var profitColor = new SKColor(16, 185, 129);

            RevenueProfitSeries = new ObservableCollection<ISeries>
            {
                new ColumnSeries<double>
                {
                    Name = "Doanh thu",
                    Values = revenueValues,
                    Fill = new LinearGradientPaint(
                        new[] { revenueColor.WithAlpha(220), revenueColor.WithAlpha(120) },
                        new SKPoint(0, 0),
                        new SKPoint(0, 1)),
                    Stroke = new SolidColorPaint(revenueColor.WithAlpha(220), 1)
                },
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
            };

            RevenueProfitXAxes = new ObservableCollection<ICartesianAxis>
            {
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 0,
                    TextSize = 10,
                    LabelsPaint = AxisTextPaint,
                    SeparatorsPaint = AxisSeparatorPaint,
                    TicksPaint = AxisSeparatorPaint
                }
            };

            RevenueProfitYAxes = new ObservableCollection<ICartesianAxis>
            {
                new Axis
                {
                    MinLimit = -max,
                    MaxLimit = max,
                    TextSize = 12,
                    LabelsPaint = AxisTextPaint,
                    SeparatorsPaint = AxisSeparatorPaint,
                    TicksPaint = AxisSeparatorPaint
                }
            };
        }

        // ---------- LOAD ----------
        [RelayCommand]
        private async Task LoadReportAsync()
        {
            IsLoading = true;
            ErrorMessage = "";

            try
            {
                SalesTimeSeries.Clear();
                RevenueProfitTimeSeries.Clear();

                var groupBy = SelectedGroupBy?.Key ?? "day";
                var effectiveFrom = ResolveFromDate(groupBy, toDate, fromDate);

                var sales =
                    await _reportService.GetSalesTimeSeriesAsync(effectiveFrom, toDate, groupBy);
                if (sales != null)
                    foreach (var i in sales.Items)
                        SalesTimeSeries.Add(i);

                var profits =
                    await _reportService.GetRevenueProfitTimeSeriesAsync(effectiveFrom, toDate, groupBy);
                if (profits != null)
                    foreach (var i in profits.Items)
                        RevenueProfitTimeSeries.Add(i);

                BuildProductSalesLine();
                BuildRevenueProfitBars();
                ProductSalesChartHosts = new ObservableCollection<ReportViewModel> { this };
                RevenueProfitChartHosts = new ObservableCollection<ReportViewModel> { this };
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

        private static DateOnly? ResolveFromDate(string groupBy, DateOnly to, DateOnly? fallback)
        {
            switch (groupBy)
            {
                case "day":
                    return to.AddDays(-29);
                case "month":
                    return new DateOnly(to.Year, to.Month, 1).AddMonths(-11);
                case "year":
                    return new DateOnly(to.Year, 1, 1).AddYears(-9);
                default:
                    return fallback;
            }
        }
    }

    public class ReportGroupByOption
    {
        public string Key { get; }
        public string Label { get; }

        public ReportGroupByOption(string key, string label)
        {
            Key = key;
            Label = label;
        }

        public override string ToString() => Label;
    }
}
