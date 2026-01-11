using MyShopAPI.DTOs;
using MyShopAPI.Models;
using System.Linq;

namespace MyShopAPI.Mappers
{
    public static class ProductMapper
    {
        public static ProductResponseDto ToDto(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.ProductId,
                Sku = product.Sku,
                Name = product.Name,
                Description = product.Description,
                ImportPrice = product.ImportPrice,
                SellingPrice = product.SellingPrice,
                Stock = product.Count,
                Category = product.Category.Name,
                CategoryId = product.CategoryId,
                ImageUrl = product.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? product.Images.FirstOrDefault()?.ImageUrl ?? string.Empty,
                Images = product.Images.Select(i => i.ImageUrl).ToList()
            };
        }
    }
}
