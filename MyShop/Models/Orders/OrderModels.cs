using System;
using System.Collections.Generic;
using System.Linq;

namespace MyShop.Models.Orders;

/// <summary>
/// Request model for order preview API.
/// </summary>
public class OrderPreviewRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
    public string? CouponCode { get; set; }
}

/// <summary>
/// Individual order item request.
/// </summary>
public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Response from order preview API.
/// </summary>
public class OrderPreviewResponse
{
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string? CouponMessage { get; set; }
}

/// <summary>
/// Request model for order checkout API.
/// </summary>
public class OrderCheckoutRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
    public Guid? CustomerId { get; set; }
    public string? CouponCode { get; set; }
}

/// <summary>
/// Response from order checkout API.
/// </summary>
public class OrderCheckoutResponse
{
    public int OrderId { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal FinalAmount { get; set; }
}

/// <summary>
/// Model for available/valid coupons.
/// </summary>
public class AvailableCoupon
{
    public string Code { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    
    public string FormattedAmount => $"-{Amount:N0} VNĐ";
}
