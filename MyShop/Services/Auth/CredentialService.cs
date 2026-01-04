using Windows.Security.Credentials;

namespace MyShop.Services.Auth
{
    /// <summary>
    /// Implementation of ICredentialService using Windows PasswordVault.
    /// Provides secure storage for JWT tokens in Windows Credential Manager.
    /// </summary>
    public class CredentialService : ICredentialService
    {
        private const string ResourceName = "MyShopApp";
        private const string AccessTokenKey = "AccessToken";
        private const string RefreshTokenKey = "RefreshToken";

        private readonly PasswordVault _vault;

        public CredentialService()
        {
            _vault = new PasswordVault();
        }

        /// <inheritdoc />
        public void SaveAccessToken(string token)
        {
            try
            {
                // Remove existing token first
                RemoveCredential(AccessTokenKey);
            }
            catch { /* Ignore if not found */ }

            var credential = new PasswordCredential(ResourceName, AccessTokenKey, token);
            _vault.Add(credential);
        }

        /// <inheritdoc />
        public string? GetAccessToken()
        {
            try
            {
                var credential = _vault.Retrieve(ResourceName, AccessTokenKey);
                credential.RetrievePassword();
                return credential.Password;
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc />
        public void SaveRefreshToken(string token)
        {
            try
            {
                // Remove existing token first
                RemoveCredential(RefreshTokenKey);
            }
            catch { /* Ignore if not found */ }

            var credential = new PasswordCredential(ResourceName, RefreshTokenKey, token);
            _vault.Add(credential);
        }

        /// <inheritdoc />
        public string? GetRefreshToken()
        {
            try
            {
                var credential = _vault.Retrieve(ResourceName, RefreshTokenKey);
                credential.RetrievePassword();
                return credential.Password;
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc />
        public void ClearCredentials()
        {
            try
            {
                RemoveCredential(AccessTokenKey);
            }
            catch { /* Ignore if not found */ }

            try
            {
                RemoveCredential(RefreshTokenKey);
            }
            catch { /* Ignore if not found */ }
        }

        /// <inheritdoc />
        public bool HasStoredCredentials()
        {
            try
            {
                var credentials = _vault.FindAllByResource(ResourceName);
                return credentials.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Remove a specific credential from the vault.
        /// </summary>
        private void RemoveCredential(string userName)
        {
            try
            {
                var credential = _vault.Retrieve(ResourceName, userName);
                _vault.Remove(credential);
            }
            catch
            {
                // Ignore if credential doesn't exist
            }
        }
    }
}
