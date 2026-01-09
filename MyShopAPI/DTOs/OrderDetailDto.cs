namespace MyShopAPI.DTOs;

/// <summary>
/// DTO for full order details view (including items).
/// </summary>
public class OrderDetailDto
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
    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for individual items within an order.
/// </summary>
public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
