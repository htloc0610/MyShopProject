using System;
using System.Text.Json;
using Windows.Storage;

namespace MyShop.Services.Products;

/// <summary>
/// Service for managing product draft in local storage.
/// Auto-saves form data when navigating away without saving.
/// </summary>
public class ProductDraftService
{
    private const string DraftKey = "ProductDraft";
    private readonly ApplicationDataContainer _localSettings;

    public ProductDraftService()
    {
        _localSettings = ApplicationData.Current.LocalSettings;
    }

    /// <summary>
    /// Product draft data structure.
    /// </summary>
    public class ProductDraft
    {
        public string? Sku { get; set; }
        public string? Name { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int Stock { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public System.Collections.Generic.List<string> Images { get; set; } = new();
        public DateTime SavedAt { get; set; }
    }

    /// <summary>
    /// Saves a product draft to local storage.
    /// </summary>
    public void SaveDraft(ProductDraft draft)
    {
        try
        {
            draft.SavedAt = DateTime.Now;
            var json = JsonSerializer.Serialize(draft);
            _localSettings.Values[DraftKey] = json;
        }
        catch
        {
            // Silently fail - draft saving is not critical
        }
    }

    /// <summary>
    /// Loads a product draft from local storage.
    /// Returns null if no draft exists.
    /// </summary>
    public ProductDraft? LoadDraft()
    {
        try
        {
            if (_localSettings.Values.TryGetValue(DraftKey, out var value) && value is string json)
            {
                return JsonSerializer.Deserialize<ProductDraft>(json);
            }
        }
        catch
        {
            // Silently fail - return null
        }
        return null;
    }

    /// <summary>
    /// Clears the saved draft.
    /// Should be called after successful product save.
    /// </summary>
    public void ClearDraft()
    {
        try
        {
            _localSettings.Values.Remove(DraftKey);
        }
        catch
        {
            // Silently fail
        }
    }

    /// <summary>
    /// Checks if a draft exists.
    /// </summary>
    public bool HasDraft()
    {
        return _localSettings.Values.ContainsKey(DraftKey);
    }

    /// <summary>
    /// Checks if the draft has any meaningful data (not all empty).
    /// </summary>
    public bool HasMeaningfulData(ProductDraft draft)
    {
        return !string.IsNullOrWhiteSpace(draft.Name) ||
               !string.IsNullOrWhiteSpace(draft.Sku) ||
               draft.ImportPrice > 0 ||
               draft.SellingPrice > 0 ||
               draft.Stock > 0 ||
               !string.IsNullOrWhiteSpace(draft.Description) ||
               draft.CategoryId.HasValue ||
               (draft.Images != null && draft.Images.Count > 0);
    }
}
