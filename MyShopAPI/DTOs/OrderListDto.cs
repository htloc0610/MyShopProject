namespace MyShopAPI.DTOs;

/// <summary>
/// DTO for order list item (lightweight for list views).
/// </summary>
public class OrderListDto
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
}
