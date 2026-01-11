namespace MyShopAPI.Models
{
    /// <summary>
    /// Enum representing the status of an order.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// New order that has been created.
        /// </summary>
        Created = 0,
        
        /// <summary>
        /// Order has been paid.
        /// </summary>
        Paid = 1,
        
        /// <summary>
        /// Order has been cancelled.
        /// </summary>
        Cancelled = 2
    }
}
