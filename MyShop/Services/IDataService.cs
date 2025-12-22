using System.Collections.Generic;
using System.Threading.Tasks;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Defines the contract for data operations.
/// This abstraction allows for easy testing and swapping of implementations.
/// </summary>
public interface IDataService
{
    /// <summary>
    /// Gets a welcome message for the application.
    /// </summary>
    string GetWelcomeMessage();

    /// <summary>
    /// Loads data asynchronously (simulated).
    /// </summary>
    Task<string> LoadDataAsync();

    /// <summary>
    /// Gets all products from the API.
    /// </summary>
    Task<List<Product>> GetProductsAsync();

    /// <summary>
    /// Gets a product by ID from the API.
    /// </summary>
    Task<Product?> GetProductByIdAsync(int id);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    Task<Product?> CreateProductAsync(Product product);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    Task<Product?> UpdateProductAsync(int id, Product product);

    /// <summary>
    /// Deletes a product.
    /// </summary>
    Task<bool> DeleteProductAsync(int id);
}
