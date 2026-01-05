namespace MyShopAPI.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Data ownership - links category to specific user/shop
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
