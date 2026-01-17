using System;
using System.IO;
using System.Text.Json;

namespace MyShop.Services.Products;

/// <summary>
/// Service for managing product draft in local storage.
/// Auto-saves form data when navigating away without saving.
/// Uses file-based storage to work with both packaged and unpackaged apps.
/// </summary>
public class ProductDraftService
{
    private const string DraftFileName = "ProductDraft.json";
    private readonly string _draftFilePath;

    public ProductDraftService()
    {
        // Use LocalApplicationData folder - works for both packaged and unpackaged apps
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MyShop");

        // Ensure directory exists
        Directory.CreateDirectory(appDataFolder);

        _draftFilePath = Path.Combine(appDataFolder, DraftFileName);
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
    /// Saves a product draft to file.
    /// </summary>
    public void SaveDraft(ProductDraft draft)
    {
        try
        {
            draft.SavedAt = DateTime.Now;
            var json = JsonSerializer.Serialize(draft, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_draftFilePath, json);
        }
        catch
        {
            // Silently fail - draft saving is not critical
        }
    }

    /// <summary>
    /// Loads a product draft from file.
    /// Returns null if no draft exists.
    /// </summary>
    public ProductDraft? LoadDraft()
    {
        try
        {
            if (File.Exists(_draftFilePath))
            {
                var json = File.ReadAllText(_draftFilePath);
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
            if (File.Exists(_draftFilePath))
            {
                File.Delete(_draftFilePath);
            }
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
        return File.Exists(_draftFilePath);
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
