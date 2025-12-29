using System.Collections.Generic;

namespace MyShopAPI.DTOs
{
    /// <summary>
    /// Result from bulk import operation.
    /// </summary>
    public class BulkImportResult
    {
        public int ImportedCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
