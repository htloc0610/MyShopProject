namespace MyShopAPI.Models
{
    /// <summary>
    /// Represents a customer belonging to a specific shop/user.
    /// Follows ownership rule: ShopId == UserId.
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Customer's full name (required).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Customer's phone number (required).
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Customer's address (optional).
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Customer's birthday (optional).
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// Total amount spent by this customer.
        /// Acts as loyalty points / total revenue indicator.
        /// </summary>
        public long TotalSpent { get; set; } = 0;

        /// <summary>
        /// When the customer record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Foreign key to the shop owner (ApplicationUser).
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property to the user/shop owner.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;
    }
}
