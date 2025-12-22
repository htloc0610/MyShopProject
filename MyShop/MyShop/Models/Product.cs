using System;

namespace MyShop.Models
{
    /// <summary>
    /// Represents a product in the shop (Client-side model).
    /// Matches the API Product model for serialization.
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Formatted price for display
        /// </summary>
        public string FormattedPrice => $"${Price:N2}";

        /// <summary>
        /// Stock status for display
        /// </summary>
        public string StockStatus => Stock > 0 ? $"{Stock} in stock" : "Out of stock";
    }
}
