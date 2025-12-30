using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Models.Categories;
using MyShop.ViewModels.Categories;

namespace MyShop.Views.Categories;

/// <summary>
/// Page for managing categories with full CRUD operations.
/// </summary>
public sealed partial class CategoryListPage : Page
{
    public CategoryViewModel ViewModel { get; }

    public CategoryListPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<CategoryViewModel>();
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
    }

    private async void AddCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CategoryDialog
        {
            XamlRoot = this.XamlRoot
        };
        
        var result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            var success = await ViewModel.CreateCategoryAsync(
                dialog.CategoryName,
                dialog.CategoryDescription);
            
            if (success)
            {
                await ShowSuccessDialogAsync("Tạo danh mục thành công!");
            }
        }
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Category category)
        {
            var dialog = new CategoryDialog(category)
            {
                XamlRoot = this.XamlRoot
            };
            
            var result = await dialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                var success = await ViewModel.UpdateCategoryAsync(
                    category.CategoryId,
                    dialog.CategoryName,
                    dialog.CategoryDescription);
                
                if (success)
                {
                    await ShowSuccessDialogAsync("Cập nhật danh mục thành công!");
                }
            }
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Category category)
        {
            var confirmDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = $"Bạn có chắc chắn muốn xóa danh mục '{category.Name}' không?\n\n" +
                          $"Danh mục này có {category.ProductCount} sản phẩm.",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var (success, errorMessage) = await ViewModel.DeleteCategoryAsync(category.CategoryId);
                
                if (success)
                {
                    await ShowSuccessDialogAsync("Xóa danh mục thành công!");
                }
                else
                {
                    await ShowErrorDialogAsync("Không thể xóa danh mục", errorMessage ?? "Lỗi không xác định");
                }
            }
        }
    }

    private async System.Threading.Tasks.Task ShowSuccessDialogAsync(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Thành công",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async System.Threading.Tasks.Task ShowErrorDialogAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }
}

/// <summary>
/// Dialog for creating/editing categories.
/// </summary>
public sealed class CategoryDialog : ContentDialog
{
    private readonly TextBox _nameTextBox;
    private readonly TextBox _descriptionTextBox;

    public string CategoryName => _nameTextBox.Text;
    public string CategoryDescription => _descriptionTextBox.Text;

    public CategoryDialog(Category? existingCategory = null)
    {
        Title = existingCategory == null ? "Thêm Danh Mục Mới" : "Sửa Danh Mục";
        PrimaryButtonText = "Lưu";
        CloseButtonText = "Hủy";
        DefaultButton = ContentDialogButton.Primary;

        var stackPanel = new StackPanel { Spacing = 16 };

        // Name TextBox
        _nameTextBox = new TextBox
        {
            Header = "Tên danh mục",
            PlaceholderText = "Nhập tên danh mục...",
            Text = existingCategory?.Name ?? string.Empty
        };
        stackPanel.Children.Add(_nameTextBox);

        // Description TextBox
        _descriptionTextBox = new TextBox
        {
            Header = "Mô tả",
            PlaceholderText = "Nhập mô tả...",
            Text = existingCategory?.Description ?? string.Empty,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 100
        };
        stackPanel.Children.Add(_descriptionTextBox);

        Content = stackPanel;

        // Validate before allowing save
        PrimaryButtonClick += OnPrimaryButtonClick;
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(CategoryName))
        {
            args.Cancel = true;
        }
    }
}
