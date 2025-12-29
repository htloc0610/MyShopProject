namespace MyShop.Models
{
    /// <summary>
    /// Represents a product category (Client-side DTO).
    /// </summary>
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
