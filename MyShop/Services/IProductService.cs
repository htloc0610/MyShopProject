using System.Collections.Generic;
using System.Threading.Tasks;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Interface for product-related API operations.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Gets products with paging, sorting, and filtering support.
    /// </summary>
    Task<PagedResult<Product>> GetProductsPagedAsync(
        int page, 
        int pageSize, 
        string? sortBy = null, 
        bool isDescending = false,
        string? keyword = null,
        int? categoryId = null,
        double? minPrice = null,
        double? maxPrice = null);

    /// <summary>
    /// Gets a single product by ID.
    /// </summary>
    Task<Product?> GetProductByIdAsync(int id);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    Task<Product?> CreateProductAsync(Product product);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    Task<Product?> UpdateProductAsync(int id, ProductUpdateDto productUpdate);

    /// <summary>
    /// Deletes a product by ID.
    /// </summary>
    Task<bool> DeleteProductAsync(int id);
}
