# Plugin Architecture - WinUI 3 with Pure C# UI

## Overview

This project implements a **Plugin Architecture** for WinUI 3 applications using **pure C# code** for UI generation, avoiding XAML parsing issues when loading plugins dynamically.

### Key Features

- ? **Dynamic Plugin Loading** using `AssemblyLoadContext`
- ? **Pure C# UI Generation** - No XAML required in plugins
- ? **Clean Architecture** - Separation between Host and Plugin
- ? **Bidirectional Communication** via Events and Interfaces
- ? **Hot-Swappable** - Toggle between built-in and plugin filters

---

## Architecture

### 1. Plugin Interface (`ISearchPlugin`)

Located in: `MyShop/PluginInterfaces/ISearchPlugin.cs`

```csharp
public interface ISearchPlugin
{
    string Name { get; }
    string Description { get; }
    
    event EventHandler<SearchFilterArgs>? OnFilterChanged;
    
    SearchFilterArgs GetCurrentFilter();
    void ApplyFilter(SearchFilterArgs filter);
    void ClearFilter();
    
    PluginFilterConfiguration GetFilterConfiguration();
    UIElement? GetView(object? viewModel = null);  // ? Returns pure C# UI
}
```

### 2. Plugin Implementation

Located in: `MyShopPlugin/AdvancedSearchPlugin.cs`

- Implements `ISearchPlugin` interface
- Provides **logic-only** functionality
- Optionally returns **pure C# UI** via `GetView()`

### 3. Pure C# UI Implementation

Located in: `MyShopPlugin/AdvancedSearchUI.cs`

**Key Points:**
- ? All UI elements created programmatically (no XAML)
- ? Uses `Grid`, `StackPanel`, `TextBox`, `ComboBox`, `NumberBox`, `Button`
- ? Data binding via `SetBinding()` method
- ? Event handlers via `+=` operator
- ? Fully self-contained in plugin DLL

**Example:**

```csharp
private StackPanel CreateKeywordSection()
{
    var stack = new StackPanel { Spacing = 8 };
    
    var label = new TextBlock
    {
        Text = "T? khóa tìm ki?m",
        FontWeight = FontWeights.SemiBold
    };
    stack.Children.Add(label);
    
    var textBox = new TextBox
    {
        PlaceholderText = "Nh?p t? khóa tìm ki?m..."
    };
    
    var binding = new Binding
    {
        Source = _viewModel,
        Path = new PropertyPath(nameof(PluginUIViewModel.Keyword)),
        Mode = BindingMode.TwoWay
    };
    textBox.SetBinding(TextBox.TextProperty, binding);
    
    stack.Children.Add(textBox);
    return stack;
}
```

### 4. Host Integration

Located in: `Views/Products/ProductListPage.xaml.cs`

The host application:
1. Loads plugin from `Plugins/` directory
2. Subscribes to `OnFilterChanged` event
3. Gets UI from plugin via `GetView()`
4. If plugin UI not available, falls back to host-rendered UI

```csharp
UIElement? pluginUI = _searchPlugin.GetView();

if (pluginUI != null)
{
    // Use plugin's custom UI
    FilterContainer.Content = pluginUI;
}
else
{
    // Fall back to host-rendered UI
    var pluginViewModel = new PluginFilterViewModel(_searchPlugin);
    var hostRenderedUI = new PluginFilterUI(pluginViewModel);
    FilterContainer.Content = hostRenderedUI;
}
```

---

## Project Structure

```
MyShopProject/
??? MyShop/                          # Host Application
?   ??? PluginInterfaces/           # Shared contracts
?   ?   ??? ISearchPlugin.cs        # Plugin interface
?   ?   ??? SearchFilterArgs.cs     # Data transfer objects
?   ??? Services/Plugins/
?   ?   ??? PluginLoader.cs         # Plugin loading logic
?   ?   ??? PluginLoadContext.cs    # Assembly isolation
?   ??? ViewModels/Products/
?   ?   ??? PluginFilterViewModel.cs # Host-side ViewModel (fallback)
?   ??? Views/Products/
?   ?   ??? ProductListPage.xaml    # Main page with filter UI
?   ?   ??? ProductListPage.xaml.cs # Plugin integration
?   ?   ??? PluginFilterUI.xaml.cs  # Host-rendered UI (fallback)
?   ??? bin/.../Plugins/            # Plugin deployment directory
?
??? MyShopPlugin/                    # Plugin Project (Class Library)
    ??? AdvancedSearchPlugin.cs      # Plugin implementation
    ??? AdvancedSearchUI.cs          # ? Pure C# UI implementation
    ??? MyShopPlugin.csproj          # Plugin project file
```

---

## Why Pure C# UI?

### Problem with XAML in Dynamic Plugins

WinUI 3 has difficulty loading XAML resources (`.xaml`, `.xbf`, `.pri`) from dynamically loaded assemblies due to:
- **XamlMetadataProvider** issues
- **Resource dictionary** not being registered in host app
- **AssemblyLoadContext** isolation preventing resource access

### Solution: Pure C# Code

By creating UI entirely in C# code:
- ? **No XAML parsing** required
- ? **No resource files** needed (`.xbf`, `.pri`)
- ? **Direct control** over UI hierarchy
- ? **Full data binding** support via `SetBinding()`
- ? **Dynamic loading** works seamlessly

---

## Building the Plugin

### Prerequisites

- .NET 8.0 SDK
- Windows App SDK 1.8+
- Visual Studio 2022 (recommended)

### Build Steps

1. **Build the plugin project:**
   ```bash
   dotnet build MyShopPlugin/MyShopPlugin.csproj -c Release -p:Platform=x64
   ```

2. **Plugin auto-copies to host:**
   - The `.csproj` includes a post-build task
   - Copies `MyShopPlugin.dll` to `MyShop/bin/.../Plugins/`

3. **Run the host application:**
   - Plugin loads automatically on startup
   - Toggle between built-in and plugin filters

---

## Usage

### User Guide

1. **Start the application**
2. **Navigate to Products page**
3. **Click the toggle button** (top-right of filter section)
   - ?? Built-in Filter (default)
   - ?? Plugin Mode (if plugin loaded)
4. **Apply filters:**
   - Keyword search
   - Category selection
   - Price range
5. **Click "L?c" (Filter)** to apply

### Developer Guide

#### Creating a New Plugin

1. **Create a new Class Library** targeting `net8.0-windows10.0.19041.0`
2. **Reference** `Microsoft.WindowsAppSDK` NuGet package
3. **Implement** `ISearchPlugin` interface
4. **Create UI in C# code** (see `AdvancedSearchUI.cs` as example)
5. **Return UI** from `GetView()` method
6. **Build and copy** DLL to `Plugins/` folder

#### Example Plugin Template

```csharp
public class MyCustomPlugin : ISearchPlugin
{
    public string Name => "My Custom Plugin";
    public string Description => "Custom search functionality";
    
    public event EventHandler<SearchFilterArgs>? OnFilterChanged;
    
    public UIElement? GetView(object? viewModel = null)
    {
        // Return your pure C# UI implementation
        return new MyCustomPluginUI(this);
    }
    
    public void ApplyFilter(SearchFilterArgs filter)
    {
        // Apply filter logic
        OnFilterChanged?.Invoke(this, filter);
    }
    
    // ... implement other methods
}
```

---

## Technical Details

### Data Binding in Code

```csharp
// Two-way binding
var binding = new Binding
{
    Source = viewModel,
    Path = new PropertyPath("PropertyName"),
    Mode = BindingMode.TwoWay,
    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
};
control.SetBinding(Control.PropertyProperty, binding);
```

### Event Handling

```csharp
// Direct event subscription
button.Click += OnButtonClick;

// Lambda expression
button.Click += (s, e) => 
{
    // Handle click
};
```

### Layout Management

```csharp
// Grid layout
var grid = new Grid { RowSpacing = 12 };
grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
Grid.SetRow(element, 0);

// StackPanel
var stack = new StackPanel 
{ 
    Orientation = Orientation.Horizontal, 
    Spacing = 8 
};
stack.Children.Add(child);
```

---

## Troubleshooting

### Plugin Not Loading

**Symptoms:** Toggle button disabled, "Plugin not available" message

**Solutions:**
1. Check plugin file exists: `MyShop/bin/.../Plugins/MyShopPlugin.dll`
2. Verify plugin implements `ISearchPlugin`
3. Check Debug output for error messages
4. Ensure plugin targets correct .NET version

### UI Not Displaying

**Symptoms:** Blank filter area

**Solutions:**
1. Verify `GetView()` returns non-null `UIElement`
2. Check for exceptions in Debug output
3. Ensure UI controls are properly initialized
4. Verify data binding paths are correct

### Filter Not Working

**Symptoms:** Clicking "L?c" does nothing

**Solutions:**
1. Ensure `OnFilterChanged` event is raised
2. Check host is subscribed to event
3. Verify `SearchFilterArgs` contains correct data
4. Check ViewModel properties are updated

---

## Best Practices

### Plugin Development

1. ? **Keep UI simple** - Complex layouts may be harder to maintain
2. ? **Use data binding** - Don't manually sync UI and data
3. ? **Handle errors gracefully** - Show user-friendly messages
4. ? **Test in isolation** - Verify plugin works before integrating
5. ? **Document your API** - Help other developers understand your plugin

### Host Application

1. ? **Provide fallback UI** - In case plugin UI fails to load
2. ? **Validate plugin data** - Don't trust plugin input blindly
3. ? **Handle plugin crashes** - Isolate plugin errors from host
4. ? **Log plugin activity** - Debug output for troubleshooting
5. ? **Version compatibility** - Check plugin contract version

---

## Future Enhancements

- [ ] Plugin versioning system
- [ ] Multiple plugin support (plugin manager)
- [ ] Plugin configuration UI
- [ ] Plugin marketplace/repository
- [ ] Hot-reload capability
- [ ] Plugin sandboxing/security

---

## References

- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
- [AssemblyLoadContext Guide](https://learn.microsoft.com/en-us/dotnet/core/dependency-loading/understanding-assemblyloadcontext)
- [Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/)

---

## License

This plugin architecture is part of MyShop project.

---

## Contributors

- Initial implementation: Plugin Architecture Team
- Pure C# UI approach: To solve XAML dynamic loading issues

---

**Last Updated:** January 2026
