using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShopAPI.Data;
using MyShopAPI.DTOs;
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
            var today = DateTime.UtcNow.Date;

            var totalProducts = await _context.Products.CountAsync();

            var todayOrders = await _context.Orders
                .CountAsync(o => o.CreatedTime >= today);

            var todayRevenue = await _context.Orders
                .Where(o => o.CreatedTime >= today)
                .SumAsync(o => (decimal?)o.FinalPrice) ?? 0;
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
                .OrderByDescending(o => o.CreatedTime)
                .Take(3)
                .Select(o => new RecentOrderDto
                {
                    OrderId = o.OrderId,
                    CreatedTime = o.CreatedTime,
                    FinalPrice = o.FinalPrice
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
                var now = DateTime.UtcNow;

                var start = new DateTime(
                    now.Year,
                    now.Month,
                    1,
                    0, 0, 0,
                    DateTimeKind.Utc);

                var end = start.AddMonths(1);

                var revenue = await _context.Orders
                    .Where(o => o.CreatedTime >= start && o.CreatedTime < end)
                    .GroupBy(o => o.CreatedTime.Day)
                    .Select(g => new RevenueByDayDto
                    {
                        Day = g.Key,
                        Revenue = g.Sum(o => (decimal)o.FinalPrice)
                    })
                    .OrderBy(x => x.Day)
                    .ToListAsync();

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
