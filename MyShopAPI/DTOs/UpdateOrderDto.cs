namespace MyShopAPI.DTOs;

/// <summary>
/// DTO for updating order information.
/// </summary>
public class UpdateOrderDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
    public string Status { get; set; } = "New";
}
