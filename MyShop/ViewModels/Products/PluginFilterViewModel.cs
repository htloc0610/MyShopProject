using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using MyShopPlugin;

namespace MyShop.ViewModels.Products;

/// <summary>
/// ViewModel for Plugin Filter UI (Host-rendered)
/// Binds to the XAML UI and communicates with the Plugin logic
/// </summary>
public partial class PluginFilterViewModel : ObservableObject
{
    private readonly ISearchPlugin _plugin;

    #region Observable Properties

    [ObservableProperty]
    private string _keyword = string.Empty;

    [ObservableProperty]
    private CategoryOption? _selectedCategory;

    [ObservableProperty]
    private double _minPrice;

    [ObservableProperty]
    private double _maxPrice;

    [ObservableProperty]
    private ObservableCollection<CategoryOption> _categories = new();

    // Configuration properties (controls visibility)
    [ObservableProperty]
    private Visibility _supportsKeywordSearch = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _supportsCategoryFilter = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _supportsPriceRange = Visibility.Collapsed;

    [ObservableProperty]
    private decimal _minPriceLimit;

    [ObservableProperty]
    private decimal _maxPriceLimit;

    #endregion

    public PluginFilterViewModel(ISearchPlugin plugin)
    {
        _plugin = plugin;
        
        // Load configuration from plugin
        LoadConfiguration();
        
        // Subscribe to plugin events
        _plugin.OnFilterChanged += OnPluginFilterChanged;
    }

    /// <summary>
    /// Loads filter configuration from the plugin
    /// </summary>
    private void LoadConfiguration()
    {
        var config = _plugin.GetFilterConfiguration();

        // Set visibility based on plugin capabilities
        SupportsKeywordSearch = config.SupportsKeywordSearch ? Visibility.Visible : Visibility.Collapsed;
        SupportsCategoryFilter = config.SupportsCategoryFilter ? Visibility.Visible : Visibility.Collapsed;
        SupportsPriceRange = config.SupportsPriceRange ? Visibility.Visible : Visibility.Collapsed;

        // Load categories
        if (config.SupportsCategoryFilter)
        {
            Categories.Clear();
            foreach (var category in config.AvailableCategories)
            {
                Categories.Add(category);
            }

            if (Categories.Any())
            {
                SelectedCategory = Categories.First(); // Default to "All"
            }
        }

        // Set price constraints
        if (config.PriceConstraints != null)
        {
            MinPriceLimit = config.PriceConstraints.MinValue;
            MaxPriceLimit = config.PriceConstraints.MaxValue;
            MinPrice = (double)config.PriceConstraints.DefaultMin;
            MaxPrice = (double)config.PriceConstraints.DefaultMax;
        }
    }

    /// <summary>
    /// Apply filter command
    /// </summary>
    [RelayCommand]
    private void ApplyFilter()
    {
        try
        {
            var filter = new SearchFilterArgs
            {
                Keyword = string.IsNullOrWhiteSpace(Keyword) ? null : Keyword.Trim(),
                CategoryId = SelectedCategory?.Id == 0 ? null : SelectedCategory?.Id,
                MinPrice = MinPrice > 0 ? (decimal?)MinPrice : null,
                MaxPrice = MaxPrice > 0 ? (decimal?)MaxPrice : null
            };

            _plugin.ApplyFilter(filter);
        }
        catch (Exception ex)
        {
            // Handle validation errors
            System.Diagnostics.Debug.WriteLine($"Filter validation error: {ex.Message}");
            // TODO: Show error dialog
        }
    }

    /// <summary>
    /// Clear filter command
    /// </summary>
    [RelayCommand]
    private void ClearFilter()
    {
        Keyword = string.Empty;
        SelectedCategory = Categories.FirstOrDefault();
        MinPrice = 0;
        MaxPrice = 0;

        _plugin.ClearFilter();
    }

    /// <summary>
    /// Handle filter changed event from plugin
    /// Update UI to reflect plugin state changes
    /// </summary>
    private void OnPluginFilterChanged(object? sender, SearchFilterArgs e)
    {
        // Update ViewModel from plugin state (if needed)
        Keyword = e.Keyword ?? string.Empty;
        
        if (e.CategoryId.HasValue)
        {
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == e.CategoryId.Value);
        }
        else
        {
            SelectedCategory = Categories.FirstOrDefault();
        }

        MinPrice = e.MinPrice.HasValue ? (double)e.MinPrice.Value : 0;
        MaxPrice = e.MaxPrice.HasValue ? (double)e.MaxPrice.Value : 0;
    }

    /// <summary>
    /// Clean up when disposing
    /// </summary>
    public void Dispose()
    {
        _plugin.OnFilterChanged -= OnPluginFilterChanged;
    }
}
