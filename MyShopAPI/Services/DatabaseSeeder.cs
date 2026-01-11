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
            // Apply any pending migrations (creates database if not exists)
            await _context.Database.MigrateAsync();

            // Seed roles first
            await SeedRolesAsync();

            // Seed demo user
            var demoUserId = await SeedDemoUserAsync();

            // Seed additional users
            await SeedAdditionalUsersAsync();

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

            await SeedOrdersAsync(demoUserId);
            
            await SeedDiscountsAsync(demoUserId);

            await SeedCustomersAsync(demoUserId);

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
            Console.WriteLine($"Error occurred while seeding database: {ex.Message}");
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
            Console.WriteLine($"Error creating demo user: {error.Description}");
        }

        return null;
    }

    private async Task SeedElectronicsProducts(int categoryId, string userId)
    {
        var products = new[]
        {
            new Product { Sku = "ELEC-001", Name = "iPhone 15 Pro Max 256GB", ImportPrice = 28990000, SellingPrice = 37687000, Count = 45, Description = "Latest flagship iPhone with titanium design and advanced camera system", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-002", Name = "Samsung Galaxy S24 Ultra 512GB", ImportPrice = 32990000, SellingPrice = 42887000, Count = 38, Description = "Premium Android phone with AI features and S Pen support", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-003", Name = "MacBook Pro 14\" M3 Pro 18GB/512GB", ImportPrice = 52990000, SellingPrice = 68887000, Count = 22, Description = "Powerful laptop for creative professionals with stunning Liquid Retina XDR display", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-004", Name = "Dell XPS 15 9530 i7/16GB/1TB", ImportPrice = 42990000, SellingPrice = 55887000, Count = 18, Description = "High-performance Windows laptop with InfinityEdge display", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-005", Name = "iPad Air 5th Gen M1 256GB WiFi", ImportPrice = 16990000, SellingPrice = 22087000, Count = 56, Description = "Versatile tablet with M1 chip and Apple Pencil support", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-006", Name = "Sony WH-1000XM5 Headphones", ImportPrice = 8490000, SellingPrice = 11037000, Count = 67, Description = "Industry-leading noise cancellation with premium sound quality", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-007", Name = "Apple AirPods Pro 2nd Gen", ImportPrice = 5990000, SellingPrice = 7787000, Count = 89, Description = "Wireless earbuds with active noise cancellation and spatial audio", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-008", Name = "Samsung Galaxy Watch 6 Classic", ImportPrice = 7990000, SellingPrice = 10387000, Count = 41, Description = "Advanced smartwatch with comprehensive health tracking", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-009", Name = "LG OLED C3 65-inch TV", ImportPrice = 42990000, SellingPrice = 55887000, Count = 12, Description = "Premium OLED TV with stunning picture quality and smart features", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-010", Name = "Sony PlayStation 5 Console", ImportPrice = 12490000, SellingPrice = 16237000, Count = 28, Description = "Next-gen gaming console with ray tracing and fast SSD", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-011", Name = "Xbox Series X 1TB", ImportPrice = 12490000, SellingPrice = 16237000, Count = 24, Description = "Powerful gaming console with Game Pass subscription benefits", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-012", Name = "Nintendo Switch OLED Model", ImportPrice = 8490000, SellingPrice = 11037000, Count = 55, Description = "Hybrid gaming console with vibrant OLED screen", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-013", Name = "Canon EOS R6 Mark II Camera", ImportPrice = 59990000, SellingPrice = 77987000, Count = 8, Description = "Professional mirrorless camera with advanced autofocus", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-014", Name = "DJI Mini 3 Pro Drone", ImportPrice = 18990000, SellingPrice = 24687000, Count = 15, Description = "Compact drone with 4K camera and obstacle avoidance", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-015", Name = "GoPro Hero 12 Black", ImportPrice = 9990000, SellingPrice = 12987000, Count = 33, Description = "Action camera for extreme sports with 5.3K video", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-016", Name = "Bose QuietComfort 45", ImportPrice = 7490000, SellingPrice = 9737000, Count = 44, Description = "Comfortable noise-cancelling headphones with long battery life", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-017", Name = "Logitech MX Master 3S Mouse", ImportPrice = 2490000, SellingPrice = 3237000, Count = 78, Description = "Premium wireless mouse for productivity with precise scrolling", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-018", Name = "Keychron K8 Pro Keyboard", ImportPrice = 2990000, SellingPrice = 3887000, Count = 62, Description = "Wireless mechanical keyboard with hot-swappable switches", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-019", Name = "Samsung T7 Shield 2TB SSD", ImportPrice = 4990000, SellingPrice = 6487000, Count = 51, Description = "Rugged portable SSD with fast transfer speeds", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-020", Name = "Anker PowerCore 26800mAh", ImportPrice = 1490000, SellingPrice = 1937000, Count = 94, Description = "High-capacity portable charger for multiple devices", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-021", Name = "Ring Video Doorbell Pro 2", ImportPrice = 5990000, SellingPrice = 7787000, Count = 37, Description = "Smart doorbell with HD video and motion detection", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-022", Name = "Amazon Echo Show 10", ImportPrice = 6490000, SellingPrice = 8437000, Count = 29, Description = "Smart display with motion tracking and Alexa integration", CategoryId = categoryId, UserId = userId }
        };

        await _context.Products.AddRangeAsync(products);
        _logger.LogInformation("Seeded {Count} Electronics products", products.Length);
    }

    private async Task SeedFashionProducts(int categoryId, string userId)
    {
        var products = new[]
        {
            new Product { Sku = "FASH-001", Name = "Nike Air Max 2024 Sneakers", ImportPrice = 3490000, SellingPrice = 4189000, Count = 72, Description = "Iconic sneakers with Air cushioning technology and modern design", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-002", Name = "Adidas Ultraboost 23 Running Shoes", ImportPrice = 4290000, SellingPrice = 5148000, Count = 65, Description = "Premium running shoes with responsive Boost cushioning", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-003", Name = "Levi's 501 Original Fit Jeans", ImportPrice = 1890000, SellingPrice = 2173000, Count = 118, Description = "Classic straight-leg denim jeans with iconic styling", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-004", Name = "Zara Wool Blend Overcoat", ImportPrice = 3290000, SellingPrice = 3948000, Count = 34, Description = "Elegant winter coat perfect for formal occasions", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-005", Name = "H&M Premium Cotton T-Shirt Pack", ImportPrice = 599000, SellingPrice = 689000, Count = 205, Description = "5-pack of essential cotton t-shirts in various colors", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-006", Name = "Ralph Lauren Classic Polo Shirt", ImportPrice = 2190000, SellingPrice = 2628000, Count = 87, Description = "Timeless polo shirt with signature pony logo", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-007", Name = "The North Face Waterproof Jacket", ImportPrice = 5490000, SellingPrice = 6588000, Count = 43, Description = "Durable outdoor jacket with advanced waterproof technology", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-008", Name = "Ray-Ban Aviator Classic Sunglasses", ImportPrice = 3890000, SellingPrice = 4668000, Count = 91, Description = "Iconic aviator-style sunglasses with UV protection", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-009", Name = "Michael Kors Jet Set Handbag", ImportPrice = 6990000, SellingPrice = 8388000, Count = 25, Description = "Luxury leather handbag with signature MK logo", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-010", Name = "Fossil Gen 6 Hybrid Smartwatch", ImportPrice = 4890000, SellingPrice = 5868000, Count = 38, Description = "Elegant analog watch with smart features and leather strap", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-011", Name = "Columbia Newton Ridge Hiking Boots", ImportPrice = 2990000, SellingPrice = 3588000, Count = 56, Description = "Durable waterproof hiking boots for outdoor adventures", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-012", Name = "Patagonia Better Sweater Fleece", ImportPrice = 3990000, SellingPrice = 4788000, Count = 47, Description = "Warm recycled fleece jacket with sustainable materials", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-013", Name = "Tommy Hilfiger Dress Shirt", ImportPrice = 1490000, SellingPrice = 1714000, Count = 93, Description = "Formal cotton dress shirt in classic fit", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-014", Name = "Calvin Klein Underwear 3-Pack", ImportPrice = 990000, SellingPrice = 1139000, Count = 156, Description = "Premium cotton underwear set with elastic waistband", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-015", Name = "Vans Old Skool Canvas Sneakers", ImportPrice = 1590000, SellingPrice = 1829000, Count = 124, Description = "Classic skate shoes with signature side stripe", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-016", Name = "Champion Reverse Weave Hoodie", ImportPrice = 1290000, SellingPrice = 1484000, Count = 142, Description = "Comfortable fleece hoodie with iconic logo", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-017", Name = "Timberland 6-Inch Premium Boots", ImportPrice = 4290000, SellingPrice = 5148000, Count = 52, Description = "Iconic leather work boots with waterproof construction", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-018", Name = "Converse Chuck Taylor All Star", ImportPrice = 1390000, SellingPrice = 1599000, Count = 168, Description = "Timeless canvas sneakers in high-top design", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-019", Name = "Under Armour Sports Bra", ImportPrice = 890000, SellingPrice = 1024000, Count = 97, Description = "High-support sports bra for intense workouts", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-020", Name = "Lululemon Align Yoga Pants", ImportPrice = 2490000, SellingPrice = 2988000, Count = 76, Description = "Premium athletic leggings with buttery-soft fabric", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-021", Name = "Carhartt Rugged Work Pants", ImportPrice = 1590000, SellingPrice = 1829000, Count = 85, Description = "Durable work trousers with reinforced knees", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-022", Name = "Guess Classic Denim Jacket", ImportPrice = 2590000, SellingPrice = 3108000, Count = 41, Description = "Stylish denim jacket with vintage wash finish", CategoryId = categoryId, UserId = userId }
        };

        await _context.Products.AddRangeAsync(products);
        _logger.LogInformation("Seeded {Count} Fashion products", products.Length);
    }

    private async Task SeedHomeLivingProducts(int categoryId, string userId)
    {
        var products = new[]
        {
            new Product { Sku = "HOME-001", Name = "IKEA MALM Bed Frame Queen", ImportPrice = 6490000, SellingPrice = 7139000, Count = 18, Description = "Modern bed frame with integrated storage drawers", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-002", Name = "Ashley 3-Piece Leather Sofa Set", ImportPrice = 28990000, SellingPrice = 34788000, Count = 8, Description = "Premium leather sofa set for living room", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-003", Name = "Wayfair Solid Wood Dining Table", ImportPrice = 12490000, SellingPrice = 14988000, Count = 12, Description = "Elegant dining table seats up to 6 people", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-004", Name = "CB2 Glass Coffee Table", ImportPrice = 8990000, SellingPrice = 10788000, Count = 22, Description = "Contemporary tempered glass coffee table", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-005", Name = "West Elm Mid-Century Bookshelf", ImportPrice = 10490000, SellingPrice = 12588000, Count = 15, Description = "Stylish bookshelf with adjustable shelves", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-006", Name = "Pottery Barn Wool Area Rug 8x10", ImportPrice = 15990000, SellingPrice = 19188000, Count = 9, Description = "Hand-tufted wool area rug with geometric pattern", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-007", Name = "Crate & Barrel LED Table Lamp", ImportPrice = 3290000, SellingPrice = 3618000, Count = 34, Description = "Modern table lamp with adjustable brightness", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-008", Name = "Dyson V15 Detect Cordless Vacuum", ImportPrice = 15990000, SellingPrice = 19188000, Count = 26, Description = "Powerful cordless vacuum with laser dust detection", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-009", Name = "Instant Pot Duo Plus 8-Quart", ImportPrice = 2990000, SellingPrice = 3289000, Count = 67, Description = "9-in-1 multi-cooker pressure cooker", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-010", Name = "KitchenAid Artisan Stand Mixer", ImportPrice = 9990000, SellingPrice = 11988000, Count = 31, Description = "Professional 5-quart stand mixer in multiple colors", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-011", Name = "Ninja Professional Blender 1000W", ImportPrice = 2490000, SellingPrice = 2739000, Count = 58, Description = "High-power blender for smoothies and food prep", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-012", Name = "Nespresso VertuoPlus Coffee Maker", ImportPrice = 4490000, SellingPrice = 4939000, Count = 43, Description = "Coffee and espresso maker with frother", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-013", Name = "Cuisinart Air Fryer Toaster Oven", ImportPrice = 3290000, SellingPrice = 3619000, Count = 52, Description = "Large capacity air fryer oven with multiple functions", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-014", Name = "Brooklinen Luxe Sheet Set Queen", ImportPrice = 3490000, SellingPrice = 4188000, Count = 71, Description = "Premium percale sheet set with deep pockets", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-015", Name = "Casper Memory Foam Pillow 2-Pack", ImportPrice = 2490000, SellingPrice = 2988000, Count = 84, Description = "Supportive pillows with adjustable loft", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-016", Name = "Ruggable Washable Rug 8x10", ImportPrice = 6490000, SellingPrice = 7139000, Count = 37, Description = "Machine-washable area rug with non-slip pad", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-017", Name = "Umbra Gallery Wall Frame Set", ImportPrice = 1290000, SellingPrice = 1419000, Count = 96, Description = "9-piece modern picture frame set", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-018", Name = "Philips Hue Smart Bulb Starter Kit", ImportPrice = 4690000, SellingPrice = 5159000, Count = 45, Description = "Smart LED lighting system with app control", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-019", Name = "Shark IQ Robot Vacuum", ImportPrice = 7990000, SellingPrice = 8789000, Count = 28, Description = "Self-emptying robot vacuum with mapping", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-020", Name = "Ninja Foodi Indoor Grill", ImportPrice = 5490000, SellingPrice = 6039000, Count = 21, Description = "Indoor smokeless grill with air crisp technology", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-021", Name = "OXO Good Grips Kitchen Tool Set", ImportPrice = 1890000, SellingPrice = 2079000, Count = 63, Description = "15-piece essential kitchen utensil set", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-022", Name = "Calphalon Nonstick Cookware 10-Piece", ImportPrice = 6990000, SellingPrice = 7689000, Count = 19, Description = "Complete nonstick cookware set with glass lids", CategoryId = categoryId, UserId = userId }
        };

        await _context.Products.AddRangeAsync(products);
        _logger.LogInformation("Seeded {Count} Home & Living products", products.Length);
    }

    private async Task SeedOrdersAsync(string userId)
    {
        // Nếu đã có order thì không seed nữa
        var hasOrders = await _context.Orders.IgnoreQueryFilters().AnyAsync();
        if (hasOrders)
        {
            _logger.LogInformation("Orders already exist. Skipping order seeding.");
            return;
        }

        _logger.LogInformation("Seeding demo orders...");

        var random = new Random();

        // Lấy danh sách product của demo user
        var products = await _context.Products
            .IgnoreQueryFilters()
            .Where(p => p.UserId == userId)
            .ToListAsync();

        if (!products.Any())
        {
            _logger.LogWarning($"UserId: {userId}");
            //16406b84-16d0-4391-820d-000d034a53bd
            _logger.LogWarning("No products found. Skipping order seeding.");
            return;
        }

        var orders = new List<Order>();
        var orderItems = new List<OrderItem>();

        // Seed ~30 ngày trong tháng hiện tại
        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-29);

        int orderIdCounter = 1;

        for (int i = 0; i < 30; i++)
        {
            var orderDate = startDate.AddDays(i);

            // mỗi ngày 1–4 đơn
            var ordersPerDay = random.Next(1, 5);

            for (int j = 0; j < ordersPerDay; j++)
            {
                var order = new Order
                {
                    OrderDate = orderDate.AddHours(random.Next(8, 21)),
                    UserId = userId
                };

                // mỗi order có 1–3 sản phẩm
                var itemsCount = random.Next(1, 4);
                var selectedProducts = products
                    .OrderBy(_ => random.Next())
                    .Take(itemsCount)
                    .ToList();

                int finalPrice = 0;

                foreach (var product in selectedProducts)
                {
                    var quantity = random.Next(1, 4);
                    var unitPrice = product.ImportPrice + random.Next(100000, 500000); // bán có lời

                    var itemTotal = quantity * unitPrice;
                    finalPrice += itemTotal;

                    orderItems.Add(new OrderItem
                    {
                        Order = order,
                        ProductId = product.ProductId,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = itemTotal
                    });
                }

                order.FinalAmount = finalPrice;
                order.TotalAmount = finalPrice; // For seed data, assume no discount
                orders.Add(order);
            }
        }

        await _context.Orders.AddRangeAsync(orders);
        await _context.OrderItems.AddRangeAsync(orderItems);

        _logger.LogInformation(
            "Seeded {OrderCount} orders with {ItemCount} order items",
            orders.Count,
            orderItems.Count
        );
    }

    private async Task SeedAdditionalUsersAsync()
    {
        // 1. Staff user
        var staffEmail = "staff@myshop.com";
        if (await _userManager.FindByEmailAsync(staffEmail) == null)
        {
            var staffUser = new ApplicationUser
            {
                UserName = staffEmail,
                Email = staffEmail,
                ShopName = "Demo Shop Staff",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(staffUser, "Staff@123");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(staffUser, "Staff");
                _logger.LogInformation("Created staff user: {Email}", staffEmail);
            }
        }

        // 2. Second Owner user
        var owner2Email = "owner2@myshop.com";
        if (await _userManager.FindByEmailAsync(owner2Email) == null)
        {
            var owner2User = new ApplicationUser
            {
                UserName = owner2Email,
                Email = owner2Email,
                ShopName = "Second Shop",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(owner2User, "Owner@123");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(owner2User, "Owner");
                _logger.LogInformation("Created second owner user: {Email}", owner2Email);
            }
        }
    }

    private async Task SeedDiscountsAsync(string userId)
    {
        // Check if user already has discounts
        var hasDiscounts = await _context.Discounts.IgnoreQueryFilters().AnyAsync(d => d.UserId == userId);
        if (hasDiscounts)
        {
            _logger.LogInformation("Discounts already exist for user {UserId}. Skipping discount seeding.", userId);
            return;
        }

        var discounts = new[]
        {
            new Discount
            {
                Code = "WELCOME10",
                Description = "Welcome discount for new customers",
                Amount = 10000,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddMonths(1),
                UsageLimit = 100,
                UsedCount = 5,
                IsActive = true,
                UserId = userId
            },
            new Discount
            {
                Code = "SUMMERSALE",
                Description = "Special summer sale event",
                Amount = 50000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
                UsageLimit = null,
                UsedCount = 12,
                IsActive = true,
                UserId = userId
            },
            new Discount
            {
                Code = "LIMITED50",
                Description = "Limited time offer - only 50 available",
                Amount = 20000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                UsageLimit = 50,
                UsedCount = 48, // Almost running out
                IsActive = true,
                UserId = userId
            },
            new Discount
            {
                Code = "EXPIREDCODE",
                Description = "This code has expired",
                Amount = 15000,
                StartDate = DateTime.UtcNow.AddMonths(-2),
                EndDate = DateTime.UtcNow.AddMonths(-1),
                UsageLimit = null,
                UsedCount = 10,
                IsActive = false,
                UserId = userId
            },
            // New coupons requested by user
            new Discount
            {
                Code = "SAVE20K",
                Description = "Giam 20k cho don hang bat ky",
                Amount = 20000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                UsageLimit = 1000,
                UsedCount = 0,
                IsActive = true,
                UserId = userId
            },
            new Discount
            {
                Code = "SALE100K",
                Description = "Giam 100k cho don hang gia tri cao",
                Amount = 100000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                UsageLimit = 50,
                UsedCount = 0,
                IsActive = true,
                UserId = userId
            },
            new Discount
            {
                Code = "FREESHIP",
                Description = "Mien phi van chuyen (Giam 30k)",
                Amount = 30000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                UsageLimit = null,
                UsedCount = 0,
                IsActive = true,
                UserId = userId
            },
            new Discount
            {
                Code = "TET2026",
                Description = "Li xi dau nam 2026",
                Amount = 68000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                UsageLimit = 2026,
                UsedCount = 0,
                IsActive = true,
                UserId = userId
            },
            new Discount
            {
                Code = "FANPAGE",
                Description = "Ma giam gia tu Fanpage",
                Amount = 15000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                UsageLimit = 500,
                UsedCount = 0,
                IsActive = true,
                UserId = userId
            }
        };

        await _context.Discounts.AddRangeAsync(discounts);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} discounts for user {UserId}", discounts.Length, userId);
    }
    private async Task SeedCustomersAsync(string userId)
    {
        var hasCustomers = await _context.Customers.IgnoreQueryFilters().AnyAsync(c => c.UserId == userId);
        if (hasCustomers)
        {
            _logger.LogInformation("Customers already exist for user {UserId}. Skipping customer seeding.", userId);
            return;
        }

        var customers = new[]
        {
            new Customer { Name = "Nguyen Van A", PhoneNumber = "0901234567", Address = "123 Le Loi, District 1, HCMC", TotalSpent = 1500000, UserId = userId },
            new Customer { Name = "Tran Thi B", PhoneNumber = "0912345678", Address = "456 Nguyen Hue, District 1, HCMC", TotalSpent = 2500000, UserId = userId },
            new Customer { Name = "Le Van C", PhoneNumber = "0987654321", Address = "789 Hai Ba Trung, District 3, HCMC", TotalSpent = 500000, UserId = userId },
            new Customer { Name = "Pham Thi D", PhoneNumber = "0909090909", Address = "101 Dien Bien Phu, Binh Thanh, HCMC", TotalSpent = 0, UserId = userId },
            new Customer { Name = "Hoang Van E", PhoneNumber = "0918181818", Address = "202 Vo Thi Sau, District 3, HCMC", TotalSpent = 8900000, UserId = userId },
            new Customer { Name = "Ngo Thi F", PhoneNumber = "0933333333", Address = "303 Ly Tu Trong, District 1, HCMC", TotalSpent = 120000, UserId = userId },
            new Customer { Name = "Dang Van G", PhoneNumber = "0944444444", Address = "404 Nam Ky Khoi Nghia, District 3, HCMC", TotalSpent = 350000, UserId = userId },
            new Customer { Name = "Bui Thi H", PhoneNumber = "0955555555", Address = "505 Nguyen Trai, District 5, HCMC", TotalSpent = 6000000, UserId = userId },
            new Customer { Name = "Do Van I", PhoneNumber = "0966666666", Address = "606 Tran Hung Dao, District 5, HCMC", TotalSpent = 450000, UserId = userId },
            new Customer { Name = "Ho Thi K", PhoneNumber = "0977777777", Address = "707 Ly Thuong Kiet, District 10, HCMC", TotalSpent = 90000, UserId = userId },
            new Customer { Name = "Duong Van L", PhoneNumber = "0988888888", Address = "808 Cach Mang Thang 8, District 10, HCMC", TotalSpent = 7800000, UserId = userId },
            new Customer { Name = "Ly Thi M", PhoneNumber = "0999999999", Address = "909 3/2, District 10, HCMC", TotalSpent = 200000, UserId = userId },
            new Customer { Name = "Vu Van N", PhoneNumber = "0901111111", Address = "111 Nguyen Van Cu, District 5, HCMC", TotalSpent = 150000, UserId = userId },
            new Customer { Name = "Vo Thi O", PhoneNumber = "0902222222", Address = "222 An Duong Vuong, District 5, HCMC", TotalSpent = 3000000, UserId = userId },
            new Customer { Name = "Truong Van P", PhoneNumber = "0903333333", Address = "333 Hong Bang, District 5, HCMC", TotalSpent = 0, UserId = userId }
        };

        await _context.Customers.AddRangeAsync(customers);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} customers for user {UserId}", customers.Length, userId);
    }
}
