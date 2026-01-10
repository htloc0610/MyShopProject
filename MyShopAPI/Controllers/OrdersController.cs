using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShopAPI.Data;
using MyShopAPI.DTOs;
using MyShopAPI.Models;

namespace MyShopAPI.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get paginated list of orders for the current user with advanced filtering and sorting.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<OrderListDto>>> GetOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = null)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Coupon)
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(o =>
                    (o.Customer != null && o.Customer.Name.ToLower().Contains(search)) ||
                    (o.Customer != null && o.Customer.PhoneNumber != null && o.Customer.PhoneNumber.Contains(search)) ||
                    (o.Coupon != null && o.Coupon.Code.ToLower().Contains(search)));
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                query = query.Where(o => o.Status == orderStatus);
            }

            // Apply date range filter
            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                // Include the entire end date (until 23:59:59)
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.OrderDate <= endOfDay);
            }

            // Apply amount range filter
            if (minAmount.HasValue)
            {
                query = query.Where(o => o.FinalAmount >= minAmount.Value);
            }
            if (maxAmount.HasValue)
            {
                query = query.Where(o => o.FinalAmount <= maxAmount.Value);
            }

            // Apply sorting
            sortBy = sortBy?.ToLower() ?? "date";
            sortDirection = sortDirection?.ToLower() ?? "desc";

            query = sortBy switch
            {
                "date" => sortDirection == "asc" 
                    ? query.OrderBy(o => o.OrderDate) 
                    : query.OrderByDescending(o => o.OrderDate),
                "amount" => sortDirection == "asc" 
                    ? query.OrderBy(o => o.FinalAmount) 
                    : query.OrderByDescending(o => o.FinalAmount),
                "customer" => sortDirection == "asc" 
                    ? query.OrderBy(o => o.Customer != null ? o.Customer.Name : "Guest") 
                    : query.OrderByDescending(o => o.Customer != null ? o.Customer.Name : "Guest"),
                "status" => sortDirection == "asc" 
                    ? query.OrderBy(o => o.Status) 
                    : query.OrderByDescending(o => o.Status),
                _ => query.OrderByDescending(o => o.OrderDate)
            };

            // Get total count
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Get paged data
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderListDto
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    CustomerName = o.Customer != null ? o.Customer.Name : "Guest",
                    CustomerPhone = o.Customer != null ? o.Customer.PhoneNumber : null,
                    TotalAmount = o.TotalAmount,
                    DiscountAmount = o.TotalAmount - o.FinalAmount,
                    FinalAmount = o.FinalAmount,
                    ItemCount = o.OrderItems.Count,
                    CouponCode = o.Coupon != null ? o.Coupon.Code : null,
                    Status = o.Status.ToString()
                })
                .ToListAsync();

            return Ok(new PagedResultDto<OrderListDto>
            {
                Items = orders,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            });
        }

        /// <summary>
        /// Get full details of a specific order by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetailDto>> GetOrderById(int id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Coupon)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            var orderDetail = new OrderDetailDto
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                CustomerName = order.Customer != null ? order.Customer.Name : "Guest",
                CustomerPhone = order.Customer?.PhoneNumber,
                CustomerAddress = order.Customer?.Address,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.TotalAmount - order.FinalAmount,
                FinalAmount = order.FinalAmount,
                CouponCode = order.Coupon?.Code,
                Status = order.Status.ToString(),
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    OrderItemId = oi.OrderItemId,
                    ProductName = oi.Product.Name,
                    ProductSku = oi.Product.Sku,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            };

            return Ok(orderDetail);
        }

        /// <summary>
        /// Update order information (customer details and status).
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<OrderDetailDto>> UpdateOrder(int id, [FromBody] UpdateOrderDto updateDto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Coupon)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            // Prevent editing completed or cancelled orders
            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            {
                return BadRequest(new { message = $"Cannot update order with status '{order.Status}'. Order is already finalized." });
            }

            // Update customer info (if customer exists)
            if (order.Customer != null)
            {
                order.Customer.Name = updateDto.CustomerName;
                order.Customer.PhoneNumber = updateDto.CustomerPhone;
                order.Customer.Address = updateDto.CustomerAddress;
            }

            // Update order status and handle TotalSpent changes
            var oldStatus = order.Status; // Track old status
            
            if (Enum.TryParse<OrderStatus>(updateDto.Status, true, out var newStatus))
            {
                order.Status = newStatus;
                
                // If status changed to Cancelled, reduce customer's TotalSpent AND restore stock
                if (oldStatus != OrderStatus.Cancelled && newStatus == OrderStatus.Cancelled)
                {
                    // Restore product stock
                    foreach (var item in order.OrderItems)
                    {
                        var product = await _context.Products
                            .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
                        
                        if (product != null)
                        {
                            product.Count += item.Quantity;
                        }
                    }
                    
                    // Reduce customer's TotalSpent
                    if (order.CustomerId.HasValue)
                    {
                        var customer = await _context.Customers
                            .FirstOrDefaultAsync(c => c.Id == order.CustomerId.Value);
                        
                        if (customer != null)
                        {
                            customer.TotalSpent -= (long)order.FinalAmount;
                            // Ensure it doesn't go negative
                            if (customer.TotalSpent < 0)
                                customer.TotalSpent = 0;
                        }
                    }
                }
                // If status changed FROM Cancelled to something else, restore TotalSpent AND decrement stock
                else if (oldStatus == OrderStatus.Cancelled && newStatus != OrderStatus.Cancelled)
                {
                    // Decrement product stock (order is active again)
                    foreach (var item in order.OrderItems)
                    {
                        var product = await _context.Products
                            .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
                        
                        if (product != null)
                        {
                            // Check if enough stock available
                            if (product.Count < item.Quantity)
                            {
                                return BadRequest(new { message = $"Insufficient stock for product '{product.Name}' to un-cancel order. Available: {product.Count}, Required: {item.Quantity}" });
                            }
                            
                            product.Count -= item.Quantity;
                        }
                    }
                    
                    // Restore customer's TotalSpent
                    if (order.CustomerId.HasValue)
                    {
                        var customer = await _context.Customers
                            .FirstOrDefaultAsync(c => c.Id == order.CustomerId.Value);
                        
                        if (customer != null)
                        {
                            customer.TotalSpent += (long)order.FinalAmount;
                        }
                    }
                }
            }
            else
            {
                return BadRequest(new { message = $"Invalid status value: {updateDto.Status}" });
            }

            await _context.SaveChangesAsync();

            // Return updated order detail
            var orderDetail = new OrderDetailDto
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                CustomerName = order.Customer != null ? order.Customer.Name : "Guest",
                CustomerPhone = order.Customer?.PhoneNumber,
                CustomerAddress = order.Customer?.Address,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.TotalAmount - order.FinalAmount,
                FinalAmount = order.FinalAmount,
                CouponCode = order.Coupon?.Code,
                Status = order.Status.ToString(),
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    OrderItemId = oi.OrderItemId,
                    ProductName = oi.Product.Name,
                    ProductSku = oi.Product.Sku,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            };

            return Ok(orderDetail);
        }

        /// <summary>
        /// Preview order totals with optional coupon code before confirming order.
        /// </summary>
        [HttpPost("preview")]
        public async Task<ActionResult<OrderPreviewResponseDto>> PreviewOrder([FromBody] OrderPreviewRequestDto request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest(new { message = "Order must contain at least one item" });
            }

            // Step 1: Validate products and calculate total amount
            decimal totalAmount = 0;
            foreach (var item in request.Items)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                if (product == null)
                {
                    return BadRequest(new { message = $"Product with ID {item.ProductId} not found" });
                }

                if (item.Quantity <= 0)
                {
                    return BadRequest(new { message = "Quantity must be greater than 0" });
                }

                // Calculate item total using ImportPrice as base
                // In real scenario, you might have a SalePrice field
                totalAmount += product.ImportPrice * item.Quantity;
            }

            // Step 2: Validate coupon if provided
            decimal discountAmount = 0;
            string? couponMessage = null;

            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                var coupon = await _context.Discounts
                    .FirstOrDefaultAsync(d => d.Code == request.CouponCode);

                if (coupon == null)
                {
                    couponMessage = "Coupon code not found";
                }
                else if (!coupon.IsActive)
                {
                    couponMessage = "Coupon is not active";
                }
                else if (DateTime.UtcNow < coupon.StartDate)
                {
                    couponMessage = "Coupon is not yet valid";
                }
                else if (DateTime.UtcNow > coupon.EndDate)
                {
                    couponMessage = "Coupon has expired";
                }
                else if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                {
                    couponMessage = "Coupon usage limit reached";
                }
                else
                {
                    // Coupon is valid
                    discountAmount = Math.Min(coupon.Amount, totalAmount); // Don't discount more than total
                    couponMessage = $"Coupon '{coupon.Code}' applied successfully";
                }
            }

            // Step 3: Calculate final amount
            decimal finalAmount = totalAmount - discountAmount;

            return Ok(new OrderPreviewResponseDto
            {
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                CouponMessage = couponMessage
            });
        }

        /// <summary>
        /// Checkout and create an order with transactional integrity.
        /// Validates stock, creates order, updates inventory, and increments coupon usage.
        /// </summary>
        [HttpPost("checkout")]
        public async Task<ActionResult<OrderCheckoutResponseDto>> CheckoutOrder([FromBody] OrderCheckoutRequestDto request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest(new { message = "Order must contain at least one item" });
            }

            // Get the current user ID from the authenticated user
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // Begin database transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Step 1: Validate products, check stock, and load product data
                var orderItems = new List<Models.OrderItem>();
                decimal totalAmount = 0;

                foreach (var item in request.Items)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                    if (product == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = $"Product with ID {item.ProductId} not found" });
                    }

                    if (item.Quantity <= 0)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = "Quantity must be greater than 0" });
                    }

                    // Check stock availability
                    if (product.Count < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = $"Insufficient stock for product '{product.Name}'. Available: {product.Count}, Requested: {item.Quantity}" });
                    }

                    // Calculate amounts
                    decimal unitPrice = product.ImportPrice;
                    decimal itemTotal = unitPrice * item.Quantity;
                    totalAmount += itemTotal;

                    // Prepare order item (will be saved later)
                    orderItems.Add(new Models.OrderItem
                    {
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = itemTotal
                    });

                    // Decrement stock
                    product.Count -= item.Quantity;
                }

                // Step 2: Validate and apply coupon if provided
                decimal discountAmount = 0;
                int? couponId = null;

                if (!string.IsNullOrWhiteSpace(request.CouponCode))
                {
                    var coupon = await _context.Discounts
                        .FirstOrDefaultAsync(d => d.Code == request.CouponCode);

                    if (coupon != null && coupon.IsValid)
                    {
                        discountAmount = Math.Min(coupon.Amount, totalAmount);
                        couponId = coupon.DiscountId;

                        // Increment coupon usage
                        coupon.UsedCount++;
                    }
                }

                decimal finalAmount = totalAmount - discountAmount;

                // Step 3: Create Order
                var order = new Models.Order
                {
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    FinalAmount = finalAmount,
                    Status = Models.OrderStatus.New,
                    CustomerId = request.CustomerId,
                    CouponId = couponId,
                    UserId = userId
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Save to get OrderId

                // Step 4: Create OrderItems
                foreach (var item in orderItems)
                {
                    item.OrderId = order.OrderId;
                }
                _context.OrderItems.AddRange(orderItems);
                await _context.SaveChangesAsync();

                // Step 5: Update customer's total spent
                if (request.CustomerId.HasValue)
                {
                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.Id == request.CustomerId.Value);
                    
                    if (customer != null)
                    {
                        customer.TotalSpent += (long)finalAmount;
                    }
                }

                // Commit transaction
                await transaction.CommitAsync();

                return Ok(new OrderCheckoutResponseDto
                {
                    OrderId = order.OrderId,
                    FinalAmount = finalAmount,
                    Message = "Order created successfully"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "An error occurred while processing the order", error = ex.Message });
            }
        }

        /// <summary>
        /// Get list of available/valid coupons that can be used.
        /// </summary>
        [HttpGet("available-coupons")]
        public async Task<ActionResult<List<AvailableCouponDto>>> GetAvailableCoupons()
        {
            try
            {
                var userId = GetCurrentUserId();
                var now = DateTime.UtcNow;

                var coupons = await _context.Discounts
                    .Where(d => 
                        // Filter by user ownership (null = global, or owned by current user)
                        (d.UserId == null || (userId != null && d.UserId == userId)) &&
                        // Check validity using IsValid logic
                        (d.StartDate == null || d.StartDate <= now) &&
                        (d.EndDate == null || d.EndDate >= now) &&
                        (d.UsageLimit == null || d.UsedCount < d.UsageLimit))
                    .OrderByDescending(d => d.Amount) // Best deals first
                    .Select(d => new AvailableCouponDto
                    {
                        Code = d.Code,
                        Amount = d.Amount,
                        StartDate = d.StartDate,
                        EndDate = d.EndDate,
                        UsageLimit = d.UsageLimit,
                        UsageCount = d.UsedCount // Return actual usage count
                    })
                    .ToListAsync();

                return Ok(coupons);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching available coupons", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an order by ID.
        /// Only orders with 'New' status can be deleted.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            // Only allow deletion of New orders
            if (order.Status != OrderStatus.New)
            {
                return BadRequest(new { message = $"Cannot delete order with status '{order.Status}'. Only 'New' orders can be deleted." });
            }

            // Begin transaction to restore stock
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Restore product stock
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.Count += item.Quantity;
                    }
                }

                // Delete order items first (due to foreign key)
                _context.OrderItems.RemoveRange(order.OrderItems);
                
                // Delete order
                _context.Orders.Remove(order);

                // Restore customer's total spent
                if (order.CustomerId.HasValue)
                {
                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.Id == order.CustomerId.Value);
                    
                    if (customer != null)
                    {
                        customer.TotalSpent -= (long)order.FinalAmount;
                        // Ensure it doesn't go negative
                        if (customer.TotalSpent < 0)
                            customer.TotalSpent = 0;
                    }
                }
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Order deleted successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Error deleting order", error = ex.Message });
            }
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
