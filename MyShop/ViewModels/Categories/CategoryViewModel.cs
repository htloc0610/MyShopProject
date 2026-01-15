using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models.Categories;
using MyShop.Services.Categories;
using MyShop.Services.Shared;

namespace MyShop.ViewModels.Categories;

/// <summary>
/// ViewModel for managing categories.
/// Provides CRUD operations and data binding for Category views.
/// </summary>
public partial class CategoryViewModel : ObservableObject
{
    private readonly ICategoryService _categoryService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _categoryCount;

    public CategoryViewModel(ICategoryService categoryService, IToastService toastService)
    {
        _categoryService = categoryService;
        _toastService = toastService;
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
            _toastService.ShowError($"Lỗi khi tải danh mục: {ex.Message}");
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

            var newCategory = await _categoryService.CreateCategoryAsync(name, description);
            
            if (newCategory != null)
            {
                Categories.Add(newCategory);
                CategoryCount = Categories.Count;
                _toastService.ShowSuccess($"Tạo danh mục '{name}' thành công!");
                return true;
            }
            
            _toastService.ShowError("Không thể tạo danh mục. Tên đã tồn tại.");
            return false;
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"Lỗi khi tạo danh mục: {ex.Message}");
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

            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, name, description);
            
            if (updatedCategory != null)
            {
                var existing = Categories.FirstOrDefault(c => c.CategoryId == id);
                if (existing != null)
                {
                    var index = Categories.IndexOf(existing);
                    Categories[index] = updatedCategory;
                }
                _toastService.ShowSuccess($"Cập nhật danh mục '{name}' thành công!");
                return true;
            }
            
            _toastService.ShowError("Không thể cập nhật danh mục. Tên đã tồn tại.");
            return false;
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"Lỗi khi cập nhật danh mục: {ex.Message}");
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

            var (success, errorMessage) = await _categoryService.DeleteCategoryAsync(id);
            
            if (success)
            {
                var category = Categories.FirstOrDefault(c => c.CategoryId == id);
                if (category != null)
                {
                    Categories.Remove(category);
                    CategoryCount = Categories.Count;
                }
                _toastService.ShowSuccess("Xóa danh mục thành công!");
                return (true, null);
            }
            
            _toastService.ShowError(errorMessage ?? "Không thể xóa danh mục");
            return (false, errorMessage);
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"Lỗi khi xóa danh mục: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Error deleting category: {ex}");
            return (false, ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
