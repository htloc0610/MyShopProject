namespace MyShop.Services.Auth
{
    /// <summary>
    /// Interface for secure credential storage using Windows Credential Manager.
    /// </summary>
    public interface ICredentialService
    {
        /// <summary>
        /// Store the access token securely.
        /// </summary>
        void SaveAccessToken(string token);

        /// <summary>
        /// Retrieve the stored access token.
        /// </summary>
        string? GetAccessToken();

        /// <summary>
        /// Store the refresh token securely.
        /// </summary>
        void SaveRefreshToken(string token);

        /// <summary>
        /// Retrieve the stored refresh token.
        /// </summary>
        string? GetRefreshToken();

        /// <summary>
        /// Clear all stored credentials (for logout).
        /// </summary>
        void ClearCredentials();

        /// <summary>
        /// Check if user has stored credentials.
        /// </summary>
        bool HasStoredCredentials();
    }
}
