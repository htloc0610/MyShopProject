namespace MyShop.Models.Categories
{
    /// <summary>
    /// Represents a product category (Client-side DTO).
    /// </summary>
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ProductCount { get; set; }

        /// <summary>
        /// Display text for ComboBox
        /// </summary>
        public string DisplayText => $"{Name} ({ProductCount} sản phẩm)";
    }
}
