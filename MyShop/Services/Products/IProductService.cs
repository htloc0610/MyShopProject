using System.Collections.Generic;
using System.Threading.Tasks;
using MyShop.Models.Products;
using MyShop.Models.Shared;

namespace MyShop.Services.Products;

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
    /// Gets ALL products with optional filters (no paging, no keyword search).
    /// Client should apply fuzzy search and paging.
    /// </summary>
    Task<List<Product>> GetAllProductsAsync(
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

    /// <summary>
    /// Imports multiple products from Excel.
    /// </summary>
    Task<(bool Success, int ImportedCount, List<string> Errors)> ImportProductsAsync(List<ProductImportDto> products);
}
