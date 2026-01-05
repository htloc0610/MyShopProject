namespace MyShopAPI.Models
{
    /// <summary>
    /// Entity for storing refresh tokens for JWT authentication.
    /// </summary>
    public class RefreshToken
    {
        public int Id { get; set; }

        /// <summary>
        /// The refresh token string.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// When this token expires.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// When this token was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether this token has been revoked (e.g., on logout).
        /// </summary>
        public bool IsRevoked { get; set; } = false;

        /// <summary>
        /// The token that replaced this one (for token rotation).
        /// </summary>
        public string? ReplacedByToken { get; set; }

        /// <summary>
        /// Foreign key to the user.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property to the user.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Check if the token is active (not expired and not revoked).
        /// </summary>
        public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
    }
}
