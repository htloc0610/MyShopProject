using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShopAPI.Data;
using MyShopAPI.DTOs;
using MyShopAPI.Helpers;

namespace MyShopAPI.Controllers
{
    [Route("api/reports")]
    [ApiController]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _context;
        private enum ReportGroupBy
        {
            Day,
            Week,
            Month,
            Year
        }

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("product-sales-summary")]
        public async Task<ActionResult<ProductSalesSummaryDto>> GetProductSalesSummary(
            DateOnly? from,
            DateOnly to
        )
        {
            var query = _context.OrderItems.AsQueryable();

            if (from.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(
                    from.Value.ToDateTime(TimeOnly.MinValue),
                    DateTimeKind.Utc);

                query = query.Where(x => x.Order.OrderDate >= fromUtc);
            }

            var toUtcExclusive = DateTime.SpecifyKind(
                to.AddDays(1).ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc);

            query = query.Where(x => x.Order.OrderDate < toUtcExclusive);
            // Only count paid orders
            query = query.Where(x => x.Order.Status == Models.OrderStatus.Paid);

            var items = await query
                .GroupBy(x => new
                {
                    x.ProductId,
                    x.Product.Name
                })
                .Select(g => new ProductSalesSummaryItemDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToListAsync();

            return Ok(new ProductSalesSummaryDto
            {
                From = from,
                To = to,
                Items = items
            });
        }

        [HttpGet("product-revenue-profit-summary")]
        public async Task<ActionResult<ProductRevenueProfitSummaryDto>> GetProductRevenueProfitSummary(
            DateOnly? from,
            DateOnly to
        )
        {
            var orderQuery = _context.Orders
                .Where(x => x.Status == Models.OrderStatus.Paid)
                .AsQueryable();

            if (from.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(
                    from.Value.ToDateTime(TimeOnly.MinValue),
                    DateTimeKind.Utc);

                orderQuery = orderQuery.Where(x => x.OrderDate >= fromUtc);
            }

            var toUtcExclusive = DateTime.SpecifyKind(
                to.AddDays(1).ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc);

            orderQuery = orderQuery.Where(x => x.OrderDate < toUtcExclusive);

            // Get orders with items for proportional discount calculation
            var orders = await orderQuery
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToListAsync();

            // Calculate proportional revenue and profit per product
            var productData = new Dictionary<int, (string Name, decimal Revenue, decimal Profit)>();

            foreach (var order in orders)
            {
                // Calculate discount ratio for this order
                var discountRatio = order.TotalAmount > 0
                    ? order.FinalAmount / order.TotalAmount
                    : 1m;

                foreach (var item in order.OrderItems)
                {
                    // Apply proportional discount to item revenue
                    var itemRevenue = item.TotalPrice * discountRatio;
                    var itemCost = (decimal)item.Product.ImportPrice * item.Quantity;
                    var itemProfit = itemRevenue - itemCost;

                    if (productData.TryGetValue(item.ProductId, out var existing))
                    {
                        productData[item.ProductId] = (existing.Name, existing.Revenue + itemRevenue, existing.Profit + itemProfit);
                    }
                    else
                    {
                        productData[item.ProductId] = (item.Product.Name, itemRevenue, itemProfit);
                    }
                }
            }

            var items = productData
                .Select(kvp => new ProductRevenueProfitSummaryItemDto
                {
                    ProductId = kvp.Key,
                    ProductName = kvp.Value.Name,
                    Revenue = kvp.Value.Revenue,
                    Profit = kvp.Value.Profit
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            return Ok(new ProductRevenueProfitSummaryDto
            {
                From = from,
                To = to,
                Items = items
            });
        }

        [HttpGet("sales-quantity-series")]
        public async Task<ActionResult<SalesTimeSeriesDto>> GetSalesQuantitySeries(
            DateOnly? from,
            DateOnly to,
            string groupBy = "day"
        )
        {
            var query = _context.OrderItems.AsQueryable();

            if (from.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(
                    from.Value.ToDateTime(TimeOnly.MinValue),
                    DateTimeKind.Utc);

                query = query.Where(x => x.Order.OrderDate >= fromUtc);
            }

            var toUtcExclusive = DateTime.SpecifyKind(
                to.AddDays(1).ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc);

            query = query.Where(x => x.Order.OrderDate < toUtcExclusive);
            query = query.Where(x => x.Order.Status == Models.OrderStatus.Paid);

            var rows = await query
                .Select(x => new { x.Order.OrderDate, x.Quantity })
                .ToListAsync();

            var groupByValue = ParseGroupBy(groupBy);

            var items = rows
                .GroupBy(x => GetPeriodStart(x.OrderDate, groupByValue))
                .Select(g => new SalesTimeSeriesItemDto
                {
                    PeriodStart = g.Key,
                    Label = FormatPeriodLabel(g.Key, groupByValue),
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderBy(x => x.PeriodStart)
                .ToList();

            return Ok(new SalesTimeSeriesDto
            {
                From = from,
                To = to,
                GroupBy = groupByValue.ToString().ToLowerInvariant(),
                Items = items
            });
        }

        [HttpGet("revenue-profit-series")]
        public async Task<ActionResult<RevenueProfitTimeSeriesDto>> GetRevenueProfitSeries(
            DateOnly? from,
            DateOnly to,
            string groupBy = "day"
        )
        {
            var orderQuery = _context.Orders.AsQueryable();

            if (from.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(
                    from.Value.ToDateTime(TimeOnly.MinValue),
                    DateTimeKind.Utc);

                orderQuery = orderQuery.Where(x => x.OrderDate >= fromUtc);
            }

            var toUtcExclusive = DateTime.SpecifyKind(
                to.AddDays(1).ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc);

            orderQuery = orderQuery.Where(x => x.OrderDate < toUtcExclusive);
            orderQuery = orderQuery.Where(x => x.Status == Models.OrderStatus.Paid);

            // Get orders with their items for profit calculation
            var orders = await orderQuery
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToListAsync();

            var groupByValue = ParseGroupBy(groupBy);

            var items = orders
                .GroupBy(o => GetPeriodStart(o.OrderDate, groupByValue))
                .Select(g => new RevenueProfitTimeSeriesItemDto
                {
                    PeriodStart = g.Key,
                    Label = FormatPeriodLabel(g.Key, groupByValue),
                    // Use FinalAmount (after discount) for revenue
                    Revenue = g.Sum(o => o.FinalAmount),
                    // Profit = FinalAmount - Total Import Cost
                    Profit = g.Sum(o => o.FinalAmount - o.OrderItems.Sum(oi => (decimal)oi.Product.ImportPrice * oi.Quantity))
                })
                .OrderBy(x => x.PeriodStart)
                .ToList();

            return Ok(new RevenueProfitTimeSeriesDto
            {
                From = from,
                To = to,
                GroupBy = groupByValue.ToString().ToLowerInvariant(),
                Items = items
            });
        }

        private static ReportGroupBy ParseGroupBy(string groupBy)
        {
            if (Enum.TryParse(groupBy, true, out ReportGroupBy value))
                return value;

            return ReportGroupBy.Day;
        }

        private static DateOnly GetPeriodStart(DateTime dateTimeUtc, ReportGroupBy groupBy)
        {
            // Convert UTC to Vietnam time (UTC+7) before extracting date
            var vietnamTime = DateTimeHelper.ToVietnam(dateTimeUtc);
            var date = DateOnly.FromDateTime(vietnamTime);

            return groupBy switch
            {
                ReportGroupBy.Day => date,
                ReportGroupBy.Week => StartOfWeek(date),
                ReportGroupBy.Month => new DateOnly(date.Year, date.Month, 1),
                ReportGroupBy.Year => new DateOnly(date.Year, 1, 1),
                _ => date
            };
        }

        private static DateOnly StartOfWeek(DateOnly date)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            var mondayBased = (dayOfWeek + 6) % 7;
            return date.AddDays(-mondayBased);
        }

        private static string FormatPeriodLabel(DateOnly periodStart, ReportGroupBy groupBy)
        {
            return groupBy switch
            {
                ReportGroupBy.Day => periodStart.ToString("dd/MM"),
                ReportGroupBy.Week => BuildWeekLabel(periodStart),
                ReportGroupBy.Month => periodStart.ToString("MM/yyyy"),
                ReportGroupBy.Year => periodStart.ToString("yyyy"),
                _ => periodStart.ToString("dd/MM")
            };
        }

        private static string BuildWeekLabel(DateOnly periodStart)
        {
            var end = periodStart.AddDays(6);
            return $"{periodStart:dd/MM}-{end:dd/MM}";
        }
    }
}
