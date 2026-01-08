# MyShopPlugin - Build Instructions

## ?? Cách Build Plugin

### S? d?ng Visual Studio
1. M? solution `MyShopProject.sln` trong Visual Studio
2. Ch?n configuration: **Debug** ho?c **Release**
3. Ch?n platform: **x64**
4. Right-click vào project **MyShopPlugin** ? **Build**

### S? d?ng Command Line
```bash
cd MyShopPlugin
dotnet build MyShopPlugin.csproj -p:Platform=x64
```

## ?? Output

Sau khi build thành công, plugin DLL s? ???c t? ??ng copy vào:
```
MyShop\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\AppX\Plugins\MyShopPlugin.dll
```

## ? Ki?m tra Plugin

1. Build và ch?y project **MyShop**
2. ??ng nh?p vào h? th?ng
3. Vào trang **Products**
4. Tìm nút **Toggle Filter Mode** (icon ?? Settings) ? góc trên bên ph?i b? l?c
5. N?u plugin load thành công:
   - Nút toggle s? ???c **enable** (opacity = 1.0)
   - Click vào nút ?? chuy?n ??i gi?a:
     - **Built-in Filter** (b? l?c m?c ??nh)
     - **Plugin Mode** (b? l?c nâng cao t? plugin)

## ?? Debug Plugin

?? debug plugin trong Visual Studio:

1. Set **MyShop** làm Startup Project
2. Set breakpoint trong file `AdvancedSearchControl.xaml.cs`
3. Run project (F5)
4. Khi ??n trang Products, breakpoint s? ???c trigger

## ?? C?u trúc Plugin

```
MyShopPlugin/
??? AdvancedSearchControl.xaml       # UI c?a plugin
??? AdvancedSearchControl.xaml.cs    # Code-behind implement ISearchPlugin
??? SearchPluginExample.cs           # Example code (tham kh?o)
??? README.md                        # H??ng d?n s? d?ng
??? BUILD_INSTRUCTIONS.md            # File này
??? MyShopPlugin.csproj              # Project file
```

## ?? Dependencies

Plugin ph? thu?c vào:
- **MyShop project** (?? s? d?ng `ISearchPlugin` và `SearchFilterArgs`)
- **Microsoft.WindowsAppSDK** v1.8.251106002
- **.NET 8** v?i Windows SDK 10.0.19041.0

## ?? L?u ý

- Plugin ph?i ???c build v?i platform **x64**
- Plugin s? t? ??ng copy vào th? m?c c?a MyShop sau khi build
- Khi ch?nh s?a plugin, c?n **rebuild** ?? c?p nh?t DLL
- MyShop s? t? ??ng load plugin khi kh?i ??ng
