using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MyShop.Services.Orders;

/// <summary>
/// Service for managing order draft in local storage.
/// Auto-saves cart data when navigating away without completing order.
/// Uses file-based storage to work with both packaged and unpackaged apps.
/// </summary>
public class OrderDraftService
{
    private const string DraftFileName = "OrderDraft.json";
    private readonly string _draftFilePath;

    public OrderDraftService()
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
    /// Cart item data for draft storage.
    /// </summary>
    public class CartItemDraft
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Order draft data structure.
    /// </summary>
    public class OrderDraft
    {
        public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public List<CartItemDraft> CartItems { get; set; } = new();
        public string? CouponCode { get; set; }
        public DateTime SavedAt { get; set; }
    }

    /// <summary>
    /// Saves an order draft to file.
    /// </summary>
    public void SaveDraft(OrderDraft draft)
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
    /// Loads an order draft from file.
    /// Returns null if no draft exists.
    /// </summary>
    public OrderDraft? LoadDraft()
    {
        try
        {
            if (File.Exists(_draftFilePath))
            {
                var json = File.ReadAllText(_draftFilePath);
                return JsonSerializer.Deserialize<OrderDraft>(json);
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
    /// Should be called after successful order creation.
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
    /// Checks if the draft has any meaningful data.
    /// </summary>
    public bool HasMeaningfulData(OrderDraft draft)
    {
        return draft.CustomerId.HasValue ||
               draft.CartItems.Count > 0 ||
               !string.IsNullOrWhiteSpace(draft.CouponCode);
    }
}
