# MyShopPlugin - Advanced Search Plugin

A WinUI 3 plugin with **pure C# UI** (no XAML) for advanced product filtering.

## Quick Setup (After Cloning)

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 (or VS Code with C# extension)
- Windows 10/11

### Build Steps

```bash
# 1. Navigate to plugin directory
cd Plugins\FuzzySearch

# 2. Restore packages
dotnet restore

# 3. Build plugin (x64 platform)
dotnet build -c Debug -p:Platform=x64

# 4. Plugin auto-copies to MyShop/bin/.../Plugins/
```

### Verify Installation

```bash
# Check if plugin DLL exists
dir ..\MyShop\bin\x64\Release\net8.0-windows10.0.19041.0\win-x64\AppX\Plugins\FuzzySearch.dll
```

## Project Structure

```
MyShopPlugin/? AdvancedSearchPlugin.cs    # Plugin implementation? AdvancedSearchUI.cs         # Pure C# UI (no XAML)? MyShopPlugin.csproj         # Project file with auto-copy
```

## How It Works

1. **Plugin builds** ? Auto-copies to `MyShop/bin/.../Plugins/`
2. **MyShop starts** ? Loads plugin dynamically
3. **Plugin creates UI** ? Pure C# controls (TextBox, ComboBox, NumberBox, Buttons)
4. **User filters products** ? Events fire to update main app

## Features

- ? **Keyword search**
- ? **Category filtering** (6 categories)
- ? **Price range filtering** (min/max)
- ? **Pure C# UI** (solves WinUI 3 XAML loading issues)
- ? **Fuzzy Search** - Find products even with typos! ??
  - "ipone" ? finds "iPhone" ?
  - "samsng" ? finds "Samsung" ?
  - Adjustable precision (50%-95%)

## Troubleshooting

**Plugin not loading?**
```bash
# Rebuild with verbose output
dotnet build -c Release -p:Platform=x64 -v:detailed
```

**Still not working?**
- Check Visual Studio Output window ? Build
- Verify plugin path: `MyShop/bin/.../Plugins/MyShopPlugin.dll`
- Ensure both projects target `net8.0-windows10.0.19041.0`

## Architecture

This plugin uses **pure C# UI** instead of XAML because:
- WinUI 3 can't load `.xbf` files from dynamic assemblies
- No `XamlMetadataProvider` issues
- Full control over UI creation

See `../MyShop/PLUGIN_ARCHITECTURE.md` for detailed documentation.

## License

Part of MyShopProject
