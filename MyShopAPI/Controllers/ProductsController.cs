using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShopAPI.Data;
using MyShopAPI.DTOs;
using MyShopAPI.Mappers;
using MyShopAPI.Models;
using MyShopAPI.Services;
using System.Linq.Expressions;

namespace MyShopAPI.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Authorize] // Require authentication for all product operations
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductsController> _logger;
        private readonly IUserContextService _userContextService;

        public ProductsController(
            AppDbContext context,
            ILogger<ProductsController> logger,
            IUserContextService userContextService)
        {
            _context = context;
            _logger = logger;
            _userContextService = userContextService;
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
        /// Get ALL products with optional filters (no paging).
        /// Used by client-side fuzzy search and filtering.
        /// </summary>
        [HttpGet("all")]
        public async Task<ActionResult<List<ProductResponseDto>>> GetAll(
            [FromQuery] int? categoryId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null)
        {
            // Build query with optional filters
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            // Apply filters
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue && minPrice.Value >= 0)
            {
                query = query.Where(p => p.ImportPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue && maxPrice.Value >= 0)
            {
                query = query.Where(p => p.ImportPrice <= maxPrice.Value);
            }

            // Load ALL products (no paging)
            var allProducts = await query.ToListAsync();
            var productDtos = allProducts.Select(ProductMapper.ToDto).ToList();

            _logger.LogInformation("Returning {Count} products for client-side processing", productDtos.Count);

            return Ok(productDtos);
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

            // Set UserId for data ownership
            var userId = _userContextService.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });
            
            product.UserId = userId;

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
        public async Task<ActionResult<ProductResponseDto>> Update(int id, [FromBody] ProductUpdateDto updateDto)
        {
            _logger.LogInformation("Updating product {ProductId}", id);
            _logger.LogInformation("Received data: {@UpdateDto}", updateDto);

            // Validate that IDs match
            if (id != updateDto.ProductId)
            {
                _logger.LogWarning("Product ID mismatch. URL id: {UrlId}, DTO id: {DtoId}", id, updateDto.ProductId);
                return BadRequest(new { message = "Product ID mismatch" });
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found", id);
                return NotFound(new { message = $"Product {id} not found" });
            }

            // Validate category exists
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryId == updateDto.CategoryId);

            if (!categoryExists)
            {
                _logger.LogWarning("Invalid category id: {CategoryId}", updateDto.CategoryId);
                return BadRequest(new { message = "Invalid category id" });
            }

            // Update product properties
            product.Sku = updateDto.Sku;
            product.Name = updateDto.Name;
            product.ImportPrice = (int)Math.Round(updateDto.ImportPrice); // Convert decimal to int
            product.Count = updateDto.Count;
            product.Description = updateDto.Description;
            product.CategoryId = updateDto.CategoryId;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Product {ProductId} updated successfully", id);

            // Reload category for response
            await _context.Entry(product)
                .Reference(p => p.Category)
                .LoadAsync();

            return Ok(ProductMapper.ToDto(product));
        }

        /// <summary>
        /// Delete product.
        /// Owner only - Staff cannot delete products.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "OwnerOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = $"Product {id} not found" });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Bulk import products from Excel.
        /// Validates ALL data first. If ANY error exists, rejects the entire batch.
        /// </summary>
        [HttpPost("bulk-import")]
        public async Task<ActionResult<BulkImportResult>> BulkImport([FromBody] List<ProductImportDto> products)
        {
            var result = new BulkImportResult();
            var validationErrors = new List<string>();

            try
            {
                _logger.LogInformation("Starting bulk import validation for {Count} products", products.Count);

                if (products == null || products.Count == 0)
                {
                    result.Errors.Add("Không có dữ liệu để import");
                    return BadRequest(result);
                }

                // PHASE 1: VALIDATE ALL PRODUCTS FIRST
                // We will NOT insert any product if even one has an error
                var validProducts = new List<Product>();
                var skuSet = new HashSet<string>(); // Track SKUs in this batch

                for (int i = 0; i < products.Count; i++)
                {
                    var dto = products[i];
                    var rowNumber = i + 2; // Excel row number (row 1 is header, data starts at row 2)
                    
                    try
                    {
                        // Validate required fields
                        if (string.IsNullOrWhiteSpace(dto.Name))
                        {
                            validationErrors.Add($"Sản phẩm {i + 1} (Dòng {rowNumber}): Tên không được để trống");
                            continue;                       }

                        if (string.IsNullOrWhiteSpace(dto.Sku))
                        {
                            validationErrors.Add($"Sản phẩm {i + 1} (Dòng {rowNumber}): SKU không được để trống");
                            continue;                       }

                        // Check duplicate SKU in the current batch
                        if (skuSet.Contains(dto.Sku))
                        {
                            validationErrors.Add($"Sản phẩm {i + 1} (Dòng {rowNumber}, '{dto.Name}'): SKU '{dto.Sku}' bị trùng trong file");
                            continue;                       }

                        // Check if SKU already exists in database
                        var skuExists = await _context.Products.AnyAsync(p => p.Sku == dto.Sku);
                        if (skuExists)
                        {
                            validationErrors.Add($"Sản phẩm {i + 1} (Dòng {rowNumber}, '{dto.Name}'): SKU '{dto.Sku}' đã tồn tại trong hệ thống");
                            continue;                       }

                        // Validate category exists
                        var categoryExists = await _context.Categories
                            .AnyAsync(c => c.CategoryId == dto.CategoryId);

                        if (!categoryExists)
                        {
                            validationErrors.Add($"Sản phẩm {i + 1} (Dòng {rowNumber}, '{dto.Name}'): CategoryId {dto.CategoryId} không hợp lệ");
                            continue;                       }

                        // Validate price
                        if (dto.Price <= 0)
                        {
                            validationErrors.Add($"Sản phẩm {i + 1} (Dòng {rowNumber}, '{dto.Name}'): Giá ({dto.Price}) phải lớn hơn 0");
                            continue;                       }

                        // Validate stock
                        if (dto.Stock < 0)
                        {
                            validationErrors.Add($"Sản phẩm {i + 1} (Dòng {rowNumber}, '{dto.Name}'): Số lượng ({dto.Stock}) không được âm");
                            continue;                       }

                        // Add to SKU tracking set
                        skuSet.Add(dto.Sku);

                        // Get current user ID for data ownership
                        var userId = _userContextService.GetUserId();
                        if (string.IsNullOrEmpty(userId))
                        {
                            validationErrors.Add($"User not authenticated");
                            continue;                       }

                        // Create product entity (but don't insert yet)
                        var product = new Product
                        {
                            Sku = dto.Sku,
                            Name = dto.Name,
                            ImportPrice = (int)Math.Round(dto.Price),
                            Count = dto.Stock,
                            Description = dto.Description ?? string.Empty,
                            CategoryId = dto.CategoryId,
                            UserId = userId
                        };

                        validProducts.Add(product);
                    }
                    catch (Exception ex)
                    {
                        validationErrors.Add($"Sản phẩm {i + 1} (Dòng {rowNumber}, '{dto.Name}'): Lỗi xử lý - {ex.Message}");
                        _logger.LogError(ex, "Eor processing product {Index}", i + 1);
                    }
                }

                // PHASE 2: CHECK IF ALL PRODUCTS ARE VALID
                if (validationErrors.Any())
                {
                    // Reject the entire batch
                    result.Errors.Add("BATCH BỊ TỪ CHỐI - Dữ liệu chứa lỗi");
                    result.Errors.Add($"Tổng số sản phẩm: {products.Count}");
                    result.Errors.Add($"Số lỗi phát hiện: {validationErrors.Count}");
                    result.Errors.Add($"Số sản phẩm hợp lệ: {validProducts.Count}");
                    result.Errors.Add("");
                    result.Errors.Add("CHI TIẾT LỖI:");
                    result.Errors.AddRange(validationErrors);
                    result.Errors.Add("");
                    result.Errors.Add("Vui lòng sửa TẤT CẢ các lỗi và thử lại. Không có sản phẩm nào được import.");

                    _logger.LogWarning("Bulk import rejected: {ErrorCount} validation errors found", validationErrors.Count);
                    
                    return BadRequest(result);
                }

                // PHASE 3: ALL VALID - INSERT ALL PRODUCTS
                if (validProducts.Any())
                {
                    await _context.Products.AddRangeAsync(validProducts);
                    await _context.SaveChangesAsync();
                    result.ImportedCount = validProducts.Count;
                    
                    _logger.LogInformation("Successfully imported {Count} products", validProducts.Count);
                    
                    return Ok(result);
                }
                else
                {
                    result.Errors.Add("Không có sản phẩm hợp lệ để import");
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk import");
                result.Errors.Add($"LỖI HỆ THỐNG: {ex.Message}");
                result.Errors.Add("Vui lòng liên hệ quản trị viên nếu lỗi tiếp tục xảy ra.");
                return StatusCode(500, result);
            }
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
