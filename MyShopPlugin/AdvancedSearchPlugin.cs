using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;

namespace MyShopPlugin;

/// <summary>
/// Advanced Search Plugin - Provides both logic and optional custom UI
/// Can provide a pre-built UI via GetView() or let the host render based on configuration
/// </summary>
public class AdvancedSearchPlugin : ISearchPlugin
{
    private SearchFilterArgs _currentFilter;

    public string Name => "Advanced Search Plugin";
    public string Description => "Provides advanced filtering capabilities with keyword, category, and price range";

    public event EventHandler<SearchFilterArgs>? OnFilterChanged;

    public AdvancedSearchPlugin()
    {
        // Initialize with empty filter
        _currentFilter = new SearchFilterArgs();
    }

    /// <summary>
    /// Gets the current filter state
    /// </summary>
    public SearchFilterArgs GetCurrentFilter()
    {
        return new SearchFilterArgs
        {
            Keyword = _currentFilter.Keyword,
            CategoryId = _currentFilter.CategoryId,
            MinPrice = _currentFilter.MinPrice,
            MaxPrice = _currentFilter.MaxPrice
        };
    }

    /// <summary>
    /// Applies a filter and raises the OnFilterChanged event
    /// </summary>
    public void ApplyFilter(SearchFilterArgs filter)
    {
        // Validate price range
        if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue
            && filter.MinPrice > filter.MaxPrice)
        {
            throw new ArgumentException("Giá t?i thi?u không ???c l?n h?n giá t?i ?a!");
        }

        // Update current filter
        _currentFilter = new SearchFilterArgs
        {
            Keyword = filter.Keyword,
            CategoryId = filter.CategoryId,
            MinPrice = filter.MinPrice,
            MaxPrice = filter.MaxPrice
        };

        // Notify subscribers
        OnFilterChanged?.Invoke(this, _currentFilter);
    }

    /// <summary>
    /// Clears all filters
    /// </summary>
    public void ClearFilter()
    {
        _currentFilter = new SearchFilterArgs
        {
            Keyword = null,
            CategoryId = null,
            MinPrice = null,
            MaxPrice = null
        };

        OnFilterChanged?.Invoke(this, _currentFilter);
    }

    /// <summary>
    /// Tells the Host what UI elements to render for this plugin
    /// </summary>
    public PluginFilterConfiguration GetFilterConfiguration()
    {
        return new PluginFilterConfiguration
        {
            SupportsKeywordSearch = true,
            SupportsCategoryFilter = true,
            SupportsPriceRange = true,

            AvailableCategories = new List<CategoryOption>
            {
                new CategoryOption { Id = 0, Name = "T?t c?" },
                new CategoryOption { Id = 1, Name = "?i?n t?" },
                new CategoryOption { Id = 2, Name = "Th?i trang" },
                new CategoryOption { Id = 3, Name = "Th?c ph?m" },
                new CategoryOption { Id = 4, Name = "Sách" },
                new CategoryOption { Id = 5, Name = "?? gia d?ng" }
            },

            PriceConstraints = new PriceRangeConstraints
            {
                MinValue = 0,
                MaxValue = 999_999_999,
                DefaultMin = 0,
                DefaultMax = 0 // 0 means no limit
            }
        };
    }

    /// <summary>
    /// Returns a custom UI for this plugin (pure C# implementation)
    /// </summary>
    /// <param name="viewModel">Not used - plugin creates its own internal ViewModel</param>
    /// <returns>UIElement containing the plugin's custom UI</returns>
    public UIElement? GetView(object? viewModel = null)
    {
        try
        {
            // Return the pure C# UI implementation
            return new AdvancedSearchUI(this);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Error creating plugin UI: {ex.Message}");
            return null; // Fall back to host rendering
        }
    }
}
