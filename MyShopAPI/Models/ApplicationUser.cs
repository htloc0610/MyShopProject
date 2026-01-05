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
    }
}
