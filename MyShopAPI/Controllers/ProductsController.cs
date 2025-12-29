using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShopAPI.Data;
using MyShopAPI.DTOs;
using MyShopAPI.Mappers;
using MyShopAPI.Models;

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
        /// Get all products (API output giống version cũ)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAll()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();

            return Ok(products.Select(ProductMapper.ToDto));
        }

        /// <summary>
        /// Get product by id
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
        /// Get products by category id
        /// </summary>
        [HttpGet("category/{categoryId:int}")]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetByCategory(int categoryId)
        {
            var products = await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .ToListAsync();

            return Ok(products.Select(ProductMapper.ToDto));
        }

        /// <summary>
        /// Create new product (vẫn dùng Entity để ghi DB)
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
        /// Update product
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
        /// Delete product
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
    }
}
