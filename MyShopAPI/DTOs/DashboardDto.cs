namespace MyShopAPI.DTOs
{
    // ==============================
    // SUMMARY
    // ==============================
    public class DashboardSummaryDto
    {
        public int TotalProducts { get; set; }
        public int TodayOrders { get; set; }
        public decimal TodayRevenue { get; set; }
    }

    // ==============================
    // LOW STOCK PRODUCTS
    // ==============================
    public class LowStockProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    // ==============================
    // TOP SELLING PRODUCTS
    // ==============================
    public class TopSellingProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalSold { get; set; }
    }

    // ==============================
    // RECENT ORDERS
    // ==============================
    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal FinalAmount { get; set; }
    }

    // ==============================
    // REVENUE CHART
    // ==============================
    public class RevenueByDayDto
    {
        public int Day { get; set; }
        public decimal Revenue { get; set; }
    }
}
