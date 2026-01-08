using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using MyShop.Contracts;
using MyShop.Contracts.Search;
using MyShop.Contracts.Helpers;

namespace MyShopPlugin;

/// <summary>
/// Advanced Search Plugin with fuzzy search support
/// </summary>
public class AdvancedSearchPlugin : ISearchPlugin
{
    private SearchFilterArgs _currentFilter = new();
    private List<CategoryOption> _categories = new();

    public string Name => "Advanced Search Plugin";
    public string Description => "Advanced product search with fuzzy matching and filters";
    public string Version => "1.0.0";

    public event EventHandler<SearchFilterArgs>? OnFilterChanged;

    public SearchFilterArgs GetCurrentFilter() => _currentFilter;

    public PluginFilterConfiguration GetFilterConfiguration()
    {
        return new PluginFilterConfiguration
        {
            SupportsKeywordSearch = true,
            SupportsCategoryFilter = true,
            SupportsPriceRange = true,
            EnableFuzzySearch = true,
            FuzzySearchThreshold = 70,
            AvailableCategories = _categories
        };
    }

    public void ApplyFilter(SearchFilterArgs filter)
    {
        _currentFilter = filter;
        OnFilterChanged?.Invoke(this, filter);
    }

    public void ClearFilter()
    {
        _currentFilter = new SearchFilterArgs
        {
            UseFuzzySearch = true,
            FuzzyThreshold = 70
        };
        OnFilterChanged?.Invoke(this, _currentFilter);
    }

    public void SetCategories(List<CategoryOption> categories)
    {
        _categories = categories;
    }

    public UIElement? GetView(object? viewModel = null)
    {
        // Return custom UI (ignore viewModel parameter as we use internal ViewModel)
        return new AdvancedSearchUI(this);
    }
}
