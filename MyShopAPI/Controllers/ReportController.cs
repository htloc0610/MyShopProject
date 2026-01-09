using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShopAPI.Data;
using MyShopAPI.DTOs;

namespace MyShopAPI.Controllers
{
    [Route("api/reports")]
    [ApiController]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _context;

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

            var items = await query
                .GroupBy(x => new
                {
                    x.ProductId,
                    x.Product.Name
                })
                .Select(g => new ProductRevenueProfitSummaryItemDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    Revenue = g.Sum(x => x.TotalPrice),
                    Profit = g.Sum(x =>
                        ((decimal)x.UnitPrice - (decimal)x.Product.ImportPrice) * x.Quantity
                    )
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            return Ok(new ProductRevenueProfitSummaryDto
            {
                From = from,
                To = to,
                Items = items
            });
        }
    }
}
