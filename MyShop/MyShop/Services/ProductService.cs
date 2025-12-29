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
    /// GET /api/products
    /// </summary>
    public async Task<List<Product>> GetProductsAsync()
    {
        try
        {
            var products = await _httpClient.GetFromJsonAsync<List<Product>>(ProductsEndpoint);
            return products ?? new List<Product>();
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Error getting products: {ex.Message}");
            return new List<Product>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting products: {ex.Message}");
            return new List<Product>();
        }
    }

    /// <summary>
    /// Gets a single product by ID.
    /// GET /api/products/{id}
    /// </summary>
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        try
        {
            var product = await _httpClient.GetFromJsonAsync<Product>($"{ProductsEndpoint}/{id}");
            return product;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Error getting product {id}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting product {id}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets products by category ID.
    /// GET /api/products/category/{categoryId}
    /// </summary>
    public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        try
        {
            var products = await _httpClient.GetFromJsonAsync<List<Product>>($"{ProductsEndpoint}/category/{categoryId}");
            return products ?? new List<Product>();
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Error getting products for category {categoryId}: {ex.Message}");
            return new List<Product>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting products for category {categoryId}: {ex.Message}");
            return new List<Product>();
        }
    }
}
