using MyShop.Models.Dashboard;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MyShop.Services.Dashboard;

/// <summary>
/// Service for managing dashboard data from API.
/// </summary>
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
        return await _httpClient.GetFromJsonAsync<DashboardSummary>("api/dashboard/summary");
    }

    public async Task<List<LowStockProduct>> GetLowStockProductsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<LowStockProduct>>("api/dashboard/low-stock")
            ?? new List<LowStockProduct>();
    }

    public async Task<List<TopSellingProduct>> GetTopSellingProductsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<TopSellingProduct>>("api/dashboard/top-selling")
            ?? new List<TopSellingProduct>();
    }

    public async Task<List<RecentOrder>> GetRecentOrdersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<RecentOrder>>("api/dashboard/recent-orders")
            ?? new List<RecentOrder>();
    }

    public async Task<List<RevenueByDay>> GetMonthlyRevenueAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<RevenueByDay>>("api/dashboard/revenue-month")
            ?? new List<RevenueByDay>();
    }
}
