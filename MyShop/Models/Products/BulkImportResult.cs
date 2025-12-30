using System.Collections.Generic;

namespace MyShop.Models.Products;

/// <summary>
/// Result from bulk import operation.
/// </summary>
public class BulkImportResult
{
    public int ImportedCount { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
