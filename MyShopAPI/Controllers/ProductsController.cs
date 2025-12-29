using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShopAPI.Data;
using MyShopAPI.DTOs;
using MyShopAPI.Mappers;
using MyShopAPI.Models;
using System.Linq.Expressions;

namespace MyShopAPI.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            AppDbContext context,
            ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get products with paging, sorting, and filtering.
        /// Server-side processing for optimal performance.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductResponseDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool isDescending = false,
            [FromQuery] string? keyword = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null)
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Max 100 items per page

            // Build query
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            // Apply filters
            query = ApplyFilters(query, keyword, categoryId, minPrice, maxPrice);

            // Apply sorting
            query = ApplySorting(query, sortBy, isDescending);

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Build result
            var result = new PagedResult<ProductResponseDto>
            {
                Items = products.Select(ProductMapper.ToDto).ToList(),
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize
            };

            return Ok(result);
        }

        /// <summary>
        /// Get product by id.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductResponseDto>> GetById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound(new { message = $"Product {id} not found" });

            return Ok(ProductMapper.ToDto(product));
        }

        /// <summary>
        /// Create new product.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> Create(Product product)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryId == product.CategoryId);

            if (!categoryExists)
                return BadRequest(new { message = "Invalid category id" });

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _context.Entry(product)
                .Reference(p => p.Category)
                .LoadAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.ProductId },
                ProductMapper.ToDto(product)
            );
        }

        /// <summary>
        /// Update product.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProductResponseDto>> Update(int id, Product updated)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound(new { message = $"Product {id} not found" });

            product.Sku = updated.Sku;
            product.Name = updated.Name;
            product.ImportPrice = updated.ImportPrice;
            product.Count = updated.Count;
            product.Description = updated.Description;
            product.CategoryId = updated.CategoryId;

            await _context.SaveChangesAsync();

            await _context.Entry(product)
                .Reference(p => p.Category)
                .LoadAsync();

            return Ok(ProductMapper.ToDto(product));
        }

        /// <summary>
        /// Delete product.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = $"Product {id} not found" });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        #region Private Helper Methods

        /// <summary>
        /// Apply filters to products query.
        /// </summary>
        private IQueryable<Product> ApplyFilters(
            IQueryable<Product> query,
            string? keyword,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice)
        {
            // Filter by keyword (search in product name)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var normalizedKeyword = keyword.ToLower().Trim();
                query = query.Where(p => p.Name.ToLower().Contains(normalizedKeyword));
            }

            // Filter by category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Filter by price range
            if (minPrice.HasValue && minPrice.Value >= 0)
            {
                query = query.Where(p => p.ImportPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue && maxPrice.Value >= 0)
            {
                query = query.Where(p => p.ImportPrice <= maxPrice.Value);
            }

            return query;
        }

        /// <summary>
        /// Apply sorting to products query.
        /// </summary>
        private IQueryable<Product> ApplySorting(
            IQueryable<Product> query,
            string? sortBy,
            bool isDescending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                // Default sorting by ProductId
                return query.OrderBy(p => p.ProductId);
            }

            // Map sort column name to property
            Expression<Func<Product, object>> sortExpression = sortBy.ToLower() switch
            {
                "id" => p => p.ProductId,
                "name" => p => p.Name,
                "price" => p => p.ImportPrice,
                "stock" => p => p.Count,
                "category" => p => p.Category!.Name,
                "sku" => p => p.Sku,
                _ => p => p.ProductId // Default
            };

            return isDescending
                ? query.OrderByDescending(sortExpression)
                : query.OrderBy(sortExpression);
        }

        #endregion
    }
}
