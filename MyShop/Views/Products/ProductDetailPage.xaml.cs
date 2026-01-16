using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Models.Products;
using MyShop.ViewModels.Products;
using MyShop.Services.Products;
using MyShop.Services.Shared;

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
    private readonly IToastService _toastService;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ProductDetailPage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ProductViewModel>();
        _toastService = App.Current.Services.GetRequiredService<IToastService>();
    }

    /// <summary>
    /// Handles image reordering when drag-drop completes
    /// </summary>
    private void EditorImageGridView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
    {
        // Get the new order from GridView.Items (which reflects the visual order after reorder)
        var newOrder = sender.Items.ToList();
        ViewModel.ReorderImages(newOrder);
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
            if (Product?.Images != null)
            {
                foreach (var img in Product.Images)
                {
                    ViewModel.SelectedImageUrls.Add(img);
                }
            }
            else if (Product != null && !string.IsNullOrEmpty(Product.ImageUrl))
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

            _toastService.ShowSuccess("Đã cập nhật sản phẩm thành công!");
            await ReloadProductAsync();
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"Lỗi khi cập nhật sản phẩm: {ex.Message}");
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
            _toastService.ShowError($"Lỗi khi tải lại sản phẩm: {ex.Message}");
        }
    }

    private void UpdateUIState()
    {
        if (Product == null) return;

        // Toggle visibility based on mode
        NameTextBlock.Visibility = _isEditMode ? Visibility.Collapsed : Visibility.Visible;
        NameTextBox.Visibility = _isEditMode ? Visibility.Visible : Visibility.Collapsed;
        
        ImportPriceTextBlock.Visibility = _isEditMode ? Visibility.Collapsed : Visibility.Visible;
        ImportPriceNumberBox.Visibility = _isEditMode ? Visibility.Visible : Visibility.Collapsed;

        SellingPriceTextBlock.Visibility = _isEditMode ? Visibility.Collapsed : Visibility.Visible;
        SellingPriceNumberBox.Visibility = _isEditMode ? Visibility.Visible : Visibility.Collapsed;
        
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
            ImportPriceNumberBox.Value = (double)Product.ImportPrice;
            SellingPriceNumberBox.Value = (double)Product.SellingPrice;
            CategoryComboBox.SelectedValue = Product.CategoryId;
            StockNumberBox.Value = Product.Stock;
            SkuTextBox.Text = Product.Sku;
            DescriptionTextBox.Text = Product.Description;

            // Update buttons
            EditIcon.Glyph = "\uE73E"; // Cancel icon
            EditText.Text = "Hủy";
            SaveButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Collapsed;

            // Toggle visibility - hide TextBlocks, show edit inputs
            NameTextBlock.Visibility = Visibility.Collapsed;
            NameTextBox.Visibility = Visibility.Visible;
            
            ImportPriceTextBlock.Visibility = Visibility.Collapsed;
            ImportPriceNumberBox.Visibility = Visibility.Visible;
            
            SellingPriceTextBlock.Visibility = Visibility.Collapsed;
            SellingPriceNumberBox.Visibility = Visibility.Visible;
        }
        else
        {
            // Update buttons
            EditIcon.Glyph = "\uE70F"; // Edit icon
            EditText.Text = "Chỉnh sửa";
            SaveButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Visible;

            // Toggle visibility - show TextBlocks, hide edit inputs
            NameTextBlock.Visibility = Visibility.Visible;
            NameTextBox.Visibility = Visibility.Collapsed;
            
            ImportPriceTextBlock.Visibility = Visibility.Visible;
            ImportPriceNumberBox.Visibility = Visibility.Collapsed;
            
            SellingPriceTextBlock.Visibility = Visibility.Visible;
            SellingPriceNumberBox.Visibility = Visibility.Collapsed;
        }
    }

    private bool ValidateAndUpdateProduct()
    {
        if (Product == null) return false;

        // Validate name
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            _toastService.ShowWarning("Tên sản phẩm không được để trống.");
            return false;
        }

        // Validate import price
        if (ImportPriceNumberBox.Value < 0)
        {
            _toastService.ShowWarning("Giá nhập không được âm.");
            return false;
        }

        // Validate selling price
        if (SellingPriceNumberBox.Value <= 0)
        {
             _toastService.ShowWarning("Giá bán phải lớn hơn 0.");
             return false;
        }

        // Validate category
        if (CategoryComboBox.SelectedValue == null)
        {
            _toastService.ShowWarning("Vui lòng chọn loại sản phẩm.");
            return false;
        }

        // Validate stock
        if (StockNumberBox.Value < 0)
        {
            _toastService.ShowWarning("Số lượng tồn kho không được âm.");
            return false;
        }

        // Update product properties from UI
        Product.Name = NameTextBox.Text;
        Product.ImportPrice = (decimal)ImportPriceNumberBox.Value;
        Product.SellingPrice = (decimal)SellingPriceNumberBox.Value;
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
