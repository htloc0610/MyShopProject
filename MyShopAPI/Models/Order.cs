using System.ComponentModel.DataAnnotations;

namespace MyShopAPI.Models
{
    /// <summary>
    /// Represents an order placed by a customer in the shop.
    /// </summary>
    public class Order
    {
        public int OrderId { get; set; }

        /// <summary>
        /// Date and time when the order was created.
        /// </summary>
        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Total amount before applying any discounts.
        /// </summary>
        [Required]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Final amount after applying discount (if any).
        /// </summary>
        [Required]
        public decimal FinalAmount { get; set; }

        /// <summary>
        /// Current status of the order.
        /// </summary>
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.New;

        /// <summary>
        /// Foreign key to Customer (nullable - order can be placed without customer record).
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Foreign key to Discount/Coupon (nullable - order may not use a coupon).
        /// </summary>
        public int? CouponId { get; set; }

        /// <summary>
        /// Data ownership - links order to specific user/shop.
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public Customer? Customer { get; set; }
        public Discount? Coupon { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}

