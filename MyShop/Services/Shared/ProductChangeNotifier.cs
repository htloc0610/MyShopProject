using System;

namespace MyShop.Services.Shared;

/// <summary>
/// Service for notifying when product data changes.
/// Allows components to update their product counts when products are added/deleted.
/// </summary>
public class ProductChangeNotifier
{
    /// <summary>
    /// Event raised when products have been added, updated, or deleted.
    /// </summary>
    public event EventHandler? ProductsChanged;

    /// <summary>
    /// Notify all subscribers that products have changed.
    /// </summary>
    public void NotifyProductsChanged()
    {
        ProductsChanged?.Invoke(this, EventArgs.Empty);
    }
}
