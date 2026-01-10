namespace MyShopAPI.DTOs
{
    /// <summary>
    /// Data Transfer Object for updating a product.
    /// Used when receiving product update requests from clients.
    /// </summary>
    public class ProductUpdateDto
    {
        public int ProductId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal ImportPrice { get; set; }
        public int Count { get; set; }
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public List<string> Images { get; set; } = new List<string>();
    }
}
