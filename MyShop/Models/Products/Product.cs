using System;
using System.Collections.Generic;
using System.Linq;

namespace MyShop.Models.Products
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
        /// List of images from API
        /// </summary>
        public List<string> Images { get; set; } = new();

        /// <summary>
        /// List of image URLs for the product (for FlipView)
        /// </summary>
        public List<string> ImageUrls
        {
            get
            {
                // If API returns images, use them
                if (Images != null && Images.Count > 0)
                {
                    return Images;
                }

                // If ImageUrl is empty, return placeholder images from internet
                if (string.IsNullOrWhiteSpace(ImageUrl))
                {
                    return new List<string>
                    {
                        "/Assets/StoreLogo.png" // Local fallback
                    };
                }

                // If only main image exists
                return new List<string> { ImageUrl };
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

        /// <summary>
        /// Gets the first image for display in lists (Grid/DataGrid)
        /// </summary>
        public string DisplayImage => ImageUrls.FirstOrDefault() ?? "/Assets/StoreLogo.png";
    }
}
