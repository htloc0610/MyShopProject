using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Models.Products;
using MyShop.ViewModels.Products;
using MyShop.Services.Products;

namespace MyShop.Views.Products;

/// <summary>
/// Page for displaying detailed product information.
/// Supports navigation from product list and edit mode.
/// </summary>
public sealed partial class ProductDetailPage : Page, INotifyPropertyChanged
{
    private Product? _product;
    private bool _isEditMode;

    public Product? Product 
    { 
        get => _product;
        private set
        {
            if (_product != value)
            {
                _product = value;
                OnPropertyChanged();
            }
        }
    }

    public ProductViewModel ViewModel { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ProductDetailPage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ProductViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Product product)
        {
            Product = product;
            await ViewModel.LoadCategoriesCommand.ExecuteAsync(null);
            Bindings.Update();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isEditMode)
        {
            // Cancel edit - reload original data
            _isEditMode = false;
            await ReloadProductAsync();
        }
        else
        {
            // Enter edit mode
            if (ViewModel.Categories?.Count == 0)
            {
                await ViewModel.LoadCategoriesCommand.ExecuteAsync(null);
            }

            // Populate images for editing
            ViewModel.SelectedImageUrls.Clear();
            if (Product.Images != null)
            {
                foreach (var img in Product.Images)
                {
                    ViewModel.SelectedImageUrls.Add(img);
                }
            }
            else if (!string.IsNullOrEmpty(Product.ImageUrl))
            {
                // Legacy support for single image
                ViewModel.SelectedImageUrls.Add(Product.ImageUrl);
            }

            _isEditMode = true;
        }
        
        UpdateUIState();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (Product == null || !ValidateAndUpdateProduct())
        {
            return;
        }

        try
        {
            await ViewModel.UpdateProductCommand.ExecuteAsync(Product);
            _isEditMode = false;
            UpdateUIState();

            await ShowDialogAsync("Thành công", "Đã cập nhật sản phẩm thành công!");
            await ReloadProductAsync();
        }
        catch (Exception ex)
        {
            await ShowDialogAsync("Lỗi", $"Lỗi khi cập nhật sản phẩm: {ex.Message}");
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (Product == null) return;

        var dialog = new ContentDialog
        {
            Title = "Xác nhận xóa",
            Content = $"Bạn có chắc chắn muốn xóa sản phẩm '{Product.Name}' không?\n\nHành động này không thể hoàn tác.",
            PrimaryButtonText = "Xóa",
            CloseButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteProductCommand.ExecuteAsync(Product);
            
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }

    private async Task ReloadProductAsync()
    {
        if (Product == null) return;

        try
        {
            var productService = App.Current.Services.GetRequiredService<IProductService>();
            var updatedProduct = await productService.GetProductByIdAsync(Product.Id);
            
            if (updatedProduct != null)
            {
                Product = updatedProduct;
                Bindings.Update();
            }
        }
        catch (Exception ex)
        {
            await ShowDialogAsync("Lỗi", $"Lỗi khi tải lại sản phẩm: {ex.Message}");
        }
    }

    private void UpdateUIState()
    {
        if (Product == null) return;

        // Toggle visibility based on mode
        NameTextBlock.Visibility = _isEditMode ? Visibility.Collapsed : Visibility.Visible;
        NameTextBox.Visibility = _isEditMode ? Visibility.Visible : Visibility.Collapsed;
        
        PriceTextBlock.Visibility = _isEditMode ? Visibility.Collapsed : Visibility.Visible;
        PriceNumberBox.Visibility = _isEditMode ? Visibility.Visible : Visibility.Collapsed;
        
        CategoryBadge.Visibility = _isEditMode ? Visibility.Collapsed : Visibility.Visible;
        CategoryComboBox.Visibility = _isEditMode ? Visibility.Visible : Visibility.Collapsed;
        
        StockStatusPanel.Visibility = _isEditMode ? Visibility.Collapsed : Visibility.Visible;
        StockNumberBox.Visibility = _isEditMode ? Visibility.Visible : Visibility.Collapsed;
        
        SkuTextBlock.Visibility = _isEditMode ? Visibility.Collapsed : Visibility.Visible;
        SkuTextBox.Visibility = _isEditMode ? Visibility.Visible : Visibility.Collapsed;
        

        
        DescriptionBorder.Visibility = _isEditMode ? Visibility.Collapsed : Visibility.Visible;
        DescriptionTextBox.Visibility = _isEditMode ? Visibility.Visible : Visibility.Collapsed;

        // Toggle Image Panels
        ImageGalleryPanel.Visibility = _isEditMode ? Visibility.Collapsed : Visibility.Visible;
        ImageEditorPanel.Visibility = _isEditMode ? Visibility.Visible : Visibility.Collapsed;

        if (_isEditMode)
        {
            // Populate edit controls with current values
            NameTextBox.Text = Product.Name;
            PriceNumberBox.Value = (double)Product.Price;
            CategoryComboBox.SelectedValue = Product.CategoryId;
            StockNumberBox.Value = Product.Stock;
            SkuTextBox.Text = Product.Sku;
            DescriptionTextBox.Text = Product.Description;

            // Update buttons
            EditIcon.Glyph = "\uE73E"; // Cancel icon
            EditText.Text = "Hủy";
            SaveButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Collapsed;
        }
        else
        {
            // Update buttons
            EditIcon.Glyph = "\uE70F"; // Edit icon
            EditText.Text = "Chỉnh sửa";
            SaveButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Visible;
        }
    }

    private bool ValidateAndUpdateProduct()
    {
        if (Product == null) return false;

        // Validate name
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            _ = ShowDialogAsync("Lỗi", "Tên sản phẩm không được để trống.");
            return false;
        }

        // Validate price
        if (PriceNumberBox.Value <= 0)
        {
            _ = ShowDialogAsync("Lỗi", "Giá sản phẩm phải lớn hơn 0.");
            return false;
        }

        // Validate category
        if (CategoryComboBox.SelectedValue == null)
        {
            _ = ShowDialogAsync("Lỗi", "Vui lòng chọn loại sản phẩm.");
            return false;
        }

        // Validate stock
        if (StockNumberBox.Value < 0)
        {
            _ = ShowDialogAsync("Lỗi", "Số lượng tồn kho không được âm.");
            return false;
        }

        // Update product properties from UI
        Product.Name = NameTextBox.Text;
        Product.Price = (decimal)PriceNumberBox.Value;
        Product.Stock = (int)StockNumberBox.Value;
        Product.Sku = SkuTextBox.Text;
        Product.Description = DescriptionTextBox.Text;
        Product.CategoryId = (int)CategoryComboBox.SelectedValue;

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
