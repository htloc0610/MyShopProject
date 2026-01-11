using MyShop.Helpers.Converters;
using MyShop.Models.Reports;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyShop.Services.Reports
{
    public class ReportService
    {
        private readonly HttpClient _httpClient;

        public ReportService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new NullableDateOnlyJsonConverter(),
            }
        };

        public async Task<ProductSalesSummaryResponse?> GetProductSalesSummaryAsync(
            DateOnly? from,
            DateOnly to)
        {
            var url = BuildUrl(
                "api/reports/product-sales-summary",
                from,
                to);

            return await _httpClient.GetFromJsonAsync<ProductSalesSummaryResponse>(
                url,
                JsonOptions);
        }

        public async Task<SalesTimeSeriesResponse?> GetSalesTimeSeriesAsync(
            DateOnly? from,
            DateOnly to,
            string groupBy)
        {
            var url = BuildUrl(
                "api/reports/sales-quantity-series",
                from,
                to,
                groupBy);

            return await _httpClient.GetFromJsonAsync<SalesTimeSeriesResponse>(
                url,
                JsonOptions);
        }
        public async Task<ProductRevenueProfitSummaryResponse?> GetProductRevenueProfitSummaryAsync(
            DateOnly? from,
            DateOnly to)
        {
            var url = BuildUrl(
                "api/reports/product-revenue-profit-summary",
                from,
                to);

            return await _httpClient.GetFromJsonAsync<ProductRevenueProfitSummaryResponse>(
                url,
                JsonOptions);
        }

        public async Task<RevenueProfitTimeSeriesResponse?> GetRevenueProfitTimeSeriesAsync(
            DateOnly? from,
            DateOnly to,
            string groupBy)
        {
            var url = BuildUrl(
                "api/reports/revenue-profit-series",
                from,
                to,
                groupBy);

            return await _httpClient.GetFromJsonAsync<RevenueProfitTimeSeriesResponse>(
                url,
                JsonOptions);
        }

        private static string BuildUrl(
            string basePath,
            DateOnly? from,
            DateOnly to,
            string? groupBy = null)
        {
            var url = $"{basePath}?to={to:yyyy-MM-dd}";

            if (from.HasValue)
                url += $"&from={from.Value:yyyy-MM-dd}";
            if (!string.IsNullOrWhiteSpace(groupBy))
                url += $"&groupBy={groupBy}";

            return url;
        }
    }
}
