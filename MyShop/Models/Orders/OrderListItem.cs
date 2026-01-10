using System;

namespace MyShop.Models.Orders;

/// <summary>
/// Model for order list item (lightweight).
/// </summary>
public class OrderListItem
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public int ItemCount { get; set; }
    public string? CouponCode { get; set; }
    public string Status { get; set; } = "New";

    // Computed properties for UI
    public string FormattedOrderDate { get { return OrderDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm"); } }
    public string FormattedTotalAmount { get { return $"{TotalAmount:N0}"; } }
    public string FormattedFinalAmount { get { return $"{FinalAmount:N0}"; } }
    public string FormattedDiscount { get { return DiscountAmount > 0 ? $"-{DiscountAmount:N0}" : "0"; } }
}
