using System.Collections.Generic;

namespace MyShopAPI.DTOs
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal ImportPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new List<string>();
    }
}
