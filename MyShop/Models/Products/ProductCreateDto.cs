namespace MyShop.Models.Products;

/// <summary>
/// Data Transfer Object for creating a new product.
/// Maps client fields to API expected field names.
/// </summary>
public class ProductCreateDto
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ImportPrice { get; set; }
    public int SellingPrice { get; set; }
    public int Count { get; set; }
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public System.Collections.Generic.List<string> Images { get; set; } = new();
}
