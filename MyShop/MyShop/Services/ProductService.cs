using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Service for managing product data from API.
/// Handles HTTP communication with the MyShopAPI backend.
/// </summary>
public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5002";
    private const string ProductsEndpoint = "/api/products";

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    /// <summary>
    /// Gets products with paging, sorting, and filtering support.
    /// </summary>
    public async Task<PagedResult<Product>> GetProductsPagedAsync(
        int page,
        int pageSize,
        string? sortBy = null,
        bool isDescending = false,
        string? keyword = null,
        int? categoryId = null,
        double? minPrice = null,
        double? maxPrice = null)
    {
        try
        {
            // Build query string with all parameters
            var queryString = BuildQueryString(page, pageSize, sortBy, isDescending, keyword, categoryId, minPrice, maxPrice);
            var url = $"{ProductsEndpoint}?{queryString}";

            var result = await _httpClient.GetFromJsonAsync<PagedResult<Product>>(url);
            
            return result ?? new PagedResult<Product>
            {
                Items = new List<Product>(),
                TotalCount = 0,
                CurrentPage = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting paged products: {ex.Message}");
            return new PagedResult<Product>
            {
                Items = new List<Product>(),
                TotalCount = 0,
                CurrentPage = page,
                PageSize = pageSize
            };
        }
    }

    /// <summary>
    /// Gets a single product by ID.
    /// </summary>
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Product>($"{ProductsEndpoint}/{id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting product {id}: {ex.Message}");
            return null;
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Builds query string for product filtering and paging.
    /// </summary>
    private string BuildQueryString(
        int page,
        int pageSize,
        string? sortBy,
        bool isDescending,
        string? keyword,
        int? categoryId,
        double? minPrice,
        double? maxPrice)
    {
        var queryParams = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}",
            $"isDescending={isDescending.ToString().ToLower()}"
        };

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            queryParams.Add($"keyword={Uri.EscapeDataString(keyword)}");
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            queryParams.Add($"categoryId={categoryId.Value}");
        }

        if (minPrice.HasValue && minPrice.Value >= 0)
        {
            queryParams.Add($"minPrice={minPrice.Value}");
        }

        if (maxPrice.HasValue && maxPrice.Value >= 0)
        {
            queryParams.Add($"maxPrice={maxPrice.Value}");
        }

        return string.Join("&", queryParams);
    }

    #endregion
}
