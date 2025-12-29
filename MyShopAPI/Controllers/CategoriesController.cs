using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShopAPI.Data;
using MyShopAPI.DTOs;

namespace MyShopAPI.Controllers
{
    /// <summary>
    /// Controller for managing product categories.
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
                    .OrderBy(c => c.Name)
                    .Select(c => new CategoryDto
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name,
                        Description = c.Description
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
    }
}
