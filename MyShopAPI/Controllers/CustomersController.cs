using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShopAPI.Data;
using MyShopAPI.DTOs;
using MyShopAPI.Models;
using MyShopAPI.Services;

namespace MyShopAPI.Controllers
{
    /// <summary>
    /// Controller for managing customers.
    /// All operations are scoped to the current user's shop (via global query filter).
    /// </summary>
    [ApiController]
    [Route("api/customers")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CustomersController> _logger;
        private readonly IUserContextService _userContextService;

        public CustomersController(
            AppDbContext context,
            ILogger<CustomersController> logger,
            IUserContextService userContextService)
        {
            _context = context;
            _logger = logger;
            _userContextService = userContextService;
        }

        /// <summary>
        /// Get all customers with optional search and paging.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool isDescending = false)
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Customers.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var normalizedKeyword = keyword.ToLower().Trim();
                query = query.Where(c => 
                    c.Name.ToLower().Contains(normalizedKeyword) ||
                    c.PhoneNumber.Contains(normalizedKeyword));
            }

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "name" => isDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                "phone" => isDescending ? query.OrderByDescending(c => c.PhoneNumber) : query.OrderBy(c => c.PhoneNumber),
                "totalspent" => isDescending ? query.OrderByDescending(c => c.TotalSpent) : query.OrderBy(c => c.TotalSpent),
                "createdat" => isDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
                _ => query.OrderByDescending(c => c.CreatedAt) // Default: newest first
            };

            var totalCount = await query.CountAsync();

            var customers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    PhoneNumber = c.PhoneNumber,
                    Address = c.Address,
                    Birthday = c.Birthday,
                    TotalSpent = c.TotalSpent,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(new PagedResult<CustomerDto>
            {
                Items = customers,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize
            });
        }

        /// <summary>
        /// Get a single customer by ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CustomerDto>> GetById(Guid id)
        {
            var customer = await _context.Customers
                .Where(c => c.Id == id)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    PhoneNumber = c.PhoneNumber,
                    Address = c.Address,
                    Birthday = c.Birthday,
                    TotalSpent = c.TotalSpent,
                    CreatedAt = c.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (customer == null)
                return NotFound(new { message = $"Không tìm thấy khách hàng với ID {id}" });

            return Ok(customer);
        }

        /// <summary>
        /// Create a new customer.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> Create([FromBody] CustomerCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = _userContextService.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Người dùng chưa đăng nhập" });

            // Check if phone number already exists for this user
            var phoneExists = await _context.Customers
                .AnyAsync(c => c.PhoneNumber == createDto.PhoneNumber);

            if (phoneExists)
                return BadRequest(new { message = "Số điện thoại này đã tồn tại trong hệ thống" });

            // Convert Birthday to UTC if provided
            DateTime? birthdayUtc = null;
            if (createDto.Birthday.HasValue)
            {
                birthdayUtc = createDto.Birthday.Value.Kind == DateTimeKind.Utc
                    ? createDto.Birthday.Value
                    : DateTime.SpecifyKind(createDto.Birthday.Value, DateTimeKind.Utc);
            }

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                PhoneNumber = createDto.PhoneNumber,
                Address = createDto.Address,
                Birthday = birthdayUtc,
                TotalSpent = 0,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created customer {CustomerId} for user {UserId}", customer.Id, userId);

            var dto = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                Birthday = customer.Birthday,
                TotalSpent = customer.TotalSpent,
                CreatedAt = customer.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, dto);
        }

        /// <summary>
        /// Update an existing customer.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<CustomerDto>> Update(Guid id, [FromBody] CustomerUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.Id)
                return BadRequest(new { message = "ID không khớp" });

            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
                return NotFound(new { message = $"Không tìm thấy khách hàng với ID {id}" });

            // Check if phone number is being changed and if new phone already exists
            if (customer.PhoneNumber != updateDto.PhoneNumber)
            {
                var phoneExists = await _context.Customers
                    .AnyAsync(c => c.PhoneNumber == updateDto.PhoneNumber && c.Id != id);

                if (phoneExists)
                    return BadRequest(new { message = "Số điện thoại này đã tồn tại trong hệ thống" });
            }

            // Convert Birthday to UTC if provided
            DateTime? birthdayUtc = null;
            if (updateDto.Birthday.HasValue)
            {
                birthdayUtc = updateDto.Birthday.Value.Kind == DateTimeKind.Utc
                    ? updateDto.Birthday.Value
                    : DateTime.SpecifyKind(updateDto.Birthday.Value, DateTimeKind.Utc);
            }

            customer.Name = updateDto.Name;
            customer.PhoneNumber = updateDto.PhoneNumber;
            customer.Address = updateDto.Address;
            customer.Birthday = birthdayUtc;
            customer.TotalSpent = updateDto.TotalSpent;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated customer {CustomerId}", id);

            return Ok(new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                Birthday = customer.Birthday,
                TotalSpent = customer.TotalSpent,
                CreatedAt = customer.CreatedAt
            });
        }

        /// <summary>
        /// Delete a customer.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
                return NotFound(new { message = $"Không tìm thấy khách hàng với ID {id}" });

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted customer {CustomerId}", id);

            return NoContent();
        }
    }
}
