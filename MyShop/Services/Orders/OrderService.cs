using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MyShop.Models.Orders;

namespace MyShop.Services.Orders;

/// <summary>
/// Service for managing order operations via API.
/// </summary>
public class OrderService : IOrderService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5002";
    private const string OrdersEndpoint = "/api/orders";

    public OrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    /// <summary>
    /// Preview an order with optional coupon code.
    /// Calls POST /api/orders/preview to calculate totals.
    /// </summary>
    public async Task<OrderPreviewResponse?> PreviewOrderAsync(OrderPreviewRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{OrdersEndpoint}/preview", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OrderPreviewResponse>();
            }
            
            System.Diagnostics.Debug.WriteLine($"Preview order failed: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error previewing order: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Create and checkout an order.
    /// Calls POST /api/orders/checkout.
    /// </summary>
    public async Task<OrderCheckoutResponse?> CheckoutOrderAsync(OrderCheckoutRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{OrdersEndpoint}/checkout", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OrderCheckoutResponse>();
            }
            
            System.Diagnostics.Debug.WriteLine($"Checkout order failed: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking out order: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get list of available/valid coupons.
    /// Calls GET /api/orders/available-coupons.
    /// </summary>
    public async Task<List<AvailableCoupon>> GetAvailableCouponsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{OrdersEndpoint}/available-coupons");
            
            if (response.IsSuccessStatusCode)
            {
                var coupons = await response.Content.ReadFromJsonAsync<List<AvailableCoupon>>();
                return coupons ?? new List<AvailableCoupon>();
            }
            
            System.Diagnostics.Debug.WriteLine($"Get available coupons failed: {response.StatusCode}");
            return new List<AvailableCoupon>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting available coupons: {ex.Message}");
            return new List<AvailableCoupon>();
        }
    }

    /// <summary>
    /// Get paginated list of orders with advanced filtering and sorting.
    /// Calls GET /api/orders with pagination, search, filters, and sorting params.
    /// </summary>
    public async Task<PagedResult<OrderListItem>?> GetOrdersAsync(
        int page = 1, 
        int pageSize = 10, 
        string? searchKeyword = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        string? sortBy = null,
        string? sortDirection = null)
    {
        try
        {
            var url = $"{OrdersEndpoint}?page={page}&pageSize={pageSize}";
            
            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                url += $"&search={Uri.EscapeDataString(searchKeyword)}";
            }
            
            if (!string.IsNullOrWhiteSpace(status))
            {
                url += $"&status={Uri.EscapeDataString(status)}";
            }
            
            if (startDate.HasValue)
            {
                url += $"&startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}";
            }
            
            if (endDate.HasValue)
            {
                url += $"&endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}";
            }
            
            if (minAmount.HasValue)
            {
                url += $"&minAmount={minAmount.Value}";
            }
            
            if (maxAmount.HasValue)
            {
                url += $"&maxAmount={maxAmount.Value}";
            }
            
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                url += $"&sortBy={Uri.EscapeDataString(sortBy)}";
            }
            
            if (!string.IsNullOrWhiteSpace(sortDirection))
            {
                url += $"&sortDirection={Uri.EscapeDataString(sortDirection)}";
            }

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PagedResult<OrderListItem>>();
            }
            
            System.Diagnostics.Debug.WriteLine($"Get orders failed: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting orders: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get full details of a specific order.
    /// Calls GET /api/orders/{id}.
    /// </summary>
    public async Task<OrderDetail?> GetOrderByIdAsync(int orderId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{OrdersEndpoint}/{orderId}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OrderDetail>();
            }
            
            System.Diagnostics.Debug.WriteLine($"Get order by ID failed: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting order by ID: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Update order information (customer details and status).
    /// Calls PUT /api/orders/{id}.
    /// </summary>
    public async Task<OrderDetail?> UpdateOrderAsync(int orderId, string customerName, string? customerPhone, string? customerAddress, string status)
    {
        try
        {
            var updateDto = new
            {
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                CustomerAddress = customerAddress,
                Status = status
            };

            var response = await _httpClient.PutAsJsonAsync($"{OrdersEndpoint}/{orderId}", updateDto);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OrderDetail>();
            }
            
            System.Diagnostics.Debug.WriteLine($"Update order failed: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating order: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Delete an order by ID.
    /// Calls DELETE /api/orders/{id}.
    /// </summary>
    public async Task<bool> DeleteOrderAsync(int orderId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{OrdersEndpoint}/{orderId}");
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            // Extract error message from API response
            var errorContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Delete order failed: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Error content: {errorContent}");
            
            // Try to parse JSON error response
            try
            {
                var errorObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent);
                if (errorObj != null && errorObj.ContainsKey("message"))
                {
                    throw new Exception(errorObj["message"].ToString());
                }
            }
            catch (System.Text.Json.JsonException)
            {
                // If not JSON, use the raw content
            }
            
            throw new Exception($"Không thể xóa đơn hàng (Status: {response.StatusCode})");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting order: {ex.Message}");
            throw; // Re-throw to let ViewModel handle it
        }
    }
}
