# Toast Notification System - Hướng Dẫn Sử Dụng

## Tổng Quan

Hệ thống Toast Notification toàn cục cho phép hiển thị thông báo đẹp mắt, thread-safe ở góc dưới bên phải màn hình.

## Các Loại Toast

| Loại | Màu | Thời gian | Sử dụng khi |
|------|-----|-----------|-------------|
| **Success** 🟢 | Xanh lá | 3 giây | Thao tác thành công |
| **Error** 🔴 | Đỏ | 5 giây | Lỗi, validation thất bại |
| **Warning** 🟠 | Cam | 4 giây | Cảnh báo |
| **Info** 🔵 | Xanh dương | 3 giây | Thông tin |

## Cách Sử Dụng

### 1. Inject IToastService vào ViewModel

```csharp
public class MyViewModel : ObservableObject
{
    private readonly IToastService _toastService;

    public MyViewModel(IToastService toastService)
    {
        _toastService = toastService;
    }
}
```

### 2. Hiển Thị Toast

#### Success (Thành công)
```csharp
_toastService.ShowSuccess("Lưu dữ liệu thành công!");
```

#### Error (Lỗi)
```csharp
_toastService.ShowError("Email không được để trống!");
```

#### Warning (Cảnh báo)
```csharp
_toastService.ShowWarning("Giá bán thấp hơn giá vốn!");
```

#### Info (Thông tin)
```csharp
_toastService.ShowInfo("Đang xử lý dữ liệu...");
```

### 3. Tùy Chỉnh Thời Gian Hiển Thị

```csharp
// Hiển thị 10 giây
_toastService.ShowSuccess("Thông báo quan trọng", 10000);

// Hiển thị 2 giây
_toastService.ShowInfo("Thông báo nhanh", 2000);
```

## Ví Dụ Thực Tế

### Validation Form
```csharp
[RelayCommand]
private void SaveProduct()
{
    if (string.IsNullOrEmpty(ProductName))
    {
        _toastService.ShowError("Tên sản phẩm không được để trống!");
        return;
    }
    
    if (Price <= 0)
    {
        _toastService.ShowError("Giá phải lớn hơn 0!");
        return;
    }
    
    // Lưu thành công
    _toastService.ShowSuccess("Lưu sản phẩm thành công!");
}
```

### API Call với Try-Catch
```csharp
[RelayCommand]
private async Task LoadDataAsync()
{
    try
    {
        _toastService.ShowInfo("Đang tải dữ liệu...");
        
        var data = await _apiService.GetDataAsync();
        
        _toastService.ShowSuccess($"Đã tải {data.Count} bản ghi!");
    }
    catch (Exception ex)
    {
        _toastService.ShowError($"Lỗi: {ex.Message}");
    }
}
```

### Background Thread (Thread-Safe!)
```csharp
[RelayCommand]
private async Task ProcessDataAsync()
{
    await Task.Run(() =>
    {
        // Xử lý nặng trên background thread
        ProcessLargeFile();
        
        // An toàn gọi từ background thread!
        _toastService.ShowSuccess("Xử lý hoàn tất!");
    });
}
```

### Kiểm Tra Điều Kiện
```csharp
[RelayCommand]
private void CheckStock()
{
    if (Stock < 10)
    {
        _toastService.ShowWarning($"Sắp hết hàng! Còn {Stock} sản phẩm");
    }
    else if (Stock == 0)
    {
        _toastService.ShowError("Đã hết hàng!");
    }
    else
    {
        _toastService.ShowInfo($"Tồn kho: {Stock} sản phẩm");
    }
}
```

## Lưu Ý Quan Trọng

### ✅ Nên Làm
- Sử dụng Success cho thao tác thành công
- Sử dụng Error cho validation và lỗi
- Sử dụng Warning cho cảnh báo
- Sử dụng Info cho thông tin trung lập
- Giữ message ngắn gọn (1-2 dòng)
- Gọi từ bất kỳ thread nào (thread-safe)

### ❌ Không Nên
- Hiển thị quá nhiều toast cùng lúc
- Message quá dài (> 100 ký tự)
- Sử dụng cho thông báo quan trọng cần user action (dùng Dialog thay thế)
- Quên inject IToastService vào constructor

## Vị Trí Hiển Thị

Toast sẽ hiển thị ở **góc dưới bên phải** màn hình với:
- Margin: 10px từ cạnh phải và dưới
- MaxWidth: 400px
- Shadow effect đẹp mắt
- Auto-dismiss sau thời gian quy định

## Ví Dụ Đầy Đủ

```csharp
public partial class ProductViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private decimal _price;

    public ProductViewModel(
        IProductService productService,
        IToastService toastService)
    {
        _productService = productService;
        _toastService = toastService;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(ProductName))
        {
            _toastService.ShowError("Tên sản phẩm không được để trống!");
            return;
        }

        if (Price <= 0)
        {
            _toastService.ShowError("Giá phải lớn hơn 0!");
            return;
        }

        if (Price < 1000)
        {
            _toastService.ShowWarning("Giá sản phẩm thấp hơn 1,000 VNĐ");
        }

        try
        {
            // Lưu vào database
            await _productService.SaveAsync(new Product 
            { 
                Name = ProductName, 
                Price = Price 
            });

            _toastService.ShowSuccess("Lưu sản phẩm thành công!");
            
            // Reset form
            ProductName = string.Empty;
            Price = 0;
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"Lỗi: {ex.Message}");
        }
    }
}
```

## Tích Hợp Sẵn

Toast notification đã được tích hợp vào:
- ✅ **LoginViewModel** - Validation và thông báo đăng nhập/đăng ký
- ✅ Có thể sử dụng trong bất kỳ ViewModel nào

## Kỹ Thuật

- **Thread-Safe**: Sử dụng `DispatcherQueue.TryEnqueue()`
- **DI-Based**: Inject qua `IToastService`
- **Global**: Một control cho toàn bộ app
- **Auto-Dismiss**: Tự động ẩn sau thời gian quy định

---

**Tạo bởi**: MyShop Development Team  
**Cập nhật**: 2026-01-13
