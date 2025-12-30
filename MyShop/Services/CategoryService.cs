using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Service for managing category data from API.
/// Handles HTTP communication with the MyShopAPI backend.
/// Provides full CRUD operations for categories.
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

    /// <summary>
    /// Gets a category by ID.
    /// </summary>
    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Category>($"{CategoriesEndpoint}/{id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting category {id}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    public async Task<Category?> CreateCategoryAsync(string name, string description)
    {
        try
        {
            var createDto = new { Name = name, Description = description };
            var response = await _httpClient.PostAsJsonAsync(CategoriesEndpoint, createDto);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Category>();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Error creating category: {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating category: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    public async Task<Category?> UpdateCategoryAsync(int id, string name, string description)
    {
        try
        {
            var updateDto = new { CategoryId = id, Name = name, Description = description };
            var response = await _httpClient.PutAsJsonAsync($"{CategoriesEndpoint}/{id}", updateDto);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Category>();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Error updating category: {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating category: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Deletes a category.
    /// Returns error message if deletion fails (e.g., category has products).
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteCategoryAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{CategoriesEndpoint}/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    if (errorJson.TryGetProperty("message", out var messageElement))
                    {
                        return (false, messageElement.GetString());
                    }
                }
                catch
                {
                    // Fallback if JSON parsing fails
                }
                return (false, errorContent);
            }
            
            return (false, $"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting category: {ex.Message}");
            return (false, ex.Message);
        }
    }
}
