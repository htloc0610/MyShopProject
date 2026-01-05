namespace MyShopAPI.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public int FinalPrice { get; set; }

        // Data ownership - links order to specific user/shop
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        // Navigation
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
