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
                    Name = "Điện tử",
                    Description = "Thiết bị điện tử, đồ công nghệ và phụ kiện",
                    UserId = demoUserId
                },
                new Category
                {
                    Name = "Thời trang",
                    Description = "Quần áo, giày dép và phụ kiện thời trang",
                    UserId = demoUserId
                },
                new Category
                {
                    Name = "Nhà cửa & Đời sống",
                    Description = "Nội thất, trang trí nhà cửa và đồ gia dụng",
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

            // Seed product images (3 images per product)
            await SeedProductImagesAsync(demoUserId);

            // Seed customers FIRST so orders can reference them
            await SeedCustomersAsync(demoUserId);
            
            await SeedDiscountsAsync(demoUserId);

            // Seed orders AFTER customers exist
            await SeedOrdersAsync(demoUserId);

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
            new Product { Sku = "ELEC-001", Name = "Điện thoại iPhone 15 Pro Max 256GB", ImportPrice = 28990000, SellingPrice = 37687000, Count = 45, Description = "Điện thoại cao cấp với thiết kế titanium và hệ thống camera tiên tiến", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-002", Name = "Điện thoại Samsung Galaxy S24 Ultra 512GB", ImportPrice = 32990000, SellingPrice = 42887000, Count = 38, Description = "Điện thoại Android cao cấp với tính năng AI và hỗ trợ S Pen", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-003", Name = "Laptop MacBook Pro 14\" M3 Pro 18GB/512GB", ImportPrice = 52990000, SellingPrice = 68887000, Count = 22, Description = "Laptop mạnh mẽ cho dân chuyên nghiệp với màn hình Liquid Retina XDR", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-004", Name = "Laptop Dell XPS 15 9530 i7/16GB/1TB", ImportPrice = 42990000, SellingPrice = 55887000, Count = 18, Description = "Laptop Windows hiệu suất cao với màn hình InfinityEdge", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-005", Name = "Máy tính bảng iPad Air 5 M1 256GB WiFi", ImportPrice = 16990000, SellingPrice = 22087000, Count = 56, Description = "Máy tính bảng đa năng với chip M1 và hỗ trợ Apple Pencil", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-006", Name = "Tai nghe Sony WH-1000XM5", ImportPrice = 8490000, SellingPrice = 11037000, Count = 67, Description = "Tai nghe chống ồn hàng đầu với chất lượng âm thanh cao cấp", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-007", Name = "Tai nghe Apple AirPods Pro 2", ImportPrice = 5990000, SellingPrice = 7787000, Count = 89, Description = "Tai nghe không dây với chống ồn chủ động và âm thanh không gian", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-008", Name = "Đồng hồ thông minh Samsung Galaxy Watch 6 Classic", ImportPrice = 7990000, SellingPrice = 10387000, Count = 41, Description = "Đồng hồ thông minh cao cấp với theo dõi sức khỏe toàn diện", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-009", Name = "TV LG OLED C3 65 inch", ImportPrice = 42990000, SellingPrice = 55887000, Count = 12, Description = "TV OLED cao cấp với chất lượng hình ảnh tuyệt vời", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-010", Name = "Máy chơi game Sony PlayStation 5", ImportPrice = 12490000, SellingPrice = 16237000, Count = 28, Description = "Máy chơi game thế hệ mới với ray tracing và SSD nhanh", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-011", Name = "Máy chơi game Xbox Series X 1TB", ImportPrice = 12490000, SellingPrice = 16237000, Count = 24, Description = "Máy chơi game mạnh mẽ với Game Pass", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-012", Name = "Máy chơi game Nintendo Switch OLED", ImportPrice = 8490000, SellingPrice = 11037000, Count = 55, Description = "Máy chơi game lai với màn hình OLED sống động", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-013", Name = "Máy ảnh Canon EOS R6 Mark II", ImportPrice = 59990000, SellingPrice = 77987000, Count = 8, Description = "Máy ảnh không gương lật chuyên nghiệp với lấy nét tự động tiên tiến", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-014", Name = "Drone DJI Mini 3 Pro", ImportPrice = 18990000, SellingPrice = 24687000, Count = 15, Description = "Drone nhỏ gọn với camera 4K và tránh chướng ngại vật", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-015", Name = "Camera hành động GoPro Hero 12 Black", ImportPrice = 9990000, SellingPrice = 12987000, Count = 33, Description = "Camera hành động cho thể thao mạo hiểm với video 5.3K", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-016", Name = "Tai nghe Bose QuietComfort 45", ImportPrice = 7490000, SellingPrice = 9737000, Count = 44, Description = "Tai nghe chống ồn thoải mái với pin lâu", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-017", Name = "Chuột không dây Logitech MX Master 3S", ImportPrice = 2490000, SellingPrice = 3237000, Count = 78, Description = "Chuột không dây cao cấp cho năng suất làm việc", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-018", Name = "Bàn phím cơ Keychron K8 Pro", ImportPrice = 2990000, SellingPrice = 3887000, Count = 62, Description = "Bàn phím cơ không dây với switch có thể thay thế", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-019", Name = "Ổ cứng SSD Samsung T7 Shield 2TB", ImportPrice = 4990000, SellingPrice = 6487000, Count = 51, Description = "Ổ cứng SSD di động chống va đập với tốc độ truyền nhanh", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-020", Name = "Sạc dự phòng Anker PowerCore 26800mAh", ImportPrice = 1490000, SellingPrice = 1937000, Count = 94, Description = "Sạc dự phòng dung lượng cao cho nhiều thiết bị", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-021", Name = "Chuông cửa thông minh Ring Video Doorbell Pro 2", ImportPrice = 5990000, SellingPrice = 7787000, Count = 37, Description = "Chuông cửa thông minh với video HD và phát hiện chuyển động", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "ELEC-022", Name = "Màn hình thông minh Amazon Echo Show 10", ImportPrice = 6490000, SellingPrice = 8437000, Count = 29, Description = "Màn hình thông minh với theo dõi chuyển động và tích hợp Alexa", CategoryId = categoryId, UserId = userId }
        };

        await _context.Products.AddRangeAsync(products);
        _logger.LogInformation("Seeded {Count} Electronics products", products.Length);
    }

    private async Task SeedFashionProducts(int categoryId, string userId)
    {
        var products = new[]
        {
            new Product { Sku = "FASH-001", Name = "Giày thể thao Nike Air Max 2024", ImportPrice = 3490000, SellingPrice = 4189000, Count = 72, Description = "Giày sneaker biểu tượng với công nghệ đệm Air và thiết kế hiện đại", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-002", Name = "Giày chạy bộ Adidas Ultraboost 23", ImportPrice = 4290000, SellingPrice = 5148000, Count = 65, Description = "Giày chạy bộ cao cấp với đế Boost đàn hồi", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-003", Name = "Quần jeans Levi's 501 Original", ImportPrice = 1890000, SellingPrice = 2173000, Count = 118, Description = "Quần jeans thẳng cổ điển với kiểu dáng biểu tượng", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-004", Name = "Áo khoác dạ Zara Wool Blend", ImportPrice = 3290000, SellingPrice = 3948000, Count = 34, Description = "Áo khoác dạ sang trọng cho mùa đông", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-005", Name = "Áo thun H&M Cotton (5 chiếc)", ImportPrice = 599000, SellingPrice = 689000, Count = 205, Description = "Bộ 5 áo thun cotton thiết yếu nhiều màu sắc", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-006", Name = "Áo polo Ralph Lauren Classic", ImportPrice = 2190000, SellingPrice = 2628000, Count = 87, Description = "Áo polo cổ điển với logo ngựa đặc trưng", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-007", Name = "Áo khoác The North Face Waterproof", ImportPrice = 5490000, SellingPrice = 6588000, Count = 43, Description = "Áo khoác ngoài trời bền chống nước cao cấp", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-008", Name = "Kính râm Ray-Ban Aviator Classic", ImportPrice = 3890000, SellingPrice = 4668000, Count = 91, Description = "Kính râm phi công biểu tượng chống tia UV", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-009", Name = "Túi xách Michael Kors Jet Set", ImportPrice = 6990000, SellingPrice = 8388000, Count = 25, Description = "Túi xách da cao cấp với logo MK đặc trưng", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-010", Name = "Đồng hồ thông minh Fossil Gen 6 Hybrid", ImportPrice = 4890000, SellingPrice = 5868000, Count = 38, Description = "Đồng hồ analog sang trọng tích hợp tính năng thông minh", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-011", Name = "Giày leo núi Columbia Newton Ridge", ImportPrice = 2990000, SellingPrice = 3588000, Count = 56, Description = "Giày leo núi chống nước bền bỉ", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-012", Name = "Áo lông cừm Patagonia Better Sweater", ImportPrice = 3990000, SellingPrice = 4788000, Count = 47, Description = "Áo khoác lông cừm ấm từ vật liệu tái chế", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-013", Name = "Áo sơ mi Tommy Hilfiger Dress", ImportPrice = 1490000, SellingPrice = 1714000, Count = 93, Description = "Áo sơ mi công sở cotton form chuẩn", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-014", Name = "Quần lót Calvin Klein 3-Pack", ImportPrice = 990000, SellingPrice = 1139000, Count = 156, Description = "Bộ 3 quần lót cotton cao cấp", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-015", Name = "Giày Vans Old Skool Canvas", ImportPrice = 1590000, SellingPrice = 1829000, Count = 124, Description = "Giày skate cổ điển với sọc hông đặc trưng", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-016", Name = "Áo hoodie Champion Reverse Weave", ImportPrice = 1290000, SellingPrice = 1484000, Count = 142, Description = "Áo hoodie nỉ thoải mái với logo biểu tượng", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-017", Name = "Giày boot Timberland 6-Inch Premium", ImportPrice = 4290000, SellingPrice = 5148000, Count = 52, Description = "Giày boot da biểu tượng chống nước", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-018", Name = "Giày Converse Chuck Taylor All Star", ImportPrice = 1390000, SellingPrice = 1599000, Count = 168, Description = "Giày canvas cổ điển kiểu cổ cao", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-019", Name = "Áo ngực thể thao Under Armour", ImportPrice = 890000, SellingPrice = 1024000, Count = 97, Description = "Áo ngực thể thao hỗ trợ cao cho tập luyện", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-020", Name = "Quần tập yoga Lululemon Align", ImportPrice = 2490000, SellingPrice = 2988000, Count = 76, Description = "Quần legging cao cấp với chất vải mềm mịn", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-021", Name = "Quần làm việc Carhartt Rugged", ImportPrice = 1590000, SellingPrice = 1829000, Count = 85, Description = "Quần làm việc bền với đầu gối gia cường", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "FASH-022", Name = "Áo khoác jeans Guess Classic", ImportPrice = 2590000, SellingPrice = 3108000, Count = 41, Description = "Áo khoác jeans phong cách vintage", CategoryId = categoryId, UserId = userId }
        };

        await _context.Products.AddRangeAsync(products);
        _logger.LogInformation("Seeded {Count} Fashion products", products.Length);
    }

    private async Task SeedHomeLivingProducts(int categoryId, string userId)
    {
        var products = new[]
        {
            new Product { Sku = "HOME-001", Name = "IKEA MALM Giường Queen", ImportPrice = 6490000, SellingPrice = 7139000, Count = 18, Description = "Khung giường hiện đại với ngăn kéo lưu trữ tích hợp", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-002", Name = "Ashley Sofa Da 3 Món", ImportPrice = 28990000, SellingPrice = 34788000, Count = 8, Description = "Bộ sofa da cao cấp cho phòng khách", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-003", Name = "Wayfair Bàn Ăn Gỗ", ImportPrice = 12490000, SellingPrice = 14988000, Count = 12, Description = "Bàn ăn gỗ sang trọng cho 6 người", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-004", Name = "CB2 Bàn Trà Kính", ImportPrice = 8990000, SellingPrice = 10788000, Count = 22, Description = "Bàn trà kính cường lực phong cách hiện đại", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-005", Name = "West Elm Kệ Sách Mid-Century", ImportPrice = 10490000, SellingPrice = 12588000, Count = 15, Description = "Kệ sách đẹp với các kệ có thể điều chỉnh", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-006", Name = "Pottery Barn Thảm Len 8x10", ImportPrice = 15990000, SellingPrice = 19188000, Count = 9, Description = "Thảm len dệt tay với hoa văn hình học", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-007", Name = "Crate & Barrel Đèn Bàn LED", ImportPrice = 3290000, SellingPrice = 3618000, Count = 34, Description = "Đèn bàn hiện đại điều chỉnh độ sáng", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-008", Name = "Dyson V15 Detect Máy Hút Bụi", ImportPrice = 15990000, SellingPrice = 19188000, Count = 26, Description = "Máy hút bụi không dây với laser phát hiện bụi", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-009", Name = "Instant Pot Duo Plus 8L", ImportPrice = 2990000, SellingPrice = 3289000, Count = 67, Description = "Nồi áp suất đa năng 9-trong-1", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-010", Name = "KitchenAid Artisan Stand Mixer", ImportPrice = 9990000, SellingPrice = 11988000, Count = 31, Description = "Máy trộn chuyên nghiệp 5 lít nhiều màu sắc", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-011", Name = "Ninja Blender 1000W", ImportPrice = 2490000, SellingPrice = 2739000, Count = 58, Description = "Máy xay sinh tố công suất cao", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-012", Name = "Nespresso VertuoPlus", ImportPrice = 4490000, SellingPrice = 4939000, Count = 43, Description = "Máy pha cà phê và espresso kèm đánh sữa", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-013", Name = "Cuisinart Lò Nướng Chiên Không Dầu", ImportPrice = 3290000, SellingPrice = 3619000, Count = 52, Description = "Lò nướng chiên không dầu dung tích lớn đa chức năng", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-014", Name = "Brooklinen Bộ Chăn Ga Queen", ImportPrice = 3490000, SellingPrice = 4188000, Count = 71, Description = "Bộ chăn ga cao cấp với túi sâu", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-015", Name = "Casper Gối Memory Foam 2 Chiếc", ImportPrice = 2490000, SellingPrice = 2988000, Count = 84, Description = "Gối hỗ trợ điều chỉnh được độ cao", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-016", Name = "Ruggable Thảm Giặt Được 8x10", ImportPrice = 6490000, SellingPrice = 7139000, Count = 37, Description = "Thảm trải sàn giặt máy được kèm đế chống trượt", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-017", Name = "Umbra Bộ Khung Ảnh Treo Tường", ImportPrice = 1290000, SellingPrice = 1419000, Count = 96, Description = "Bộ 9 khung ảnh hiện đại", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-018", Name = "Philips Hue Smart Bulb Starter Kit", ImportPrice = 4690000, SellingPrice = 5159000, Count = 45, Description = "Hệ thống đèn LED thông minh điều khiển qua app", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-019", Name = "Shark IQ Robot Hút Bụi", ImportPrice = 7990000, SellingPrice = 8789000, Count = 28, Description = "Robot hút bụi tự đổ rác với bản đồ", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-020", Name = "Ninja Foodi Bếp Nướng Trong Nhà", ImportPrice = 5490000, SellingPrice = 6039000, Count = 21, Description = "Bếp nướng trong nhà không khói kèm chiên không dầu", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-021", Name = "OXO Bộ Dụng Cụ Bếp 15 Món", ImportPrice = 1890000, SellingPrice = 2079000, Count = 63, Description = "Bộ dụng cụ nhà bếp thiết yếu 15 món", CategoryId = categoryId, UserId = userId },
            new Product { Sku = "HOME-022", Name = "Calphalon Bộ Nồi Chảo 10 Món", ImportPrice = 6990000, SellingPrice = 7689000, Count = 19, Description = "Bộ nồi chảo chống dính kèm nắp kính", CategoryId = categoryId, UserId = userId }
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

        // Lấy danh sách customers
        var customers = await _context.Customers
            .IgnoreQueryFilters()
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!customers.Any())
        {
            _logger.LogWarning("No customers found. Skipping order seeding.");
            return;
        }

        var orders = new List<Order>();
        var orderItems = new List<OrderItem>();

        // Seed ~30 ngày trong tháng hiện tại
        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-29);

        int orderIdCounter = 1;

        // Array of possible statuses with weights (80% Created, 15% Paid, 5% Cancelled)
        var statusWeights = new[] { 
            (OrderStatus.Created, 80),
            (OrderStatus.Paid, 15),
            (OrderStatus.Cancelled, 5)
        };

        for (int i = 0; i < 30; i++)
        {
            var orderDate = startDate.AddDays(i);

            // mỗi ngày 1–4 đơn
            var ordersPerDay = random.Next(1, 5);

            for (int j = 0; j < ordersPerDay; j++)
            {
                // Chọn ngẫu nhiên customer (80% có customer, 20% guest)
                Customer? selectedCustomer = random.Next(100) < 80 
                    ? customers[random.Next(customers.Count)] 
                    : null;

                // Select order status based on weights
                var roll = random.Next(100);
                var cumulative = 0;
                OrderStatus orderStatus = OrderStatus.Created;
                foreach (var (status, weight) in statusWeights)
                {
                    cumulative += weight;
                    if (roll < cumulative)
                    {
                        orderStatus = status;
                        break;
                    }
                }

                var order = new Order
                {
                    OrderDate = orderDate.AddHours(random.Next(8, 21)),
                    UserId = userId,
                    CustomerId = selectedCustomer?.Id,
                    Status = orderStatus
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
                    // Dùng SellingPrice thay vì random markup
                    var unitPrice = product.SellingPrice;

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
                Description = "Giảm giá chào mừng khách hàng mới",
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
                Description = "Sự kiện sale mùa hè đặc biệt",
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
                Description = "Ưu đãi có hạn - chỉ còn 50 lượt",
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
                Description = "Mã này đã hết hạn",
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

    private async Task SeedProductImagesAsync(string userId)
    {
        // Check if images already exist
        var hasImages = await _context.ProductImages.AnyAsync();
        if (hasImages)
        {
            _logger.LogInformation("Product images already exist. Skipping image seeding.");
            return;
        }

        var products = await _context.Products
            .IgnoreQueryFilters()
            .Where(p => p.UserId == userId)
            .Include(p => p.Category)
            .ToListAsync();

        var productImages = new List<ProductImage>();

        foreach (var product in products)
        {
            // Get 3 unique image IDs for this product (using product ID as seed for consistency)
            var baseId = product.ProductId * 10;
            
            // Use picsum.photos for random product-like images
            for (int i = 0; i < 3; i++)
            {
                var imageId = baseId + i;
                var imageUrl = $"https://picsum.photos/seed/{imageId}/800/800";
                
                productImages.Add(new ProductImage
                {
                    ProductId = product.ProductId,
                    ImageUrl = imageUrl,
                    IsMain = i == 0 // First image is main
                });
            }
        }

        await _context.ProductImages.AddRangeAsync(productImages);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} product images ({Products} products x 3 images)", 
            productImages.Count, products.Count);
    }
}
