using System.Collections.Generic;
using System.Threading.Tasks;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Interface for category-related API operations.
/// Provides full CRUD functionality.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Gets all categories from the API.
    /// </summary>
    Task<List<Category>> GetCategoriesAsync();

    /// <summary>
    /// Gets a category by ID.
    /// </summary>
    Task<Category?> GetCategoryByIdAsync(int id);

    /// <summary>
    /// Creates a new category.
    /// </summary>
    Task<Category?> CreateCategoryAsync(string name, string description);

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    Task<Category?> UpdateCategoryAsync(int id, string name, string description);

    /// <summary>
    /// Deletes a category.
    /// Returns tuple with success status and error message if failed.
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> DeleteCategoryAsync(int id);
}
