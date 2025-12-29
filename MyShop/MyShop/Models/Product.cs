using System;

namespace MyShop.Models
{
    /// <summary>
    /// Represents a product in the shop (Client-side DTO).
    /// Matches the API ProductResponseDto for JSON deserialization.
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

        /// <summary>
        /// Formatted price for display in VND
        /// </summary>
        public string FormattedPrice => $"{Price:N0} VN?";

        /// <summary>
        /// Stock status for display
        /// </summary>
        public string StockStatus => Stock > 0 ? $"{Stock} s?n ph?m" : "H?t hàng";

        /// <summary>
        /// Stock color indicator
        /// </summary>
        public string StockColor => Stock > 10 ? "Green" : Stock > 0 ? "Orange" : "Red";
    }
}
