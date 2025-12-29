using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Helpers;
using MyShop.Models;
using MyShop.Services;
using MyShop.ViewModels;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MyShop.Views;

/// <summary>
/// Page for adding a new product.
/// Provides form to input all required product information.
/// </summary>
public sealed partial class AddProductPage : Page
{
    public ProductViewModel ViewModel { get; }
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public AddProductPage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ProductViewModel>();
        _productService = App.Current.Services.GetRequiredService<IProductService>();
        _categoryService = App.Current.Services.GetRequiredService<ICategoryService>();
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

    private async void DownloadTemplateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Create file save picker
            var savePicker = new FileSavePicker();
            
            // Get the current window's HWND
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Excel Files", new[] { ".xlsx" });
            savePicker.SuggestedFileName = $"ProductTemplate_{DateTime.Now:yyyyMMdd}";

            var file = await savePicker.PickSaveFileAsync();
            
            if (file != null)
            {
                // Create template
                ExcelHelper.CreateProductTemplate(file.Path);
                
                await ShowInfoBarAsync("Thành công", 
                    $"Đã tạo file mẫu thành công: {file.Name}", 
                    InfoBarSeverity.Success);
            }
        }
        catch (Exception ex)
        {
            await ShowDialogAsync("Lỗi", $"Lỗi khi tạo file mẫu: {ex.Message}");
        }
    }

    private async void ImportExcelButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Create file open picker
            var openPicker = new FileOpenPicker();
            
            // Get the current window's HWND
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".xlsx");

            var file = await openPicker.PickSingleFileAsync();
            
            if (file != null)
            {
                // Show loading dialog
                var loadingDialog = new ContentDialog
                {
                    Title = "Đang xử lý",
                    Content = new ProgressRing { IsActive = true, Width = 50, Height = 50 },
                    XamlRoot = XamlRoot
                };

                _ = loadingDialog.ShowAsync();

                try
                {
                    // Load categories
                    var categories = await _categoryService.GetCategoriesAsync();

                    if (!categories.Any())
                    {
                        loadingDialog.Hide();
                        await ShowDialogAsync("Lỗi", "Không tìm thấy loại sản phẩm nào trong hệ thống. Vui lòng tạo ít nhất một loại sản phẩm trước.");
                        return;
                    }

                    // Read products from Excel
                    var (products, readErrors) = await ExcelHelper.ReadProductsFromExcel(file, categories);

                    loadingDialog.Hide();

                    // Check for read errors - Now ANY error means file is rejected
                    if (readErrors.Any())
                    {
                        var errorMessage = string.Join("\n", readErrors);

                        var errorDialog = new ContentDialog
                        {
                            Title = "FILE BỊ TỪ CHỐI - Lỗi khi đọc file Excel",
                            Content = new ScrollViewer
                            {
                                Content = new TextBlock
                                {
                                    Text = errorMessage,
                                    TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                                    FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas")
                                },
                                MaxHeight = 400
                            },
                            CloseButtonText = "Đóng",
                            XamlRoot = XamlRoot
                        };

                        await errorDialog.ShowAsync();
                        return; // Stop here, don't proceed to import
                    }

                    if (!products.Any())
                    {
                        await ShowDialogAsync("Thông báo", "File không chứa dữ liệu hợp lệ nào.");
                        return;
                    }

                    // All validation passed, show confirmation
                    var confirmDialog = new ContentDialog
                    {
                        Title = "✅ File hợp lệ - Xác nhận Import",
                        Content = $"File đã được kiểm tra và hợp lệ!\n\n" +
                                 $"📊 Tổng số sản phẩm: {products.Count}\n\n" +
                                 $"Bạn có muốn import {products.Count} sản phẩm vào hệ thống?",
                        PrimaryButtonText = "Import",
                        CloseButtonText = "Hủy",
                        DefaultButton = ContentDialogButton.Primary,
                        XamlRoot = XamlRoot
                    };

                    var confirmResult = await confirmDialog.ShowAsync();

                    if (confirmResult == ContentDialogResult.Primary)
                    {
                        // Show loading again
                        _ = loadingDialog.ShowAsync();

                        // Import products
                        var (success, importedCount, importErrors) = await _productService.ImportProductsAsync(products);

                        loadingDialog.Hide();

                        if (success && importedCount > 0)
                        {
                            var message = $"✅ Đã import thành công {importedCount} sản phẩm!";
                            
                            if (importErrors.Any())
                            {
                                message += $"\n\n⚠️ Có {importErrors.Count} cảnh báo từ server:\n" + 
                                          string.Join("\n", importErrors.Take(5));
                                if (importErrors.Count > 5)
                                {
                                    message += $"\n...và {importErrors.Count - 5} cảnh báo khác";
                                }
                            }

                            await ShowDialogAsync("Import hoàn tất", message);

                            // Refresh product list
                            await ViewModel.LoadProductsPagedCommand.ExecuteAsync(null);
                        }
                        else
                        {
                            var errorMessage = "❌ Import thất bại!\n\n";
                            errorMessage += string.Join("\n", importErrors.Take(10));
                            if (importErrors.Count > 10)
                            {
                                errorMessage += $"\n\n...và {importErrors.Count - 10} lỗi khác";
                            }

                            await ShowDialogAsync("Lỗi Import", errorMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    loadingDialog.Hide();
                    await ShowDialogAsync("Lỗi", $"Lỗi khi xử lý file: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            await ShowDialogAsync("Lỗi", $"Lỗi khi chọn file: {ex.Message}");
        }
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

    private async Task ShowInfoBarAsync(string title, string message, InfoBarSeverity severity)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }
}
