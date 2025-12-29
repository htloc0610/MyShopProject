using System;
using System.Collections.Generic;
using System.Linq;

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

        // Additional properties for editing (need to get from API)
        public string Sku { get; set; } = string.Empty;
        public int CategoryId { get; set; }

        /// <summary>
        /// List of image URLs for the product (for FlipView)
        /// </summary>
        public List<string> ImageUrls
        {
            get
            {
                // If ImageUrl is empty, return placeholder images from internet
                if (string.IsNullOrWhiteSpace(ImageUrl))
                {
                    return new List<string>
                    {
                        "https://via.placeholder.com/400x400/0078D4/FFFFFF?text=Product+Image+1",
                        "https://via.placeholder.com/400x400/107C10/FFFFFF?text=Product+Image+2",
                        "https://via.placeholder.com/400x400/D83B01/FFFFFF?text=Product+Image+3"
                    };
                }

                // If there's a single image, duplicate it 3 times for demo
                // In real scenario, API should provide multiple images
                return new List<string> { ImageUrl, ImageUrl, ImageUrl };
            }
        }

        /// <summary>
        /// Formatted price for display in VND
        /// </summary>
        public string FormattedPrice => $"{Price:N0} VND";

        /// <summary>
        /// Stock status for display
        /// </summary>
        public string StockStatus => Stock > 0 ? $"{Stock} sản phẩm" : "Hết hàng";

        /// <summary>
        /// Stock color indicator
        /// </summary>
        public string StockColor => Stock > 10 ? "Green" : Stock > 0 ? "Orange" : "Red";
    }
}
