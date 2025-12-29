using System.Collections.Generic;

namespace MyShop.Models;

/// <summary>
/// DTO for importing products from Excel.
/// </summary>
public class ProductImportDto
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new List<string>();
}
