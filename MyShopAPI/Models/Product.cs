namespace MyShopAPI.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public string Sku { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int ImportPrice { get; set; }

        public int Count { get; set; }

        public string Description { get; set; } = string.Empty;

        // Foreign key to Category
        public int CategoryId { get; set; }

        // Data ownership - links product to specific user/shop
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        // Navigation
        public Category Category { get; set; } = null!;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
