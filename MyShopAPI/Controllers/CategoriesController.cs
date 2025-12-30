using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShopAPI.Data;
using MyShopAPI.DTOs;
using MyShopAPI.Models;

namespace MyShopAPI.Controllers
{
    /// <summary>
    /// Controller for managing product categories.
    /// Provides full CRUD operations.
    /// </summary>
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            AppDbContext context,
            ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.Products)
                    .OrderBy(c => c.Name)
                    .Select(c => new CategoryDto
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name,
                        Description = c.Description,
                        ProductCount = c.Products.Count
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, new { message = "Error retrieving categories" });
            }
        }

        /// <summary>
        /// Get category by id.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDto>> GetById(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);

                if (category == null)
                    return NotFound(new { message = $"Category {id} not found" });

                var dto = new CategoryDto
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Description = category.Description,
                    ProductCount = category.Products.Count
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category {CategoryId}", id);
                return StatusCode(500, new { message = "Error retrieving category" });
            }
        }

        /// <summary>
        /// Create new category.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryCreateDto createDto)
        {
            try
            {
                // Validate name is unique
                var nameExists = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == createDto.Name.ToLower());

                if (nameExists)
                    return BadRequest(new { message = $"Category name '{createDto.Name}' already exists" });

                var category = new Category
                {
                    Name = createDto.Name,
                    Description = createDto.Description ?? string.Empty
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var dto = new CategoryDto
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Description = category.Description,
                    ProductCount = 0
                };

                _logger.LogInformation("Created category {CategoryId}: {CategoryName}", category.CategoryId, category.Name);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = category.CategoryId },
                    dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new { message = "Error creating category" });
            }
        }

        /// <summary>
        /// Update category.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] CategoryUpdateDto updateDto)
        {
            try
            {
                if (id != updateDto.CategoryId)
                    return BadRequest(new { message = "Category ID mismatch" });

                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);

                if (category == null)
                    return NotFound(new { message = $"Category {id} not found" });

                // Validate name is unique (except for current category)
                var nameExists = await _context.Categories
                    .AnyAsync(c => c.CategoryId != id && c.Name.ToLower() == updateDto.Name.ToLower());

                if (nameExists)
                    return BadRequest(new { message = $"Category name '{updateDto.Name}' already exists" });

                category.Name = updateDto.Name;
                category.Description = updateDto.Description ?? string.Empty;

                await _context.SaveChangesAsync();

                var dto = new CategoryDto
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Description = category.Description,
                    ProductCount = category.Products.Count
                };

                _logger.LogInformation("Updated category {CategoryId}: {CategoryName}", category.CategoryId, category.Name);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                return StatusCode(500, new { message = "Error updating category" });
            }
        }

        /// <summary>
        /// Delete category.
        /// Only allows deletion if no products are associated.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);

                if (category == null)
                    return NotFound(new { message = $"Category {id} not found" });

                // Check if category has products
                if (category.Products.Any())
                {
                    return BadRequest(new
                    {
                        message = $"Cannot delete category '{category.Name}' because it has {category.Products.Count} product(s)",
                        productCount = category.Products.Count
                    });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted category {CategoryId}: {CategoryName}", category.CategoryId, category.Name);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                return StatusCode(500, new { message = "Error deleting category" });
            }
        }
    }
}
