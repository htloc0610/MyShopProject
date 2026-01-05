using System;
using System.Diagnostics;
using MyShop.Models.Auth;

namespace MyShop.Services.Auth
{
    /// <summary>
    /// Implementation of ICredentialService using Windows PasswordVault.
    /// Securely stores tokens in the Windows Credential Locker.
    /// </summary>
    public class CredentialService : ICredentialService
    {
        private const string VaultResource = "MyShop_Auth";
        private const string AccessTokenKey = "AccessToken";
        private const string RefreshTokenKey = "RefreshToken";

        public CredentialService()
        {
            Debug.WriteLine("=== CredentialService: Initialized (PasswordVault) ===");
        }

        public void SaveAccessToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return;
            SaveCredential(AccessTokenKey, token);
        }

        public string? GetAccessToken()
        {
            return GetCredential(AccessTokenKey);
        }

        public void SaveRefreshToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return;
            SaveCredential(RefreshTokenKey, token);
        }

        public string? GetRefreshToken()
        {
            return GetCredential(RefreshTokenKey);
        }

        public void ClearCredentials()
        {
            try
            {
                var vault = new Windows.Security.Credentials.PasswordVault();
                var creds = vault.FindAllByResource(VaultResource);
                foreach (var c in creds)
                {
                    vault.Remove(c);
                }
                Debug.WriteLine("=== CredentialService: All credentials cleared ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== CredentialService: Error clearing credentials: {ex.Message} ===");
            }
        }

        public bool HasStoredCredentials()
        {
            return !string.IsNullOrEmpty(GetAccessToken()) && !string.IsNullOrEmpty(GetRefreshToken());
        }

        private void SaveCredential(string userName, string password)
        {
            try
            {
                var vault = new Windows.Security.Credentials.PasswordVault();
                var cred = new Windows.Security.Credentials.PasswordCredential(VaultResource, userName, password);
                vault.Add(cred);
                Debug.WriteLine($"=== CredentialService: Saved {userName} ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== CredentialService: Error saving {userName}: {ex.Message} ===");
            }
        }

        private string? GetCredential(string userName)
        {
            try
            {
                var vault = new Windows.Security.Credentials.PasswordVault();
                var cred = vault.Retrieve(VaultResource, userName);
                cred.RetrievePassword();
                return cred.Password;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                // Credential not found (Element not found), this is expected.
                return null;
            }
            catch
            {
                // Other errors
                return null;
            }
        }
    }
}
