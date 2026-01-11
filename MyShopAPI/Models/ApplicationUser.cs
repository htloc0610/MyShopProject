using Microsoft.AspNetCore.Identity;

namespace MyShopAPI.Models
{
    /// <summary>
    /// Custom Identity user representing a shop owner or staff member.
    /// ShopId is equivalent to UserId - each user owns their own data.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// The name of the shop owned by this user.
        /// </summary>
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// When the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indicates whether the user has activated their account with a license key.
        /// Default is false, meaning users start in trial mode.
        /// </summary>
        public bool IsLicensed { get; set; } = false;

        /// <summary>
        /// Stores the server-generated activation code waiting to be entered by the user.
        /// This is set when an expired trial user logs in and needs activation.
        /// Cleared after successful activation.
        /// </summary>
        public string? CurrentLicenseKey { get; set; }

        /// <summary>
        /// Navigation property for user's categories.
        /// </summary>
        public ICollection<Category> Categories { get; set; } = new List<Category>();

        /// <summary>
        /// Navigation property for user's products.
        /// </summary>
        public ICollection<Product> Products { get; set; } = new List<Product>();

        /// <summary>
        /// Navigation property for user's orders.
        /// </summary>
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        /// <summary>
        /// Navigation property for user's refresh tokens.
        /// </summary>
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        /// <summary>
        /// Navigation property for user's customers.
        /// </summary>
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();

        /// <summary>
        /// Navigation property for user's discount codes.
        /// </summary>
        public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
    }
}
