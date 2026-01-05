using System.Diagnostics;

namespace MyShop.Services.Auth
{
    /// <summary>
    /// Implementation of ICredentialService using in-memory storage.
    /// For production, use Windows PasswordVault (but needs proper capability setup).
    /// </summary>
    public class CredentialService : ICredentialService
    {
        // In-memory storage (simple approach for now)
        private string? _accessToken;
        private string? _refreshToken;

        public CredentialService()
        {
            Debug.WriteLine("=== CredentialService: Created (in-memory storage) ===");
        }

        /// <inheritdoc />
        public void SaveAccessToken(string token)
        {
            _accessToken = token;
            Debug.WriteLine($"=== CredentialService: AccessToken saved (length: {token?.Length}) ===");
        }

        /// <inheritdoc />
        public string? GetAccessToken()
        {
            Debug.WriteLine($"=== CredentialService: GetAccessToken called, has token: {_accessToken != null} ===");
            return _accessToken;
        }

        /// <inheritdoc />
        public void SaveRefreshToken(string token)
        {
            _refreshToken = token;
            Debug.WriteLine($"=== CredentialService: RefreshToken saved ===");
        }

        /// <inheritdoc />
        public string? GetRefreshToken()
        {
            return _refreshToken;
        }

        /// <inheritdoc />
        public void ClearCredentials()
        {
            _accessToken = null;
            _refreshToken = null;
            Debug.WriteLine("=== CredentialService: Credentials cleared ===");
        }

        /// <inheritdoc />
        public bool HasStoredCredentials()
        {
            return !string.IsNullOrEmpty(_accessToken);
        }
    }
}
