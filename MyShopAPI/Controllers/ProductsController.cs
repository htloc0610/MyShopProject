using Microsoft.AspNetCore.Mvc;
using MyShopAPI.Models;

namespace MyShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;

        // Sample in-memory data (in real app, this would be from database)
        private static readonly List<Product> _products = new()
        {
            new Product
            {
                Id = 1,
                Name = "Laptop Dell XPS 13",
                Description = "High-performance ultrabook with 13-inch display",
                Price = 1299.99M,
                Stock = 15,
                Category = "Electronics",
                ImageUrl = "https://via.placeholder.com/200"
            },
            new Product
            {
                Id = 2,
                Name = "iPhone 15 Pro",
                Description = "Latest Apple smartphone with advanced features",
                Price = 999.99M,
                Stock = 25,
                Category = "Electronics",
                ImageUrl = "https://via.placeholder.com/200"
            },
            new Product
            {
                Id = 3,
                Name = "Sony WH-1000XM5",
                Description = "Premium noise-cancelling headphones",
                Price = 399.99M,
                Stock = 30,
                Category = "Audio",
                ImageUrl = "https://via.placeholder.com/200"
            },
            new Product
            {
                Id = 4,
                Name = "Samsung Galaxy Watch 6",
                Description = "Advanced smartwatch with health tracking",
                Price = 349.99M,
                Stock = 20,
                Category = "Wearables",
                ImageUrl = "https://via.placeholder.com/200"
            },
            new Product
            {
                Id = 5,
                Name = "iPad Air",
                Description = "Powerful tablet for work and entertainment",
                Price = 599.99M,
                Stock = 18,
                Category = "Electronics",
                ImageUrl = "https://via.placeholder.com/200"
            }
        };

        public ProductsController(ILogger<ProductsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetAll()
        {
            _logger.LogInformation("Getting all products");
            return Ok(_products);
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<Product> GetById(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {Id} not found", id);
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            _logger.LogInformation("Retrieved product {Id}", id);
            return Ok(product);
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        [HttpGet("category/{category}")]
        public ActionResult<IEnumerable<Product>> GetByCategory(string category)
        {
            var products = _products.Where(p => 
                p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            
            _logger.LogInformation("Found {Count} products in category {Category}", 
                products.Count, category);
            
            return Ok(products);
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        [HttpPost]
        public ActionResult<Product> Create([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest(new { message = "Product data is required" });
            }

            // Auto-increment ID
            product.Id = _products.Max(p => p.Id) + 1;
            product.CreatedAt = DateTime.Now;
            
            _products.Add(product);
            
            _logger.LogInformation("Created new product with ID {Id}", product.Id);
            
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        [HttpPut("{id}")]
        public ActionResult<Product> Update(int id, [FromBody] Product updatedProduct)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.Stock = updatedProduct.Stock;
            product.Category = updatedProduct.Category;
            product.ImageUrl = updatedProduct.ImageUrl;

            _logger.LogInformation("Updated product {Id}", id);
            
            return Ok(product);
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            _products.Remove(product);
            
            _logger.LogInformation("Deleted product {Id}", id);
            
            return NoContent();
        }
    }
}
