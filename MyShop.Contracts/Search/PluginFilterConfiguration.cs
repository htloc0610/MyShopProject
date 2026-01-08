using System.Collections.Generic;

namespace MyShop.Contracts.Search;

/// <summary>
/// Plugin filter configuration
/// </summary>
public class PluginFilterConfiguration
{
    public bool SupportsKeywordSearch { get; set; }
    public bool SupportsCategoryFilter { get; set; }
    public bool SupportsPriceRange { get; set; }
    public bool EnableFuzzySearch { get; set; } = true;
    public int FuzzySearchThreshold { get; set; } = 70;
    public List<CategoryOption> AvailableCategories { get; set; } = new();
}
