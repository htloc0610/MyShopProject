using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Service for managing category data from API.
/// Handles HTTP communication with the MyShopAPI backend.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5002";
    private const string CategoriesEndpoint = "/api/categories";

    public CategoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    /// <summary>
    /// Gets all categories from the API.
    /// </summary>
    public async Task<List<Category>> GetCategoriesAsync()
    {
        try
        {
            var categories = await _httpClient.GetFromJsonAsync<List<Category>>(CategoriesEndpoint);
            return categories ?? new List<Category>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting categories: {ex.Message}");
            return new List<Category>();
        }
    }
}
