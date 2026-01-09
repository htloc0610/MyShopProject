namespace MyShopAPI.DTOs
{
    public class ProductSalesSummaryItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
    }

    public class ProductSalesSummaryDto
    {
        public DateOnly? From { get; set; }
        public DateOnly To { get; set; }
        public List<ProductSalesSummaryItemDto> Items { get; set; } = new();
    }

    public class ProductRevenueProfitSummaryItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
    }

    public class ProductRevenueProfitSummaryDto
    {
        public DateOnly? From { get; set; }
        public DateOnly To { get; set; }
        public List<ProductRevenueProfitSummaryItemDto> Items { get; set; } = new();
    }
}
