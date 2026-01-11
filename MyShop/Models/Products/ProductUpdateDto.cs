namespace MyShop.Models.Products;

/// <summary>
/// Data Transfer Object for updating a product.
/// Used when sending update requests to the API.
/// </summary>
public class ProductUpdateDto
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal ImportPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int Count { get; set; }
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public System.Collections.Generic.List<string> Images { get; set; } = new();
}
