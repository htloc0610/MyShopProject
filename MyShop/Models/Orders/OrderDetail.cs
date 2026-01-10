using System;
using System.Collections.Generic;

namespace MyShop.Models.Orders;

/// <summary>
/// Model for full order details.
/// </summary>
public class OrderDetail
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    
    // Customer information
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
    
    // Order totals
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    
    // Coupon information
    public string? CouponCode { get; set; }
    
    // Order status
    public string Status { get; set; } = "New";
    
    // Order items
    public List<OrderItemDetail> Items { get; set; } = new();

    // Computed properties for UI
    public string FormattedOrderDate { get {return OrderDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm");} }
    public string FormattedTotalAmount { get { return $"{TotalAmount:N0} VNĐ"; } }
    public string FormattedFinalAmount { get { return $"{FinalAmount:N0} VNĐ"; } }
    public string FormattedDiscountAmount { get { return $"-{DiscountAmount:N0} VNĐ"; } }
}

/// <summary>
/// Model for individual items within an order.
/// </summary>
public class OrderItemDetail
{
    public int OrderItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Computed properties for UI
    public string FormattedUnitPrice => $"{UnitPrice:N0} VNĐ";
    public string FormattedTotalPrice => $"{TotalPrice:N0} VNĐ";
}
