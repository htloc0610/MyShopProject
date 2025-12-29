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
    /// Gets all products from the API.
    /// </summary>
    public async Task<List<Product>> GetProductsAsync()
    {
        try
        {
            var products = await _httpClient.GetFromJsonAsync<List<Product>>(ProductsEndpoint);
            return products ?? new List<Product>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting products: {ex.Message}");
            return new List<Product>();
        }
    }

    /// <summary>
    /// Gets products with paging and sorting support.
    /// </summary>
    public async Task<PagedResult<Product>> GetProductsPagedAsync(
        int page,
        int pageSize,
        string? sortBy = null,
        bool isDescending = false)
    {
        try
        {
            // Build query string
            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}",
                $"isDescending={isDescending.ToString().ToLower()}"
            };

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                queryParams.Add($"sortBy={sortBy}");
            }

            var queryString = string.Join("&", queryParams);
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

    /// <summary>
    /// Gets products by category ID.
    /// </summary>
    public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        try
        {
            var products = await _httpClient.GetFromJsonAsync<List<Product>>($"{ProductsEndpoint}/category/{categoryId}");
            return products ?? new List<Product>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting products for category {categoryId}: {ex.Message}");
            return new List<Product>();
        }
    }
}
