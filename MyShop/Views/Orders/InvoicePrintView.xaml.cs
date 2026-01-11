using Microsoft.UI.Xaml.Controls;
using MyShop.Models.Orders;
using System;

namespace MyShop.Views.Orders;

/// <summary>
/// UserControl for displaying a printer-friendly invoice.
/// Designed for A4 paper with white background and black text.
/// </summary>
public sealed partial class InvoicePrintView : UserControl
{
    public InvoicePrintView()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Populate the invoice with order data.
    /// </summary>
    /// <param name="order">The order details to display</param>
    /// <param name="shopName">The name of the shop</param>
    public void SetOrderData(OrderDetail order, string shopName)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        // Shop name
        ShopNameText.Text = string.IsNullOrEmpty(shopName) ? "MY SHOP" : shopName.ToUpper();

        // Header information
        OrderIdText.Text = $"Đơn hàng #{order.OrderId}";
        OrderDateText.Text = order.FormattedOrderDate;

        // Customer information
        CustomerNameText.Text = order.CustomerName;
        CustomerPhoneText.Text = order.CustomerPhone ?? "N/A";
        CustomerAddressText.Text = order.CustomerAddress ?? "N/A";

        // Items
        ItemsListControl.ItemsSource = order.Items;

        // Totals
        SubtotalText.Text = order.FormattedTotalAmount;
        FinalTotalText.Text = order.FormattedFinalAmount;

        // Discount (show only if applicable)
        if (!string.IsNullOrEmpty(order.CouponCode) && order.DiscountAmount > 0)
        {
            DiscountLabelText.Text = $"Giảm giá ({order.CouponCode}):";
            DiscountLabelText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            DiscountText.Text = order.FormattedDiscountAmount;
            DiscountText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }
        else
        {
            DiscountLabelText.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            DiscountText.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
    }
}
