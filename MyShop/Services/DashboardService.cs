using MyShop.Models.Dashboard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MyShop.Services;

public class DashboardService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5002";

    public DashboardService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<DashboardSummary?> GetSummaryAsync()
    {
        var rs = await _httpClient.GetFromJsonAsync<DashboardSummary>("api/dashboard/summary");
        return rs;
    }

    public async Task<List<LowStockProduct>> GetLowStockProductsAsync()
        => await _httpClient.GetFromJsonAsync<List<LowStockProduct>>(
               "api/dashboard/low-stock") ?? new();

    public async Task<List<TopSellingProduct>> GetTopSellingProductsAsync()
        => await _httpClient.GetFromJsonAsync<List<TopSellingProduct>>(
               "api/dashboard/top-selling") ?? new();

    public async Task<List<RecentOrder>> GetRecentOrdersAsync()
        => await _httpClient.GetFromJsonAsync<List<RecentOrder>>(
               "api/dashboard/recent-orders") ?? new();

    public async Task<List<RevenueByDay>> GetMonthlyRevenueAsync()
    {
        var rs = await _httpClient.GetFromJsonAsync<List<RevenueByDay>>(
               "api/dashboard/revenue-month") ?? new();
        return rs;
    }
}
