namespace MyShopAPI.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public DateTime CreatedTime { get; set; } = DateTime.Now;

        public int FinalPrice { get; set; }

        // Navigation
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
