using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Models.Reports
{
    public class ProductSalesSummaryResponse
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public List<ProductSalesSummaryItem> Items { get; set; } = new();
    }

    public class ProductSalesSummaryItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }

        public string DisplayText => $"{ProductName} - {TotalQuantity} sản phẩm";
    }
}
