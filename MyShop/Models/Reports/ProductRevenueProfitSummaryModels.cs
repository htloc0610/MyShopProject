using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Models.Reports
{
    public class ProductRevenueProfitSummaryResponse
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public List<ProductRevenueProfitSummaryItem> Items { get; set; } = new();
    }

    public class ProductRevenueProfitSummaryItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }

        public string RevenueFormatted => $"{Revenue:N0} ₫";
        public string ProfitFormatted => $"{Profit:N0} ₫";
    }
}
