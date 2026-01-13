using System;
using System.Collections.Generic;
using System.Text.Json;
using Windows.Storage;

namespace MyShop.Services.Orders;

/// <summary>
/// Service for managing order draft in local storage.
/// Auto-saves cart data when navigating away without completing order.
/// </summary>
public class OrderDraftService
{
    private const string DraftKey = "OrderDraft";
    private readonly ApplicationDataContainer _localSettings;

    public OrderDraftService()
    {
        _localSettings = ApplicationData.Current.LocalSettings;
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
    /// Saves an order draft to local storage.
    /// </summary>
    public void SaveDraft(OrderDraft draft)
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
    /// Loads an order draft from local storage.
    /// Returns null if no draft exists.
    /// </summary>
    public OrderDraft? LoadDraft()
    {
        try
        {
            if (_localSettings.Values.TryGetValue(DraftKey, out var value) && value is string json)
            {
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
    /// Checks if the draft has any meaningful data.
    /// </summary>
    public bool HasMeaningfulData(OrderDraft draft)
    {
        return draft.CustomerId.HasValue || 
               draft.CartItems.Count > 0 ||
               !string.IsNullOrWhiteSpace(draft.CouponCode);
    }
}
