using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyShopAPI.Data;
using MyShopAPI.Models;

namespace MyShopAPI.Services;

/// <summary>
/// Service for seeding the database with initial sample data.
/// Creates a demo user account and sample data.
/// </summary>
public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DatabaseSeeder> _logger;

    // Demo account credentials
    private const string DEMO_EMAIL = "demo@myshop.com";
    private const string DEMO_PASSWORD = "Demo@123";
    private const string DEMO_SHOP_NAME = "Demo Shop";

    public DatabaseSeeder(
        AppDbContext context, 
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <summary>
    /// Seeds the database with:
    /// 1. Demo user account (Owner role)
    /// 2. 3 categories with 22 products each
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Seed roles first
            await SeedRolesAsync();

            // Seed demo user
            var demoUserId = await SeedDemoUserAsync();

            if (string.IsNullOrEmpty(demoUserId))
            {
                _logger.LogWarning("Failed to create demo user. Skipping product seeding.");
                return;
            }

            // Check if there are ANY products already
            var hasProducts = await _context.Products.IgnoreQueryFilters().AnyAsync();
            
            if (hasProducts)
            {
                _logger.LogInformation("Database already contains products. Skipping product seeding.");
                return;
            }

            _logger.LogInformation("Starting product seeding for demo user...");

            // Create 3 categories for demo user
            var categories = new[]
            {
                new Category
                {
                    Name = "Electronics",
                    Description = "Latest electronic devices, gadgets, and tech accessories",
                    UserId = demoUserId
                },
                new Category
                {
                    Name = "Fashion",
                    Description = "Trendy clothing, footwear, and fashion accessories",
                    UserId = demoUserId
                },
                new Category
                {
                    Name = "Home & Living",
                    Description = "Furniture, home decoration, and kitchen appliances",
                    UserId = demoUserId
                }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created {Count} categories for demo user", categories.Length);

            // Create products for each category
            await SeedElectronicsProducts(categories[0].CategoryId, demoUserId);
            await SeedFashionProducts(categories[1].CategoryId, demoUserId);
            await SeedHomeLivingProducts(categories[2].CategoryId, demoUserId);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Database seeding completed!");
            _logger.LogInformation("========================================");
            _logger.LogInformation("DEMO ACCOUNT CREATED:");
            _logger.LogInformation("  Email: {Email}", DEMO_EMAIL);
            _logger.LogInformation("  Password: {Password}", DEMO_PASSWORD);
            _logger.LogInformation("  Role: Owner");
            _logger.LogInformation("========================================");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding database");
            throw;
        }
    }

    /// <summary>
    /// Create Owner and Staff roles if they don't exist.
    /// </summary>
    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Owner", "Staff" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
                _logger.LogInformation("Created role: {Role}", role);
            }
        }
    }

    /// <summary>
    /// Create demo user account with Owner role.
    /// </summary>
    private async Task<string?> SeedDemoUserAsync()
    {
        // Check if demo user already exists
        var existingUser = await _userManager.FindByEmailAsync(DEMO_EMAIL);
        if (existingUser != null)
        {
            _logger.LogInformation("Demo user already exists: {Email}", DEMO_EMAIL);
            return existingUser.Id;
        }

        // Create demo user
        var demoUser = new ApplicationUser
        {
            UserName = DEMO_EMAIL,
            Email = DEMO_EMAIL,
            ShopName = DEMO_SHOP_NAME,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(demoUser, DEMO_PASSWORD);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(demoUser, "Owner");
            _logger.LogInformation("Created demo user: {Email} with Owner role", DEMO_EMAIL);
            return demoUser.Id;
        }

        foreach (var error in result.Errors)
        {
            _logger.LogError("Error creating demo user: {Error}", error.Description);
        }

        return null;
    }

    private async Task SeedElectronicsProducts(int categoryId, string userId)
    {
        var products = new[]
        {
            new Product { Sku = "ELEC-001", Name = "iPhone 15 Pro Max 256GB", ImportPrice = 999, Count = 45, Description = "Latest Apple flagship with titanium design, A17 Pro chip, and advanced camera system", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-002", Name = "Samsung Galaxy S24 Ultra", ImportPrice = 899, Count = 38, Description = "Premium Android phone with S Pen, AI features, and stunning display", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-003", Name = "MacBook Air M3 15-inch", ImportPrice = 1099, Count = 25, Description = "Lightweight laptop with M3 chip, stunning Retina display, and all-day battery", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-004", Name = "Dell XPS 15 OLED", ImportPrice = 1299, Count = 18, Description = "High-performance laptop for professionals with stunning OLED display", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-005", Name = "iPad Pro 12.9-inch M2", ImportPrice = 999, Count = 32, Description = "Powerful tablet with M2 chip, ProMotion display, and Apple Pencil support", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-006", Name = "Sony WH-1000XM5 Headphones", ImportPrice = 349, Count = 67, Description = "Industry-leading noise cancellation with premium sound quality", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-007", Name = "Apple AirPods Pro 2nd Gen", ImportPrice = 199, Count = 89, Description = "Wireless earbuds with active noise cancellation and spatial audio", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-008", Name = "Samsung Galaxy Watch 6 Classic", ImportPrice = 299, Count = 41, Description = "Advanced smartwatch with comprehensive health tracking", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-009", Name = "LG OLED C3 65-inch TV", ImportPrice = 1899, Count = 12, Description = "Premium OLED TV with stunning picture quality and smart features", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-010", Name = "Sony PlayStation 5 Console", ImportPrice = 449, Count = 28, Description = "Next-gen gaming console with ray tracing and fast SSD", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-011", Name = "Xbox Series X 1TB", ImportPrice = 449, Count = 24, Description = "Powerful gaming console with Game Pass subscription benefits", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-012", Name = "Nintendo Switch OLED Model", ImportPrice = 299, Count = 55, Description = "Hybrid gaming console with vibrant OLED screen", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-013", Name = "Canon EOS R6 Mark II Camera", ImportPrice = 2199, Count = 8, Description = "Professional mirrorless camera with advanced autofocus", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-014", Name = "DJI Mini 3 Pro Drone", ImportPrice = 659, Count = 15, Description = "Compact drone with 4K camera and obstacle avoidance", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-015", Name = "GoPro Hero 12 Black", ImportPrice = 349, Count = 33, Description = "Action camera for extreme sports with 5.3K video", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-016", Name = "Bose QuietComfort 45", ImportPrice = 279, Count = 44, Description = "Comfortable noise-cancelling headphones with long battery life", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-017", Name = "Logitech MX Master 3S Mouse", ImportPrice = 89, Count = 78, Description = "Premium wireless mouse for productivity with precise scrolling", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-018", Name = "Keychron K8 Pro Keyboard", ImportPrice = 99, Count = 62, Description = "Wireless mechanical keyboard with hot-swappable switches", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-019", Name = "Samsung T7 Shield 2TB SSD", ImportPrice = 179, Count = 51, Description = "Rugged portable SSD with fast transfer speeds", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-020", Name = "Anker PowerCore 26800mAh", ImportPrice = 59, Count = 94, Description = "High-capacity portable charger for multiple devices", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-021", Name = "Ring Video Doorbell Pro 2", ImportPrice = 219, Count = 37, Description = "Smart doorbell with HD video and motion detection", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-022", Name = "Amazon Echo Show 10", ImportPrice = 219, Count = 29, Description = "Smart display with motion tracking and Alexa integration", CategoryId = categoryId, UserId = userId }
        };

        await _context.Products.AddRangeAsync(products);
        _logger.LogInformation("Seeded {Count} Electronics products", products.Length);
    }

    private async Task SeedFashionProducts(int categoryId, string userId)
    {
        var products = new[]
        {
            new Product { Sku = "FASH-001", Name = "Nike Air Max 2024 Sneakers", ImportPrice = 139, Count = 72, Description = "Iconic sneakers with Air cushioning technology and modern design", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-002", Name = "Adidas Ultraboost 23 Running Shoes", ImportPrice = 169, Count = 65, Description = "Premium running shoes with responsive Boost cushioning", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-003", Name = "Levi's 501 Original Fit Jeans", ImportPrice = 69, Count = 118, Description = "Classic straight-leg denim jeans with iconic styling", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-004", Name = "Zara Wool Blend Overcoat", ImportPrice = 129, Count = 34, Description = "Elegant winter coat perfect for formal occasions", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-005", Name = "H&M Premium Cotton T-Shirt Pack", ImportPrice = 24, Count = 205, Description = "5-pack of essential cotton t-shirts in various colors", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-006", Name = "Ralph Lauren Classic Polo Shirt", ImportPrice = 79, Count = 87, Description = "Timeless polo shirt with signature pony logo", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-007", Name = "The North Face Waterproof Jacket", ImportPrice = 219, Count = 43, Description = "Durable outdoor jacket with advanced waterproof technology", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-008", Name = "Ray-Ban Aviator Classic Sunglasses", ImportPrice = 139, Count = 91, Description = "Iconic aviator-style sunglasses with UV protection", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-009", Name = "Michael Kors Jet Set Handbag", ImportPrice = 268, Count = 25, Description = "Luxury leather handbag with signature MK logo", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-010", Name = "Fossil Gen 6 Hybrid Smartwatch", ImportPrice = 175, Count = 38, Description = "Elegant analog watch with smart features and leather strap", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-011", Name = "Columbia Newton Ridge Hiking Boots", ImportPrice = 119, Count = 56, Description = "Durable waterproof hiking boots for outdoor adventures", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-012", Name = "Patagonia Better Sweater Fleece", ImportPrice = 159, Count = 47, Description = "Warm recycled fleece jacket with sustainable materials", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-013", Name = "Tommy Hilfiger Dress Shirt", ImportPrice = 59, Count = 93, Description = "Formal cotton dress shirt in classic fit", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-014", Name = "Calvin Klein Underwear 3-Pack", ImportPrice = 42, Count = 156, Description = "Premium cotton underwear set with elastic waistband", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-015", Name = "Vans Old Skool Canvas Sneakers", ImportPrice = 55, Count = 124, Description = "Classic skate shoes with signature side stripe", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-016", Name = "Champion Reverse Weave Hoodie", ImportPrice = 48, Count = 142, Description = "Comfortable fleece hoodie with iconic logo", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-017", Name = "Timberland 6-Inch Premium Boots", ImportPrice = 169, Count = 52, Description = "Iconic leather work boots with waterproof construction", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-018", Name = "Converse Chuck Taylor All Star", ImportPrice = 50, Count = 168, Description = "Timeless canvas sneakers in high-top design", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-019", Name = "Under Armour Sports Bra", ImportPrice = 38, Count = 97, Description = "High-support sports bra for intense workouts", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-020", Name = "Lululemon Align Yoga Pants", ImportPrice = 88, Count = 76, Description = "Premium athletic leggings with buttery-soft fabric", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-021", Name = "Carhartt Rugged Work Pants", ImportPrice = 54, Count = 85, Description = "Durable work trousers with reinforced knees", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-022", Name = "Guess Classic Denim Jacket", ImportPrice = 98, Count = 41, Description = "Stylish denim jacket with vintage wash finish", CategoryId = categoryId, UserId = userId }
        };

        await _context.Products.AddRangeAsync(products);
        _logger.LogInformation("Seeded {Count} Fashion products", products.Length);
    }

    private async Task SeedHomeLivingProducts(int categoryId, string userId)
    {
        var products = new[]
        {
            new Product { Sku = "HOME-001", Name = "IKEA MALM Bed Frame Queen", ImportPrice = 249, Count = 18, Description = "Modern bed frame with integrated storage drawers", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-002", Name = "Ashley 3-Piece Leather Sofa Set", ImportPrice = 1099, Count = 8, Description = "Premium leather sofa set for living room", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-003", Name = "Wayfair Solid Wood Dining Table", ImportPrice = 479, Count = 12, Description = "Elegant dining table seats up to 6 people", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-004", Name = "CB2 Glass Coffee Table", ImportPrice = 349, Count = 22, Description = "Contemporary tempered glass coffee table", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-005", Name = "West Elm Mid-Century Bookshelf", ImportPrice = 399, Count = 15, Description = "Stylish bookshelf with adjustable shelves", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-006", Name = "Pottery Barn Wool Area Rug 8x10", ImportPrice = 599, Count = 9, Description = "Hand-tufted wool area rug with geometric pattern", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-007", Name = "Crate & Barrel LED Table Lamp", ImportPrice = 129, Count = 34, Description = "Modern table lamp with adjustable brightness", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-008", Name = "Dyson V15 Detect Cordless Vacuum", ImportPrice = 579, Count = 26, Description = "Powerful cordless vacuum with laser dust detection", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-009", Name = "Instant Pot Duo Plus 8-Quart", ImportPrice = 109, Count = 67, Description = "9-in-1 multi-cooker pressure cooker", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-010", Name = "KitchenAid Artisan Stand Mixer", ImportPrice = 379, Count = 31, Description = "Professional 5-quart stand mixer in multiple colors", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-011", Name = "Ninja Professional Blender 1000W", ImportPrice = 89, Count = 58, Description = "High-power blender for smoothies and food prep", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-012", Name = "Nespresso VertuoPlus Coffee Maker", ImportPrice = 169, Count = 43, Description = "Coffee and espresso maker with frother", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-013", Name = "Cuisinart Air Fryer Toaster Oven", ImportPrice = 119, Count = 52, Description = "Large capacity air fryer oven with multiple functions", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-014", Name = "Brooklinen Luxe Sheet Set Queen", ImportPrice = 129, Count = 71, Description = "Premium percale sheet set with deep pockets", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-015", Name = "Casper Memory Foam Pillow 2-Pack", ImportPrice = 89, Count = 84, Description = "Supportive pillows with adjustable loft", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-016", Name = "Ruggable Washable Rug 8x10", ImportPrice = 249, Count = 37, Description = "Machine-washable area rug with non-slip pad", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-017", Name = "Umbra Gallery Wall Frame Set", ImportPrice = 45, Count = 96, Description = "9-piece modern picture frame set", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-018", Name = "Philips Hue Smart Bulb Starter Kit", ImportPrice = 179, Count = 45, Description = "Smart LED lighting system with app control", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-019", Name = "Shark IQ Robot Vacuum", ImportPrice = 299, Count = 28, Description = "Self-emptying robot vacuum with mapping", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-020", Name = "Ninja Foodi Indoor Grill", ImportPrice = 199, Count = 21, Description = "Indoor smokeless grill with air crisp technology", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-021", Name = "OXO Good Grips Kitchen Tool Set", ImportPrice = 69, Count = 63, Description = "15-piece essential kitchen utensil set", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-022", Name = "Calphalon Nonstick Cookware 10-Piece", ImportPrice = 269, Count = 19, Description = "Complete nonstick cookware set with glass lids", CategoryId = categoryId, UserId = userId }
        };

        await _context.Products.AddRangeAsync(products);
        _logger.LogInformation("Seeded {Count} Home & Living products", products.Length);
    }
}
