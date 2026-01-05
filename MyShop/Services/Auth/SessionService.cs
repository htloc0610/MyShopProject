using System.Threading.Tasks;

namespace MyShop.Services.Auth
{
    /// <summary>
    /// Singleton service for managing the current user session.
    /// Holds user information and tokens for the duration of the app session.
    /// </summary>
    public class SessionService : ISessionService
    {
        private readonly ICredentialService _credentialService;

        public SessionService(ICredentialService credentialService)
        {
            _credentialService = credentialService;
        }

        /// <inheritdoc />
        public bool IsLoggedIn { get; private set; }

        /// <inheritdoc />
        public string? UserId { get; private set; }

        /// <inheritdoc />
        public string? Email { get; private set; }

        /// <inheritdoc />
        public string? ShopName { get; private set; }

        /// <inheritdoc />
        public string? Role { get; private set; }

        /// <inheritdoc />
        public string? AccessToken { get; private set; }

        /// <inheritdoc />
        public string? RefreshToken { get; private set; }

        /// <inheritdoc />
        public bool IsOwner => Role == "Owner";

        /// <inheritdoc />
        public void SetSession(string userId, string email, string shopName, string role, string accessToken, string refreshToken)
        {
            UserId = userId;
            Email = email;
            ShopName = shopName;
            Role = role;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            IsLoggedIn = true;

            IsLoggedIn = true;

            // Tokens are NOT saved here automatically. 
            // Persistence is handled by the caller (LoginViewModel or AuthService) based on context (Remember Me).
        }

        /// <inheritdoc />
        public void ClearSession()
        {
            UserId = null;
            Email = null;
            ShopName = null;
            Role = null;
            AccessToken = null;
            RefreshToken = null;
            IsLoggedIn = false;

            // Clear stored credentials
            _credentialService.ClearCredentials();
        }

        /// <inheritdoc />
        public void UpdateAccessToken(string newAccessToken)
        {
            AccessToken = newAccessToken;
            _credentialService.SaveAccessToken(newAccessToken);
        }

        /// <inheritdoc />
        public async Task<bool> TryRestoreSessionAsync()
        {
            // Check if we have stored credentials
            if (!_credentialService.HasStoredCredentials())
            {
                return false;
            }

            var accessToken = _credentialService.GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                return false;
            }

            // Set the access token temporarily
            AccessToken = accessToken;
            IsLoggedIn = true;

            // The actual user info will be fetched via API call
            // For now, return true to indicate we have tokens
            return await Task.FromResult(true);
        }
    }
}
