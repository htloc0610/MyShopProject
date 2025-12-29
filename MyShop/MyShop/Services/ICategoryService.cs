using System.Collections.Generic;
using System.Threading.Tasks;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Interface for category-related API operations.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Gets all categories from the API.
    /// </summary>
    Task<List<Category>> GetCategoriesAsync();
}
