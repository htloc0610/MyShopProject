namespace MyShopAPI.DTOs
{
    /// <summary>
    /// Request DTO for order preview endpoint.
    /// </summary>
    public class OrderPreviewRequestDto
    {
        /// <summary>
        /// List of order items with product ID and quantity.
        /// </summary>
        public List<OrderItemRequestDto> Items { get; set; } = new();

        /// <summary>
        /// Optional coupon code to apply discount.
        /// </summary>
        public string? CouponCode { get; set; }
    }

    /// <summary>
    /// Order item for preview request.
    /// </summary>
    public class OrderItemRequestDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Response DTO for order preview showing calculated amounts.
    /// </summary>
    public class OrderPreviewResponseDto
    {
        /// <summary>
        /// Total amount before discount.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Discount amount from coupon (0 if no coupon or invalid).
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Final amount after applying discount.
        /// </summary>
        public decimal FinalAmount { get; set; }

        /// <summary>
        /// Message about coupon validation (e.g., "Coupon applied successfully", "Invalid coupon code").
        /// </summary>
        public string? CouponMessage { get; set; }
    }

    /// <summary>
    /// Request DTO for order checkout.
    /// </summary>
    public class OrderCheckoutRequestDto
    {
        /// <summary>
        /// List of order items with product ID and quantity.
        /// </summary>
        public List<OrderItemRequestDto> Items { get; set; } = new();

        /// <summary>
        /// Optional customer ID to associate with this order.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Optional coupon code to apply discount.
        /// </summary>
        public string? CouponCode { get; set; }
    }

    /// <summary>
    /// Response DTO for successful order checkout.
    /// </summary>
    public class OrderCheckoutResponseDto
    {
        /// <summary>
        /// ID of the created order.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Success message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Final amount paid after discount.
        /// </summary>
        public decimal FinalAmount { get; set; }
    }

    /// <summary>
    /// DTO for available/valid coupons.
    /// </summary>
    public class AvailableCouponDto
    {
        public string Code { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public int UsageCount { get; set; }
        
        public string FormattedAmount => $"-{Amount:N0} VNĐ";
    }
}
