using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Implementation of IDataService.
/// This service handles data operations for the application.
/// 
/// DI Lifecycle Note:
/// - Registered as SINGLETON: Same instance shared across all requests.
///   Use for stateless services or when you need to maintain state.
/// - Could be TRANSIENT if a new instance is needed each time.
/// </summary>
public class DataService : IDataService
{
    private readonly HttpClient _httpClient;
    // Fixed: Remove /api from base URL since controller already has [Route("api/[controller]")]
    private const string BaseUrl = "http://localhost:5002";

    public DataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    /// <inheritdoc />
    public string GetWelcomeMessage()
    {
        return "Welcome to MyShop! This message comes from the DataService.";
    }

    /// <inheritdoc />
    public async Task<string> LoadDataAsync()
    {
        try
        {
            var products = await GetProductsAsync();
            return $"Loaded {products.Count} products successfully at {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            return $"Failed to load data: {ex.Message}";
        }
    }

    /// <inheritdoc />
    public async Task<List<Product>> GetProductsAsync()
    {
        try
        {
            // Now correctly calls: http://localhost:5002/api/products
            var products = await _httpClient.GetFromJsonAsync<List<Product>>("api/products");
            return products ?? new List<Product>();
        }
        catch (HttpRequestException ex)
        {
            // Log error (in production, use proper logging)
            Console.WriteLine($"Error fetching products: {ex.Message}");
            throw new Exception("Unable to connect to API. Make sure MyShopAPI is running.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Product>($"api/products/{id}");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching product {id}: {ex.Message}");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<Product?> CreateProductAsync(Product product)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/products", product);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error creating product: {ex.Message}");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<Product?> UpdateProductAsync(int id, Product product)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/products/{id}", product);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error updating product {id}: {ex.Message}");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/products/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error deleting product {id}: {ex.Message}");
            return false;
        }
    }
}
