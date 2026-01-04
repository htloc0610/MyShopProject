using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyShopAPI.Models;
using MyShopAPI.Services;

namespace MyShopAPI.Data
{
    /// <summary>
    /// Application database context with Identity support and multi-tenant query filters.
    /// Inherits from IdentityDbContext for ASP.NET Core Identity integration.
    /// </summary>
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IUserContextService? _userContextService;

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) 
        {
        }

        /// <summary>
        /// Constructor with user context service for applying global query filters.
        /// </summary>
        public AppDbContext(DbContextOptions<AppDbContext> options, IUserContextService userContextService)
            : base(options)
        {
            _userContextService = userContextService;
        }

        public string? CurrentUserId => _userContextService?.GetUserId();

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // CRITICAL: Call base method for Identity configuration
            base.OnModelCreating(modelBuilder);

            // Configure ApplicationUser relationships
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasMany(u => u.Categories)
                    .WithOne(c => c.User)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Products)
                    .WithOne(p => p.User)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Orders)
                    .WithOne(o => o.User)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.RefreshTokens)
                    .WithOne(r => r.User)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(r => r.Token).IsUnique();
            });

            // Configure Category indexes
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => new { c.UserId, c.Name });
            });

            // Configure Product indexes
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => new { p.UserId, p.Sku });
            });

            // Configure Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(o => new { o.UserId, o.CreatedTime });
            });

            // ====================================================
            // GLOBAL QUERY FILTERS for Multi-Tenant Data Isolation
            // These filters automatically filter queries by UserId
            // ====================================================

            // GLOBAL QUERY FILTERS for Multi-Tenant Data Isolation
            // These filters automatically filter queries by UserId
            // ====================================================

            // We use the property CurrentUserId so EF Core creates a parameterized query
            // and evaluates the value for EACH request (not just once at startup)
            
            modelBuilder.Entity<Category>()
                .HasQueryFilter(c => c.UserId == CurrentUserId);

            modelBuilder.Entity<Product>()
                .HasQueryFilter(p => p.UserId == CurrentUserId);

            modelBuilder.Entity<Order>()
                .HasQueryFilter(o => o.UserId == CurrentUserId);
        }
    }
}
