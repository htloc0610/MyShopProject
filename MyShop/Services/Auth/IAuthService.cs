using System.Threading.Tasks;
using MyShop.Models.Auth;

namespace MyShop.Services.Auth
{
    /// <summary>
    /// Interface for authentication operations.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Register a new user.
        /// </summary>
        Task<AuthResult> RegisterAsync(string email, string password, string shopName, string? role = null);

        /// <summary>
        /// Login with email and password.
        /// </summary>
        Task<AuthResult> LoginAsync(string email, string password);

        /// <summary>
        /// Refresh the access token using the stored refresh token.
        /// </summary>
        Task<AuthResult> RefreshTokenAsync();

        /// <summary>
        /// Logout and clear session.
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Get current user info from the API.
        /// </summary>
        Task<UserInfo?> GetCurrentUserAsync();
    }
}
