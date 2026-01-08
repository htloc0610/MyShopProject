using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MyShop.Models.Discounts;

namespace MyShop.Services.Discounts;

/// <summary>
/// Service for managing discount codes via API.
/// </summary>
public interface IDiscountService
{
    Task<List<Discount>> GetAllDiscountsAsync();
    Task<DiscountPagedResult> GetDiscountsPagedAsync(int page, int pageSize);
    Task<Discount?> GetDiscountByIdAsync(int id);
    Task<Discount?> CreateDiscountAsync(Discount discount);
    Task<bool> UpdateDiscountAsync(int id, Discount discount);
    Task<bool> DeleteDiscountAsync(int id);
    Task<(bool isValid, string message, Discount? discount)> ValidateDiscountCodeAsync(string code);
}

/// <summary>
/// Implementation of discount service.
/// Handles HTTP communication with the MyShopAPI backend.
/// </summary>
public class DiscountService : IDiscountService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5002";
    private const string DiscountsEndpoint = "/api/discounts";

    public DiscountService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    /// <summary>
    /// Gets all discounts for the current user.
    /// </summary>
    public async Task<List<Discount>> GetAllDiscountsAsync()
    {
        try
        {
            var discounts = await _httpClient.GetFromJsonAsync<List<Discount>>(DiscountsEndpoint);
            return discounts ?? new List<Discount>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting discounts: {ex.Message}");
            return new List<Discount>();
        }
    }

    /// <summary>
    /// Gets discounts with pagination.
    /// </summary>
    public async Task<DiscountPagedResult> GetDiscountsPagedAsync(int page, int pageSize)
    {
        try
        {
            var url = $"{DiscountsEndpoint}?page={page}&pageSize={pageSize}";
            var result = await _httpClient.GetFromJsonAsync<DiscountPagedResult>(url);
            return result ?? new DiscountPagedResult();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting paged discounts: {ex.Message}");
            return new DiscountPagedResult();
        }
    }

    /// <summary>
    /// Gets a single discount by ID.
    /// </summary>
    public async Task<Discount?> GetDiscountByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Discount>($"{DiscountsEndpoint}/{id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting discount {id}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Creates a new discount.
    /// </summary>
    public async Task<Discount?> CreateDiscountAsync(Discount discount)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(DiscountsEndpoint, discount);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Discount>();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Error creating discount: {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating discount: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Updates an existing discount.
    /// </summary>
    public async Task<bool> UpdateDiscountAsync(int id, Discount discount)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{DiscountsEndpoint}/{id}", discount);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating discount {id}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Deletes a discount by ID.
    /// </summary>
    public async Task<bool> DeleteDiscountAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{DiscountsEndpoint}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting discount {id}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Validates a discount code.
    /// </summary>
    public async Task<(bool isValid, string message, Discount? discount)> ValidateDiscountCodeAsync(string code)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ValidationResponse>(
                $"{DiscountsEndpoint}/validate/{Uri.EscapeDataString(code)}");

            if (response == null)
            {
                return (false, "Error validating code", null);
            }

            return (response.IsValid, response.Message, response.Discount);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error validating discount code: {ex.Message}");
            return (false, $"Error: {ex.Message}", null);
        }
    }

    // Helper class for validation response
    private class ValidationResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public Discount? Discount { get; set; }
    }
}
