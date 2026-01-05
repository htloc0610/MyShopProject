namespace MyShopAPI.DTOs.Auth
{
    /// <summary>
    /// DTO for authentication response with tokens.
    /// </summary>
    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserInfoDto User { get; set; } = null!;
    }

    /// <summary>
    /// DTO for user information in auth responses.
    /// </summary>
    public class UserInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
