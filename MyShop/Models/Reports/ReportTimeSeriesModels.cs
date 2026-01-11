using System;
using System.Collections.Generic;

namespace MyShop.Models.Reports
{
    public class SalesTimeSeriesResponse
    {
        public DateOnly? From { get; set; }
        public DateOnly To { get; set; }
        public string GroupBy { get; set; } = "day";
        public List<SalesTimeSeriesItem> Items { get; set; } = new();
    }

    public class SalesTimeSeriesItem
    {
        public DateOnly PeriodStart { get; set; }
        public string Label { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
    }

    public class RevenueProfitTimeSeriesResponse
    {
        public DateOnly? From { get; set; }
        public DateOnly To { get; set; }
        public string GroupBy { get; set; } = "day";
        public List<RevenueProfitTimeSeriesItem> Items { get; set; } = new();
    }

    public class RevenueProfitTimeSeriesItem
    {
        public DateOnly PeriodStart { get; set; }
        public string Label { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
    }
}
