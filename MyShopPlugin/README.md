# MyShopPlugin - Advanced Search Control

## ?? T?ng quan
Plugin tìm ki?m nâng cao cho ?ng d?ng MyShop, ???c xây d?ng v?i WinUI 3 và .NET 8.

## ??? Ki?n trúc Plugin

Plugin này implement **ISearchPlugin** interface t? MyShop ?? tích h?p li?n m?ch v?i h? th?ng.

### C?u trúc th? m?c
```
MyShopPlugin/
??? AdvancedSearchControl.xaml       # UI c?a plugin
??? AdvancedSearchControl.xaml.cs    # Code-behind implement ISearchPlugin
??? SearchPluginExample.cs           # Example code (tham kh?o)
??? README.md                        # File này
??? BUILD_INSTRUCTIONS.md            # H??ng d?n build plugin
??? MyShopPlugin.csproj              # Project file
```

## ?? Các thành ph?n chính

### 1. ISearchPlugin Interface (t? MyShop)
Interface chu?n cho search plugin:
```csharp
public interface ISearchPlugin
{
    event EventHandler<SearchFilterArgs>? OnFilterChanged;
    UIElement CreateUI();
}
```

### 2. SearchFilterArgs (t? MyShop)
Class ch?a các thu?c tính l?c s?n ph?m:
- `Keyword`: T? khóa tìm ki?m (string?)
- `CategoryId`: ID danh m?c (int?)
- `MinPrice`: Giá t?i thi?u (decimal?)
- `MaxPrice`: Giá t?i ?a (decimal?)

### 3. AdvancedSearchControl
UserControl WinUI 3 implement ISearchPlugin v?i các tính n?ng:

#### ?? Giao di?n (XAML):
- ? **TextBox**: Nh?p t? khóa tìm ki?m
- ? **ComboBox**: Ch?n lo?i s?n ph?m (?i?n t?, Th?i trang, Th?c ph?m, Sách, ?? gia d?ng)
- ? **NumberBox**: Giá t?i thi?u và giá t?i ?a
- ? **Button "L?c"**: Áp d?ng b? l?c
- ? **Button "Xóa l?c"**: Reset t?t c? b? l?c

#### ?? Code-behind:
- ? Implements `ISearchPlugin` interface
- ? Event `OnFilterChanged` ???c kích ho?t khi nh?n nút "L?c"
- ? Validation giá (min không ???c l?n h?n max)
- ? Method `CreateUI()` tr? v? chính UserControl
- ? Method `ClearFilters()` ?? reset t?t c? input

## ?? Cách s? d?ng trong MyShop

MyShop s? **t? ??ng load plugin** t? th? m?c `Plugins/` khi kh?i ??ng.

### Lu?ng ho?t ??ng:

1. **MyShop kh?i ??ng** ? Tìm file `MyShopPlugin.dll` trong th? m?c Plugins
2. **Load assembly** ? Tìm class implement `ISearchPlugin`
3. **Kh?i t?o instance** ? G?i `CreateUI()` ?? l?y UI
4. **Hi?n th? UI** ? Add vào FilterContainer
5. **Subscribe event** ? L?ng nghe `OnFilterChanged`
6. **User t??ng tác** ? Nh?p filter và click "L?c"
7. **Event triggered** ? MyShop nh?n `SearchFilterArgs`
8. **Apply filter** ? Update ViewModel và reload products

### Toggle gi?a Built-in Filter và Plugin Filter:

Trong trang Products, ng??i dùng có th?:
- Click nút **Toggle Filter Mode** (?? Settings icon)
- Chuy?n ??i gi?a:
  - **Built-in Filter**: B? l?c m?c ??nh c?a MyShop
  - **Plugin Mode**: B? l?c nâng cao t? plugin

## ?? Build Plugin

Xem file [`BUILD_INSTRUCTIONS.md`](BUILD_INSTRUCTIONS.md) ?? bi?t cách build plugin.

### Quick Build:
```bash
cd MyShopPlugin
dotnet build MyShopPlugin.csproj -p:Platform=x64
```

Plugin s? t? ??ng copy vào th? m?c c?a MyShop sau khi build.

## ?? Tính n?ng n?i b?t

1. **? Validation thông minh**: Ki?m tra giá min/max h?p l?
2. **? Null-safe**: T?t c? filter properties ??u nullable
3. **? Clean Architecture**: Tách bi?t interface và implementation
4. **? Error Dialog**: Hi?n th? l?i thân thi?n v?i ng??i dùng
5. **? WinUI 3 Native**: S? d?ng NumberBox, ComboBox chu?n WinUI 3
6. **? Auto-load**: MyShop t? ??ng load plugin khi kh?i ??ng
7. **? Hot-swappable**: Có th? toggle gi?a built-in và plugin filter

## ?? Yêu c?u

- .NET 8
- WinUI 3 (Windows App SDK 1.8+)
- Windows 10 version 1809 (build 17763) tr? lên
- Platform: **x64** (required)

## ?? Troubleshooting

### Plugin không load ???c?

1. **Ki?m tra log trong Output window**:
   ```
   ?? Looking for plugin at: [path]
   ? Assembly loaded: MyShopPlugin, Version=...
   ? Found plugin type: MyShopPlugin.AdvancedSearchControl
   ? Plugin loaded successfully!
   ```

2. **Ki?m tra file DLL có t?n t?i?**:
   ```
   MyShop\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\AppX\Plugins\MyShopPlugin.dll
   ```

3. **Rebuild plugin**:
   ```bash
   dotnet clean ..\MyShopPlugin\MyShopPlugin.csproj
   dotnet build ..\MyShopPlugin\MyShopPlugin.csproj -p:Platform=x64
   ```

4. **Restart MyShop** sau khi rebuild plugin

### Nút Toggle b? disable?

N?u nút Toggle Filter Mode b? disable (opacity = 0.5) và có tooltip "Plugin not available", có ngh?a là:
- Plugin DLL không ???c tìm th?y, ho?c
- Plugin không implement ?úng `ISearchPlugin` interface

? Ki?m tra l?i các b??c build và ??m b?o plugin reference ??n MyShop project.

## ?? Ví d? code

Xem file [`SearchPluginExample.cs`](SearchPluginExample.cs) ?? bi?t cách s? d?ng chi ti?t.

## ?? License

MIT License - T? do s? d?ng và ch?nh s?a.
