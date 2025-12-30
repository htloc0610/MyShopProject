using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

/// <summary>
/// ViewModel for managing categories.
/// Provides CRUD operations and data binding for Category views.
/// </summary>
public partial class CategoryViewModel : ObservableObject
{
    private readonly ICategoryService _categoryService;

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private int _categoryCount;

    public CategoryViewModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Initialize ViewModel - Load categories.
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
    }

    /// <summary>
    /// Load all categories from API.
    /// </summary>
    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var categories = await _categoryService.GetCategoriesAsync();
            
            Categories.Clear();
            foreach (var category in categories.OrderBy(c => c.Name))
            {
                Categories.Add(category);
            }

            CategoryCount = Categories.Count;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"L?i khi t?i danh m?c: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Refresh categories list.
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadCategoriesAsync();
    }

    /// <summary>
    /// Create new category.
    /// </summary>
    public async Task<bool> CreateCategoryAsync(string name, string description)
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var newCategory = await _categoryService.CreateCategoryAsync(name, description);
            
            if (newCategory != null)
            {
                Categories.Add(newCategory);
                CategoryCount = Categories.Count;
                return true;
            }
            
            HasError = true;
            ErrorMessage = "Không th? t?o danh m?c. Tên có th? ?ã t?n t?i.";
            return false;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"L?i khi t?o danh m?c: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error creating category: {ex}");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Update existing category.
    /// </summary>
    public async Task<bool> UpdateCategoryAsync(int id, string name, string description)
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, name, description);
            
            if (updatedCategory != null)
            {
                var existing = Categories.FirstOrDefault(c => c.CategoryId == id);
                if (existing != null)
                {
                    var index = Categories.IndexOf(existing);
                    Categories[index] = updatedCategory;
                }
                return true;
            }
            
            HasError = true;
            ErrorMessage = "Không th? c?p nh?t danh m?c. Tên có th? ?ã t?n t?i.";
            return false;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"L?i khi c?p nh?t danh m?c: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error updating category: {ex}");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Delete category.
    /// Returns error message if deletion fails.
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteCategoryAsync(int id)
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var (success, errorMessage) = await _categoryService.DeleteCategoryAsync(id);
            
            if (success)
            {
                var category = Categories.FirstOrDefault(c => c.CategoryId == id);
                if (category != null)
                {
                    Categories.Remove(category);
                    CategoryCount = Categories.Count;
                }
                return (true, null);
            }
            
            HasError = true;
            ErrorMessage = errorMessage ?? "Không th? xóa danh m?c";
            return (false, errorMessage);
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"L?i khi xóa danh m?c: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error deleting category: {ex}");
            return (false, ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
