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
        New = 0,
        
        /// <summary>
        /// Order is being processed.
        /// </summary>
        Processing = 1,
        
        /// <summary>
        /// Order has been completed and delivered.
        /// </summary>
        Completed = 2,
        
        /// <summary>
        /// Order has been cancelled.
        /// </summary>
        Cancelled = 3
    }
}
