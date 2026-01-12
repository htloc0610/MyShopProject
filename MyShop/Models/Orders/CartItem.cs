using CommunityToolkit.Mvvm.ComponentModel;
using MyShop.Models.Products;

namespace MyShop.Models.Orders;

/// <summary>
/// Represents an item in the shopping cart for order creation.
/// </summary>
public partial class CartItem : ObservableObject
{
    /// <summary>
    /// The product being ordered.
    /// </summary>
    [ObservableProperty]
    private Product _product = null!;

    /// <summary>
    /// Quantity of the product.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Total))]
    [NotifyPropertyChangedFor(nameof(FormattedTotal))]
    private int _quantity = 1;

    /// <summary>
    /// Total price for this cart item (Quantity * Product.SellingPrice).
    /// </summary>
    public decimal Total => Quantity * Product.SellingPrice;

    /// <summary>
    /// Formatted total with thousand separators (e.g. "1,234,000").
    /// </summary>
    public string FormattedTotal => Total.ToString("N0");

    /// <summary>
    /// Constructor for creating a cart item.
    /// </summary>
    public CartItem(Product product, int quantity = 1)
    {
        Product = product;
        Quantity = quantity;
    }
}
