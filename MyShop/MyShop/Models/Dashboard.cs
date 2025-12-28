using System;

namespace MyShop.Models.Dashboard;

public class DashboardSummary
{
    public int TotalProducts { get; set; }
    public int TodayOrders { get; set; }
    public decimal TodayRevenue { get; set; }

    public string TodayRevenueFormatted => $"{TodayRevenue:N0} ₫";
}

public class LowStockProduct
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }

    public string DisplayText => $"{Name} ({Count})";
}

public class TopSellingProduct
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSold { get; set; }

    public string DisplayText => $"{Name} - {TotalSold} sold";
}

public class RecentOrder
{
    public int OrderId { get; set; }
    public DateTime CreatedTime { get; set; }
    public decimal FinalPrice { get; set; }

    public string DisplayText =>
        $"Order #{OrderId} - {FinalPrice:N0} ₫";
}

public class RevenueByDay
{
    public int Day { get; set; }
    public decimal Revenue { get; set; }

    public double BarHeight { get; set; }
}
