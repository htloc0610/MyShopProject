namespace MyShopAPI.DTOs
{
    /// <summary>
    /// Data Transfer Object for creating a new product.
    /// Used when receiving product create requests from clients.
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
}
