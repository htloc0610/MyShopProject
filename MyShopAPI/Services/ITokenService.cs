using MyShopAPI.Models;

namespace MyShopAPI.Services
{
    /// <summary>
    /// Service for generating and validating JWT access tokens and refresh tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT access token for the specified user.
        /// </summary>
        /// <param name="user">The user to generate a token for.</param>
        /// <param name="role">The user's role.</param>
        /// <returns>A tuple containing the token string and its expiry time.</returns>
        Task<(string Token, DateTime ExpiresAt)> GenerateAccessTokenAsync(ApplicationUser user, string role);

        /// <summary>
        /// Generates a refresh token for the specified user.
        /// </summary>
        /// <param name="user">The user to generate a refresh token for.</param>
        /// <returns>The generated refresh token entity.</returns>
        Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user);

        /// <summary>
        /// Validates a refresh token and returns the associated user if valid.
        /// </summary>
        /// <param name="token">The refresh token string to validate.</param>
        /// <returns>The user associated with the token, or null if invalid.</returns>
        Task<ApplicationUser?> ValidateRefreshTokenAsync(string token);

        /// <summary>
        /// Revokes all refresh tokens for a user.
        /// </summary>
        Task RevokeAllUserTokensAsync(string userId);

        /// <summary>
        /// Revokes a specific refresh token.
        /// </summary>
        Task RevokeTokenAsync(string token);
    }
}
