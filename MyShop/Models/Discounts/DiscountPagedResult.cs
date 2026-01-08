using System.Collections.Generic;

namespace MyShop.Models.Discounts;

/// <summary>
/// Represents a paged result for discounts.
/// </summary>
public class DiscountPagedResult
{
    public List<Discount> Items { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
