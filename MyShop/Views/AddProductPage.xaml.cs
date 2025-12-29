using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Models;
using MyShop.ViewModels;

namespace MyShop.Views;

/// <summary>
/// Page for adding a new product.
/// Provides form to input all required product information.
/// </summary>
public sealed partial class AddProductPage : Page
{
    public ProductViewModel ViewModel { get; }

    public AddProductPage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ProductViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // Load categories when page is navigated to
        await ViewModel.LoadCategoriesCommand.ExecuteAsync(null);
        
        // Clear form
        ClearForm();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm())
        {
            return;
        }

        try
        {
            // Create new product from form inputs
            var newProduct = new Product
            {
                Sku = SkuTextBox.Text.Trim(),
                Name = NameTextBox.Text.Trim(),
                Price = (decimal)PriceNumberBox.Value,
                Stock = (int)StockNumberBox.Value,
                Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text) 
                    ? "" 
                    : DescriptionTextBox.Text.Trim(),
                CategoryId = (int)CategoryComboBox.SelectedValue
            };

            // Call ViewModel to create product
            await ViewModel.CreateProductCommand.ExecuteAsync(newProduct);

            // Show success message
            await ShowDialogAsync("Thành công", "Đã thêm sản phẩm mới thành công!");

            // Navigate back to product list
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
        catch (Exception ex)
        {
            await ShowDialogAsync("Lỗi", $"Lỗi khi thêm sản phẩm: {ex.Message}");
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private void UploadImageButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement image upload functionality
        _ = ShowDialogAsync("Thông báo", "Chức năng tải ảnh lên sẽ được cập nhật trong phiên bản sau.");
    }

    private void ClearForm()
    {
        NameTextBox.Text = string.Empty;
        PriceNumberBox.Value = 0;
        StockNumberBox.Value = 0;
        SkuTextBox.Text = string.Empty;
        DescriptionTextBox.Text = string.Empty;
        CategoryComboBox.SelectedIndex = -1;
    }

    private bool ValidateForm()
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            _ = ShowDialogAsync("Lỗi", "Tên sản phẩm không được để trống.");
            NameTextBox.Focus(FocusState.Programmatic);
            return false;
        }

        // Validate SKU
        if (string.IsNullOrWhiteSpace(SkuTextBox.Text))
        {
            _ = ShowDialogAsync("Lỗi", "Mã SKU không được để trống.");
            SkuTextBox.Focus(FocusState.Programmatic);
            return false;
        }

        // Validate price
        if (PriceNumberBox.Value <= 0)
        {
            _ = ShowDialogAsync("Lỗi", "Giá sản phẩm phải lớn hơn 0.");
            PriceNumberBox.Focus(FocusState.Programmatic);
            return false;
        }

        // Validate category
        if (CategoryComboBox.SelectedValue == null)
        {
            _ = ShowDialogAsync("Lỗi", "Vui lòng chọn loại sản phẩm.");
            CategoryComboBox.Focus(FocusState.Programmatic);
            return false;
        }

        // Validate stock
        if (StockNumberBox.Value < 0)
        {
            _ = ShowDialogAsync("Lỗi", "Số lượng tồn kho không được âm.");
            StockNumberBox.Focus(FocusState.Programmatic);
            return false;
        }

        return true;
    }

    private async Task ShowDialogAsync(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }
}
