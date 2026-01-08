using System;

namespace MyShop.Contracts.Search;

/// <summary>
/// Filter arguments
/// </summary>
public class SearchFilterArgs : EventArgs
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool UseFuzzySearch { get; set; } = true;
    public int FuzzyThreshold { get; set; } = 70;
}
