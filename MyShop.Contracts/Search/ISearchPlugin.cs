using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;

namespace MyShop.Contracts.Search;

/// <summary>
/// Interface for search plugin functionality
/// </summary>
public interface ISearchPlugin
{
    string Name { get; }
    string Description { get; }
    event EventHandler<SearchFilterArgs>? OnFilterChanged;
    
    SearchFilterArgs GetCurrentFilter();
    PluginFilterConfiguration GetFilterConfiguration();
    void ApplyFilter(SearchFilterArgs filter);
    void ClearFilter();
    void SetCategories(List<CategoryOption> categories);
    UIElement? GetView(object? viewModel = null);
}
