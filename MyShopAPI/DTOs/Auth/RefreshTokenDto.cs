using System.ComponentModel.DataAnnotations;

namespace MyShopAPI.DTOs.Auth
{
    /// <summary>
    /// DTO for refresh token requests.
    /// </summary>
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
