using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyShop.Models.Orders;

namespace MyShop.Services.Orders;

/// <summary>
/// Interface for order-related API operations.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Preview an order with optional coupon code.
    /// Calls POST /api/orders/preview to calculate totals.
    /// </summary>
    Task<OrderPreviewResponse?> PreviewOrderAsync(OrderPreviewRequest request);

    /// <summary>
    /// Create and checkout an order.
    /// Calls POST /api/orders/checkout.
    /// </summary>
    Task<OrderCheckoutResponse?> CheckoutOrderAsync(OrderCheckoutRequest request);
    
    Task<List<AvailableCoupon>> GetAvailableCouponsAsync();

    /// <summary>
    /// Get paginated list of orders with advanced filtering and sorting.
    /// Calls GET /api/orders with pagination, search, filters, and sorting params.
    /// </summary>
    Task<PagedResult<OrderListItem>?> GetOrdersAsync(
        int page = 1, 
        int pageSize = 10, 
        string? searchKeyword = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        string? sortBy = null,
        string? sortDirection = null);

    /// <summary>
    /// Get full details of a specific order.
    /// Calls GET /api/orders/{id}.
    /// </summary>
    Task<OrderDetail?> GetOrderByIdAsync(int orderId);

    /// <summary>
    /// Update order information (customer details and status).
    /// </summary>
    Task<OrderDetail?> UpdateOrderAsync(int orderId, string customerName, string? customerPhone, string? customerAddress, string status);

    /// <summary>
    /// Delete an order by ID.
    /// Calls DELETE /api/orders/{id}.
    /// </summary>
    Task<bool> DeleteOrderAsync(int orderId);
}
