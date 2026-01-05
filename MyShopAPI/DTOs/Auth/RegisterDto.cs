using System.ComponentModel.DataAnnotations;

namespace MyShopAPI.DTOs.Auth
{
    /// <summary>
    /// DTO for user registration.
    /// </summary>
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// Optional role: "Owner" or "Staff". Defaults to "Owner".
        /// </summary>
        public string? Role { get; set; }
    }
}
