using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using MyShop.ViewModels.Products;
using MyShop.Models.Products;
using MyShop.Services.Shared;
using MyShop.Services.Plugins;
using MyShop.Contracts;
using MyShop.Contracts.Search;

namespace MyShop.Views.Products;

/// <summary>
/// Page for displaying product list in DataGrid with paging and sorting.
/// Uses ProductViewModel for data management.
/// Supports dynamic plugin loading for advanced search.
/// </summary>
public sealed partial class ProductListPage : Page
{
    /// <summary>
    /// Gets the ViewModel for this page.
    /// Injected via DI container.
    /// </summary>
    public ProductViewModel ViewModel { get; }

    private ISearchPlugin? _searchPlugin;
    private bool _isPluginAvailable = false;
    private readonly PluginLoader _pluginLoader;

    public ProductListPage()
    {
        // Get ViewModel from DI container
        ViewModel = App.Current.Services.GetRequiredService<ProductViewModel>();
        
        // Initialize plugin loader
        _pluginLoader = new PluginLoader();
        
        this.InitializeComponent();
    }

    /// <summary>
    /// Called when page is navigated to.
    /// Automatically loads products with paging and tries to load plugin.
    /// </summary>
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Initialize ViewModel (load categories and products)
        await ViewModel.InitializeAsync();

        // Try to load search plugin
        LoadSearchPlugin();
    }

    /// <summary>
    /// Loads the search plugin dynamically using AssemblyLoadContext.
    /// Plugin provides logic-only, Host renders UI based on plugin configuration.
    /// If plugin provides custom UI via GetView(), use that instead.
    /// </summary>
    private void LoadSearchPlugin()
    {
        try
        {
            // Get the application directory
            var appDirectory = AppContext.BaseDirectory;
            var pluginsDirectory = Path.Combine(appDirectory, "Plugins");
            var pluginPath = Path.Combine(pluginsDirectory, "FuzzySearch.dll");

            System.Diagnostics.Debug.WriteLine($"🔍 Looking for plugin at: {pluginPath}");

            // Check if plugin file exists
            if (!File.Exists(pluginPath))
            {
                System.Diagnostics.Debug.WriteLine("Plugin file not found. Using built-in filters.");
                ShowBuiltInFilters();
                DisablePluginToggle();
                return;
            }

            // Load the plugin using PluginLoader (Logic-only, no UI)
            _searchPlugin = _pluginLoader.LoadPlugin<ISearchPlugin>(pluginPath);

            if (_searchPlugin == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load plugin.");
                ShowBuiltInFilters();
                DisablePluginToggle();
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Plugin loaded: {_searchPlugin.Name}");
            System.Diagnostics.Debug.WriteLine($"Description: {_searchPlugin.Description}");

            // Subscribe to plugin events
            _searchPlugin.OnFilterChanged += OnPluginFilterChanged;

            // Provide categories from host to plugin (loaded from API)
            if (ViewModel.Categories != null && ViewModel.Categories.Any())
            {
                var categoryOptions = ViewModel.Categories
                    .Select(c => new CategoryOption 
                    { 
                        Id = c.CategoryId, 
                        Name = c.Name 
                    })
                    .ToList();

                // Add "All" option at the beginning
                categoryOptions.Insert(0, new CategoryOption { Id = 0, Name = "Tất cả" });

                _searchPlugin.SetCategories(categoryOptions);
                System.Diagnostics.Debug.WriteLine($"Provided {categoryOptions.Count} categories to plugin");
            }

            // Try to get plugin-provided UI first
            UIElement? pluginUI = _searchPlugin.GetView();

            if (pluginUI != null)
            {
                // Use plugin's custom UI (pure C# implementation)
                System.Diagnostics.Debug.WriteLine("Using plugin-provided custom UI");
                FilterContainer.Content = pluginUI;
            }
            else
            {
                // Plugin doesn't provide UI - this shouldn't happen with current implementation
                System.Diagnostics.Debug.WriteLine("⚠️ Plugin UI not available - no fallback UI");
                ShowBuiltInFilters();
                DisablePluginToggle();
                return;
            }
            
            _isPluginAvailable = true;

            // Keep built-in filter visible by default, but enable toggle
            ShowBuiltInFilters();
            EnablePluginToggle();

            System.Diagnostics.Debug.WriteLine("Plugin UI setup complete!");
            
            // Show success notification
            ShowPluginStatus($"Plugin loaded: {_searchPlugin.Name}", true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading plugin: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            ShowBuiltInFilters();
            DisablePluginToggle();
            ShowPluginStatus($"Failed to load plugin: {ex.Message}", false);
        }
    }

    /// <summary>
    /// Handles filter changed event from the plugin.
    /// </summary>
    private void OnPluginFilterChanged(object? sender, SearchFilterArgs e)
    {
        // Call the async method without awaiting (fire and forget)
        _ = OnPluginFilterChangedAsync(sender, e);
    }

    /// <summary>
    /// Shows built-in filters when plugin is not available.
    /// </summary>
    private void ShowBuiltInFilters()
    {
        PluginFilterBorder.Visibility = Visibility.Collapsed;
        BuiltInFilterBorder.Visibility = Visibility.Visible;
        FilterModeToggle.IsChecked = false;
        UpdateToggleButtonUI(false);
        ViewModel.UseFuzzySearch = false;
    }

    /// <summary>
    /// Shows plugin filters.
    /// </summary>
    private void ShowPluginFilters()
    {
        PluginFilterBorder.Visibility = Visibility.Visible;
        BuiltInFilterBorder.Visibility = Visibility.Collapsed;
        FilterModeToggle.IsChecked = true;
        UpdateToggleButtonUI(true);
        ViewModel.UseFuzzySearch = true;
    }

    /// <summary>
    /// Enables the plugin toggle button.
    /// </summary>
    private void EnablePluginToggle()
    {
        FilterModeToggle.IsEnabled = true;
        FilterModeToggle.Opacity = 1.0;
    }

    /// <summary>
    /// Disables the plugin toggle button.
    /// </summary>
    private void DisablePluginToggle()
    {
        FilterModeToggle.IsEnabled = false;
        FilterModeToggle.Opacity = 0.5;
        ToolTipService.SetToolTip(FilterModeToggle, "Plugin not available");
    }

    /// <summary>
    /// Updates toggle button UI based on filter mode.
    /// </summary>
    private void UpdateToggleButtonUI(bool isPluginMode)
    {
        if (isPluginMode)
        {
            FilterModeIcon.Glyph = "\uE74C"; // Plugin icon
            FilterModeText.Text = "Plugin Mode";
        }
        else
        {
            FilterModeIcon.Glyph = "\uE713"; // Settings icon
            FilterModeText.Text = "Built-in Filter";
        }
    }

    /// <summary>
    /// Shows plugin status message.
    /// </summary>
    private void ShowPluginStatus(string message, bool isSuccess)
    {
        System.Diagnostics.Debug.WriteLine($"{(isSuccess ? "SUCCESS" : "ERROR")} {message}");
        // Optionally show a brief notification in the UI if needed
    }

    /// <summary>
    /// Handles toggle button click.
    /// </summary>
    private void FilterModeToggle_Click(object sender, RoutedEventArgs e)
    {
        if (!_isPluginAvailable)
        {
            FilterModeToggle.IsChecked = false;
            return;
        }

        if (FilterModeToggle.IsChecked == true)
        {
            ShowPluginFilters();
            System.Diagnostics.Debug.WriteLine("🔌 Switched to Plugin Filter mode");
        }
        else
        {
            ShowBuiltInFilters();
            System.Diagnostics.Debug.WriteLine("🔧 Switched to Built-in Filter mode");
        }
    }

    /// <summary>
    /// Handles filter changed event from the plugin.
    /// </summary>
    private async Task OnPluginFilterChangedAsync(object? sender, SearchFilterArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔍 Plugin filter changed!");
            System.Diagnostics.Debug.WriteLine($"  Keyword: {e.Keyword}");
            System.Diagnostics.Debug.WriteLine($"  CategoryId: {e.CategoryId}");
            System.Diagnostics.Debug.WriteLine($"  MinPrice: {e.MinPrice}");
            System.Diagnostics.Debug.WriteLine($"  MaxPrice: {e.MaxPrice}");
            System.Diagnostics.Debug.WriteLine($"  UseFuzzySearch: {e.UseFuzzySearch}");
            System.Diagnostics.Debug.WriteLine($"  FuzzyThreshold: {e.FuzzyThreshold}");

            // Update ViewModel with filter values from plugin
            ViewModel.SearchKeyword = e.Keyword ?? string.Empty;
            ViewModel.SelectedCategoryId = e.CategoryId;
            ViewModel.MinPrice = e.MinPrice.HasValue ? (double)e.MinPrice.Value : 0;
            ViewModel.MaxPrice = e.MaxPrice.HasValue ? (double)e.MaxPrice.Value : 0;
            ViewModel.UseFuzzySearch = e.UseFuzzySearch;
            ViewModel.FuzzyThreshold = e.FuzzyThreshold;

            // Update built-in filter UI to sync with plugin
            if (e.CategoryId.HasValue && ViewModel.Categories != null)
            {
                ViewModel.SelectedCategory = ViewModel.Categories
                    .FirstOrDefault(c => c.CategoryId == e.CategoryId.Value);
            }

            // Note: Fuzzy search is handled by the plugin's event
            // The filter arguments already include fuzzy search settings
            // Host just needs to apply them when filtering

            // Reset to page 1 and reload products
            ViewModel.CurrentPage = 1;
            await ViewModel.LoadProductsPagedCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling plugin filter: {ex.Message}");
            
            // Show error dialog
            var dialog = new ContentDialog
            {
                Title = "Lỗi",
                Content = $"Lỗi khi áp dụng bộ lọc: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }

    /// <summary>
    /// Handles double-click on DataGrid to show product details.
    /// </summary>
    private void ProductDataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (ViewModel.SelectedProduct != null)
        {
            NavigateToProductDetail(ViewModel.SelectedProduct);
        }
    }

    /// <summary>
    /// Handles View Detail button click.
    /// </summary>
    private void ViewDetailButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Product product)
        {
            NavigateToProductDetail(product);
        }
    }

    /// <summary>
    /// Navigates to product detail page.
    /// </summary>
    private void NavigateToProductDetail(Product product)
    {
        Frame.Navigate(typeof(ProductDetailPage), product);
    }

    /// <summary>
    /// Handles Delete button click with confirmation.
    /// </summary>
    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Product product)
        {
            // Show confirmation dialog
            var dialog = new ContentDialog
            {
                Title = "Xác nhận xóa sản phẩm",
                Content = $"Bạn có chắc chắn muốn xóa sản phẩm '{product.Name}' không?\n\nHành động này không thể hoàn tác.",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.DeleteProductCommand.ExecuteAsync(product);
            }
        }
    }

    /// <summary>
    /// Clean up plugin resources when page is unloaded.
    /// </summary>
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        // Note: Plugin event unsubscription is handled via reflection if needed
        // For now, we let the event subscription be garbage collected when the page is disposed
    }
}
