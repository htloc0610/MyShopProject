namespace MyShop.Models.Orders;

public class UpdateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
    public string Status { get; set; } = string.Empty;
}
