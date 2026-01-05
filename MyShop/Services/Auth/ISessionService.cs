using System.Threading.Tasks;

namespace MyShop.Services.Auth
{
    /// <summary>
    /// Interface for managing the current user session.
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// Gets whether the user is currently logged in.
        /// </summary>
        bool IsLoggedIn { get; }

        /// <summary>
        /// Gets the current user's ID.
        /// </summary>
        string? UserId { get; }

        /// <summary>
        /// Gets the current user's email.
        /// </summary>
        string? Email { get; }

        /// <summary>
        /// Gets the current user's shop name.
        /// </summary>
        string? ShopName { get; }

        /// <summary>
        /// Gets the current user's role (Owner or Staff).
        /// </summary>
        string? Role { get; }

        /// <summary>
        /// Gets the current access token.
        /// </summary>
        string? AccessToken { get; }

        /// <summary>
        /// Gets the current refresh token.
        /// </summary>
        string? RefreshToken { get; }

        /// <summary>
        /// Check if the current user has the Owner role.
        /// </summary>
        bool IsOwner { get; }

        /// <summary>
        /// Set session data after successful login.
        /// </summary>
        void SetSession(string userId, string email, string shopName, string role, string accessToken, string refreshToken);

        /// <summary>
        /// Clear session data on logout.
        /// </summary>
        void ClearSession();

        /// <summary>
        /// Update access token after refresh.
        /// </summary>
        void UpdateAccessToken(string newAccessToken);

        /// <summary>
        /// Try to restore session from stored credentials.
        /// </summary>
        Task<bool> TryRestoreSessionAsync();
    }
}
