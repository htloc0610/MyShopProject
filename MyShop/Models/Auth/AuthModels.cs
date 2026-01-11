using System;

namespace MyShop.Models.Auth
{
    /// <summary>
    /// Result of authentication operations.
    /// </summary>
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public UserInfo? User { get; set; }
        public AccountStatusInfo? AccountStatus { get; set; }
    }

    /// <summary>
    /// User information from authentication.
    /// </summary>
    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// Token response from the API.
    /// </summary>
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserInfo User { get; set; } = null!;
        public AccountStatusInfo AccountStatus { get; set; } = null!;
    }

    /// <summary>
    /// Login request DTO.
    /// </summary>
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Register request DTO.
    /// </summary>
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string? Role { get; set; }
    }

    /// <summary>
    /// Refresh token request DTO.
    /// </summary>
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Account status enum matching the backend.
    /// </summary>
    public enum AccountStatus
    {
        Active,
        Trial,
        Expired
    }

    /// <summary>
    /// Account status information for trial/licensing.
    /// </summary>
    public class AccountStatusInfo
    {
        public AccountStatus Status { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsLicensed { get; set; }
    }

    /// <summary>
    /// Request to activate account with license key.
    /// </summary>
    public class ActivationRequest
    {
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result of activation attempt.
    /// </summary>
    public class ActivationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
