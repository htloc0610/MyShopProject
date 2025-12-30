using MyShopAPI.DTOs;
using MyShopAPI.Models;

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
                Price = product.ImportPrice,
                Stock = product.Count,
                Category = product.Category.Name,
                CategoryId = product.CategoryId,
                ImageUrl = string.Empty
            };
        }
    }
}
