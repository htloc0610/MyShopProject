namespace MyShopAPI.DTOs
{
    public class SalesTimeSeriesItemDto
    {
        public DateOnly PeriodStart { get; set; }
        public string Label { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
    }

    public class SalesTimeSeriesDto
    {
        public DateOnly? From { get; set; }
        public DateOnly To { get; set; }
        public string GroupBy { get; set; } = "day";
        public List<SalesTimeSeriesItemDto> Items { get; set; } = new();
    }

    public class RevenueProfitTimeSeriesItemDto
    {
        public DateOnly PeriodStart { get; set; }
        public string Label { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
    }

    public class RevenueProfitTimeSeriesDto
    {
        public DateOnly? From { get; set; }
        public DateOnly To { get; set; }
        public string GroupBy { get; set; } = "day";
        public List<RevenueProfitTimeSeriesItemDto> Items { get; set; } = new();
    }
}
