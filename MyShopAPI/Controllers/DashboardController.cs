using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShopAPI.Data;
using MyShopAPI.DTOs;
using MyShopAPI.Helpers;
using System.Diagnostics;

namespace MyShopAPI.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize] // Require authentication for all dashboard operations
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // ==============================
        // SUMMARY
        // ==============================
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
        {
            // Today in Vietnam time, converted to UTC for database query
            var todayVietnam = DateTimeHelper.Today; // e.g., 19-Jan 00:00 Vietnam
            var todayUtc = todayVietnam.AddHours(-7); // e.g., 18-Jan 17:00 UTC

            var totalProducts = await _context.Products.CountAsync();

            var todayOrders = await _context.Orders
                .CountAsync(o => o.OrderDate >= todayUtc && o.Status == Models.OrderStatus.Paid);

            var todayRevenue = await _context.Orders
                .Where(o => o.OrderDate >= todayUtc && o.Status == Models.OrderStatus.Paid)
                .SumAsync(o => (decimal?)o.FinalAmount) ?? 0;
            return Ok(new DashboardSummaryDto
            {
                TotalProducts = totalProducts,
                TodayOrders = todayOrders,
                TodayRevenue = todayRevenue
            });
        }

        // ==============================
        // LOW STOCK PRODUCTS
        // ==============================
        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<LowStockProductDto>>> GetLowStockProducts()
        {
            var products = await _context.Products
                .Where(p => p.Count < 5)
                .OrderBy(p => p.Count)
                .Take(5)
                .Select(p => new LowStockProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Count = p.Count
                })
                .ToListAsync();

            return Ok(products);
        }

        // ==============================
        // TOP SELLING PRODUCTS
        // ==============================
        [HttpGet("top-selling")]
        public async Task<ActionResult<IEnumerable<TopSellingProductDto>>> GetTopSellingProducts()
        {
            var products = await _context.OrderItems
                .Where(oi => oi.Order.Status == Models.OrderStatus.Paid)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new TopSellingProductDto
                {
                    ProductId = g.Key.ProductId,
                    Name = g.Key.Name,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            return Ok(products);
        }

        // ==============================
        // RECENT ORDERS
        // ==============================
        [HttpGet("recent-orders")]
        public async Task<ActionResult<IEnumerable<RecentOrderDto>>> GetRecentOrders()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(3)
                .Select(o => new RecentOrderDto
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    FinalAmount = o.FinalAmount
                })
                .ToListAsync();

            return Ok(orders);
        }

        // ==============================
        // MONTHLY REVENUE CHART
        // ==============================
        [HttpGet("revenue-month")]
        public async Task<ActionResult<IEnumerable<RevenueByDayDto>>> GetRevenueByDayInMonth()
        {
            try
            {
                var now = DateTimeHelper.Now;

                // Start of current month in Vietnam time, converted to UTC for query
                var startVietnam = new DateTime(now.Year, now.Month, 1, 0, 0, 0);
                var startUtc = DateTimeHelper.ToUtc(startVietnam);
                var endUtc = DateTimeHelper.ToUtc(startVietnam.AddMonths(1));

                var orders = await _context.Orders
                    .Where(o => o.OrderDate >= startUtc && o.OrderDate < endUtc && o.Status == Models.OrderStatus.Paid)
                    .ToListAsync();

                // Group by day in Vietnam timezone
                var revenue = orders
                    .GroupBy(o => DateTimeHelper.ToVietnam(o.OrderDate).Day)
                    .Select(g => new RevenueByDayDto
                    {
                        Day = g.Key,
                        Revenue = g.Sum(o => (decimal)o.FinalAmount)
                    })
                    .OrderBy(x => x.Day)
                    .ToList();

                return Ok(revenue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, new
                {
                    message = "Failed to load monthly revenue",
                    detail = ex.Message
                });
            }
        }
    }
}
