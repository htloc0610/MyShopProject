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
    /// Gets all products from the API.
    /// </summary>
    Task<List<Product>> GetProductsAsync();

    /// <summary>
    /// Gets products with paging and sorting support.
    /// </summary>
    Task<PagedResult<Product>> GetProductsPagedAsync(int page, int pageSize, string? sortBy = null, bool isDescending = false);

    /// <summary>
    /// Gets a single product by ID.
    /// </summary>
    Task<Product?> GetProductByIdAsync(int id);

    /// <summary>
    /// Gets products by category ID.
    /// </summary>
    Task<List<Product>> GetProductsByCategoryAsync(int categoryId);
}
