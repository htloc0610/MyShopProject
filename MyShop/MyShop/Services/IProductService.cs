using System.Collections.Generic;
using System.Threading.Tasks;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Interface for product-related API operations.
/// Defines contract for fetching product data from backend API.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Gets all products from the API.
    /// </summary>
    /// <returns>List of products or empty list if error occurs</returns>
    Task<List<Product>> GetProductsAsync();

    /// <summary>
    /// Gets a single product by ID.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product if found, null otherwise</returns>
    Task<Product?> GetProductByIdAsync(int id);

    /// <summary>
    /// Gets products by category ID.
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>List of products in the category</returns>
    Task<List<Product>> GetProductsByCategoryAsync(int categoryId);
}
