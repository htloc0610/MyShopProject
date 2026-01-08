using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShopAPI.Data;
using MyShopAPI.Models;
using MyShopAPI.Services;

namespace MyShopAPI.Controllers;

/// <summary>
/// Controller for managing discount codes.
/// All operations are scoped to the authenticated user's shop.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DiscountsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<DiscountsController> _logger;

    public DiscountsController(
        AppDbContext context,
        IUserContextService userContextService,
        ILogger<DiscountsController> logger)
    {
        _context = context;
        _userContextService = userContextService;
        _logger = logger;
    }

    // ====================================================
    // GET: api/discounts?page=1&pageSize=10
    // Get discounts for the current user with pagination
    // ====================================================
    [HttpGet]
    public async Task<ActionResult<object>> GetDiscounts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.Discounts
                .OrderByDescending(d => d.StartDate);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var discounts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                items = discounts,
                currentPage = page,
                pageSize = pageSize,
                totalCount = totalCount,
                totalPages = totalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching discounts");
            return StatusCode(500, "An error occurred while fetching discounts");
        }
    }

    // ====================================================
    // GET: api/discounts/5
    // Get a specific discount by ID
    // ====================================================
    [HttpGet("{id}")]
    public async Task<ActionResult<Discount>> GetDiscount(int id)
    {
        try
        {
            var discount = await _context.Discounts.FindAsync(id);

            if (discount == null)
            {
                return NotFound(new { message = "Discount not found" });
            }

            return Ok(discount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching discount {DiscountId}", id);
            return StatusCode(500, "An error occurred while fetching the discount");
        }
    }

    // ====================================================
    // GET: api/discounts/validate/CODE123
    // Validate a discount code (check if it's active and available)
    // ====================================================
    [HttpGet("validate/{code}")]
    public async Task<ActionResult<object>> ValidateDiscountCode(string code)
    {
        try
        {
            var discount = await _context.Discounts
                .FirstOrDefaultAsync(d => d.Code == code);

            if (discount == null)
            {
                return NotFound(new
                {
                    isValid = false,
                    message = "Discount code not found"
                });
            }

            var now = DateTime.UtcNow;
            var isExpired = now > discount.EndDate || now < discount.StartDate;
            var limitReached = discount.UsageLimit.HasValue && 
                             discount.UsedCount >= discount.UsageLimit.Value;

            if (isExpired)
            {
                return Ok(new
                {
                    isValid = false,
                    message = "Discount code has expired"
                });
            }

            if (limitReached)
            {
                return Ok(new
                {
                    isValid = false,
                    message = "Discount code usage limit reached"
                });
            }

            return Ok(new
            {
                isValid = true,
                message = "Discount code is valid",
                discount = new
                {
                    discount.DiscountId,
                    discount.Code,
                    discount.Amount,
                    discount.Description
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating discount code {Code}", code);
            return StatusCode(500, "An error occurred while validating the discount code");
        }
    }

    // ====================================================
    // POST: api/discounts
    // Create a new discount
    // ====================================================
    [HttpPost]
    public async Task<ActionResult<Discount>> CreateDiscount(Discount discount)
    {
        try
        {
            var userId = _userContextService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // Validation
            if (discount.Amount <= 0)
            {
                return BadRequest(new { message = "Amount must be greater than 0" });
            }

            if (discount.EndDate <= discount.StartDate)
            {
                return BadRequest(new { message = "End date must be after start date" });
            }

            // Check if code already exists for this user
            var existingCode = await _context.Discounts
                .AnyAsync(d => d.Code == discount.Code);

            if (existingCode)
            {
                return BadRequest(new { message = "A discount with this code already exists" });
            }

            // Set ownership
            discount.UserId = userId;
            discount.UsedCount = 0;

            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created discount {Code} for user {UserId}", 
                discount.Code, userId);

            return CreatedAtAction(nameof(GetDiscount), 
                new { id = discount.DiscountId }, discount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating discount");
            return StatusCode(500, "An error occurred while creating the discount");
        }
    }

    // ====================================================
    // PUT: api/discounts/5
    // Update an existing discount
    // ====================================================
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDiscount(int id, Discount discount)
    {
        if (id != discount.DiscountId)
        {
            return BadRequest(new { message = "Discount ID mismatch" });
        }

        try
        {
            var existingDiscount = await _context.Discounts.FindAsync(id);
            if (existingDiscount == null)
            {
                return NotFound(new { message = "Discount not found" });
            }

            // Validation
            if (discount.Amount <= 0)
            {
                return BadRequest(new { message = "Amount must be greater than 0" });
            }

            if (discount.EndDate <= discount.StartDate)
            {
                return BadRequest(new { message = "End date must be after start date" });
            }

            // Check if code exists for another discount
            var codeExists = await _context.Discounts
                .AnyAsync(d => d.Code == discount.Code && d.DiscountId != id);

            if (codeExists)
            {
                return BadRequest(new { message = "A discount with this code already exists" });
            }

            // Update fields (preserve UserId and UsedCount)
            existingDiscount.Code = discount.Code;
            existingDiscount.Description = discount.Description;
            existingDiscount.Amount = discount.Amount;
            existingDiscount.StartDate = discount.StartDate;
            existingDiscount.EndDate = discount.EndDate;
            existingDiscount.UsageLimit = discount.UsageLimit;
            existingDiscount.IsActive = discount.IsActive;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated discount {DiscountId}", id);

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await DiscountExists(id))
            {
                return NotFound(new { message = "Discount not found" });
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating discount {DiscountId}", id);
            return StatusCode(500, "An error occurred while updating the discount");
        }
    }

    // ====================================================
    // DELETE: api/discounts/5
    // Delete a discount
    // ====================================================
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDiscount(int id)
    {
        try
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
            {
                return NotFound(new { message = "Discount not found" });
            }

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted discount {DiscountId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting discount {DiscountId}", id);
            return StatusCode(500, "An error occurred while deleting the discount");
        }
    }

    // ====================================================
    // Helper Methods
    // ====================================================
    private async Task<bool> DiscountExists(int id)
    {
        return await _context.Discounts.AnyAsync(e => e.DiscountId == id);
    }
}
