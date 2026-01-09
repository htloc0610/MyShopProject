namespace MyShopAPI.Models
{
    /// <summary>
    /// Represents a line item in an order with product details at time of purchase.
    /// </summary>
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        public int Quantity { get; set; }

        /// <summary>
        /// Unit price at the time of purchase (to preserve historical pricing).
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Total price for this line item (Quantity * UnitPrice).
        /// </summary>
        public decimal TotalPrice { get; set; }

        // Foreign keys
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        // Navigation properties
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
